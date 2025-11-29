using ClosedXML.Excel;
using FLS_API.DL;
using FLS_API.DL.Models;
using Supabase.Postgrest.Models; // <--- FIXES THE BaseModel ERROR
using System.Globalization;
using System.Linq;

namespace FLS_API.BL
{
    public class TimetableService : ITimetableService
    {
        private readonly SupabaseService _supabase;
        
        // Valid enum values for the week_day database constraint
        private static readonly HashSet<string> ValidDays = new(StringComparer.OrdinalIgnoreCase)
        {
            "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"
        };

        public TimetableService(SupabaseService supabase)
        {
            _supabase = supabase;
        }

        public async Task ParseAndSaveTimetableAsync(Stream fileStream)
        {
            // ---------------------------------------------------------
            // 1. CLEANUP PHASE
            // ---------------------------------------------------------
            await _supabase.Client.From<TimeTable>()
                .Filter("id", Supabase.Postgrest.Constants.Operator.NotEqual, Guid.Empty.ToString())
                .Delete();

            await _supabase.Client.From<Section>()
                .Filter("id", Supabase.Postgrest.Constants.Operator.NotEqual, Guid.Empty.ToString())
                .Delete();

            // ---------------------------------------------------------
            // 2. PREPARATION & CACHING
            // ---------------------------------------------------------
            var dbCourses = await _supabase.Client.From<Course>().Get();
            
            // Cache by Code (safe/unique key)
            var courseCache = dbCourses.Models.ToDictionary(
                c => c.Code.Trim().ToUpper(), 
                c => c
            );

            var newCourses = new List<Course>();
            var newSections = new Dictionary<string, Section>();
            var newTimetables = new List<TimeTable>();

            using var workbook = new XLWorkbook(fileStream);

            // ---------------------------------------------------------
            // 3. EXCEL PARSING LOOP
            // ---------------------------------------------------------
            foreach (var sheet in workbook.Worksheets)
            {
                // Skip Grid/Visual sheets
                if (sheet.Name.Contains("Combined") || sheet.Name.Contains("Grid") || sheet.Name.Contains("TimeTable"))
                    continue;

                // Find Header
                var headerRow = sheet.RowsUsed()
                    .FirstOrDefault(r => r.CellsUsed()
                        .Any(c => c.Value.ToString().Trim().Equals("Course Title", StringComparison.OrdinalIgnoreCase))
                     || r.CellsUsed()
                        .Any(c => c.Value.ToString().Trim().Equals("Duration in Minutes", StringComparison.OrdinalIgnoreCase))
                    );

                if (headerRow == null) continue;

                // Map Columns
                var colMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                foreach (var cell in headerRow.CellsUsed())
                {
                    string key = cell.Value.ToString().Trim();
                    if (!colMap.ContainsKey(key)) colMap[key] = cell.Address.ColumnNumber;
                }

                // Check Sheet Type
                bool isProgramSheet = colMap.ContainsKey("Lecture Day") && colMap.ContainsKey("Lecture Start Time");

                // Parse Rows
                var dataRows = sheet.RowsUsed().Where(r => r.RowNumber() > headerRow.RowNumber());

                foreach (var row in dataRows)
                {
                    if (IsBatchHeaderOrEmpty(row, colMap)) continue;

                    string courseCode = GetCellString(row, colMap, "Code");
                    string courseName = GetCellString(row, colMap, "Course Title");
                    string sectionName = GetCellString(row, colMap, "Section");
                    string instructorName = GetCellString(row, colMap, "Instructor Name") ?? GetCellString(row, colMap, "Instructor");

                    if (string.IsNullOrWhiteSpace(courseCode) || string.IsNullOrWhiteSpace(courseName)) continue;

                    // Duration
                    int duration = 80;
                    if (colMap.ContainsKey("Duration in Minutes"))
                    {
                        string dStr = GetCellString(row, colMap, "Duration in Minutes");
                        if (int.TryParse(dStr, out int d)) duration = d;
                    }

                    // ---- A. COURSE ----
                    string courseKey = courseCode.Trim().ToUpper();
                    if (!courseCache.TryGetValue(courseKey, out var courseObj))
                    {
                        courseObj = new Course
                        {
                            Id = Guid.NewGuid(),
                            Name = courseName,
                            Code = courseCode,
                            Description = "Imported"
                        };
                        courseCache[courseKey] = courseObj;
                        newCourses.Add(courseObj);
                    }

                    // ---- B. SECTION ----
                    string sectionKey = $"{courseObj.Id}-{sectionName}".ToLower();
                    if (!newSections.TryGetValue(sectionKey, out var sectionObj))
                    {
                        sectionObj = new Section
                        {
                            Id = Guid.NewGuid(),
                            CourseId = courseObj.Id,
                            Name = sectionName,
                            InstructorName = instructorName
                        };
                        newSections[sectionKey] = sectionObj;
                    }

                    // ---- C. TIMETABLE ----
                    if (isProgramSheet)
                    {
                        ProcessSlot(row, colMap, "Lecture Day", "Lecture Start Time", "Room",
                            duration, newTimetables, courseObj, sectionObj, courseName);
                    }
                    else
                    {
                        ProcessSlot(row, colMap, "Day 1", "Slot 1", "Venue 1",
                            duration, newTimetables, courseObj, sectionObj, courseName);

                        ProcessSlot(row, colMap, "Day 2", "Slot 2", "Venue 2",
                            duration, newTimetables, courseObj, sectionObj, courseName);
                    }
                }
            }

            // ---------------------------------------------------------
            // 4. BATCH SAVE
            // ---------------------------------------------------------
            if (newCourses.Any()) await BatchInsert(newCourses);
            if (newSections.Any()) await BatchInsert(newSections.Values.ToList());
            if (newTimetables.Any()) await BatchInsert(newTimetables);
        }

        public async Task<List<TimeTable>> GetTimetableAsync()
        {
            var response = await _supabase.Client.From<TimeTable>().Get();
            return response.Models;
        }

        // ==========================================
        // HELPERS
        // ==========================================

        private bool IsBatchHeaderOrEmpty(IXLRow row, Dictionary<string, int> colMap)
        {
            if (row.CellsUsed().Count() < 3) return true;
            if (colMap.ContainsKey("Code") && row.Cell(colMap["Code"]).IsEmpty()) return true;

            var firstCell = row.Cell(1).Value.ToString();
            if (firstCell.Contains("BS(") || firstCell.Contains("MS(")) return true;

            return false;
        }

        private string GetCellString(IXLRow row, Dictionary<string, int> map, string colName)
        {
            if (!map.ContainsKey(colName)) return null;
            return row.Cell(map[colName]).Value.ToString().Trim();
        }

        private void ProcessSlot(IXLRow row, Dictionary<string, int> colMap, 
            string dayKey, string slotKey, string venueKey, int duration, 
            List<TimeTable> list, Course course, Section section, string courseName)
        {
            if (!colMap.ContainsKey(dayKey) || !colMap.ContainsKey(slotKey)) return;

            string day = row.Cell(colMap[dayKey]).Value.ToString().Trim();
            var timeCell = row.Cell(colMap[slotKey]);
            if (timeCell.IsEmpty()) return;

            string startStr = GetSafeTime(timeCell);
            string room = colMap.ContainsKey(venueKey) ? row.Cell(colMap[venueKey]).Value.ToString().Trim() : "";

            if (string.IsNullOrWhiteSpace(day) || string.IsNullOrWhiteSpace(startStr)) return;
            
            // Validate day against database enum constraint
            if (!ValidDays.Contains(day)) return;

            string timeRange = ComputeTimeRange(startStr, duration);

            list.Add(new TimeTable
            {
                Id = Guid.NewGuid(),
                CourseId = course.Id,
                SectionId = section.Id,
                Day = day,
                Time = timeRange,
                Subject = courseName,
                Room = room
            });
        }

        private string GetSafeTime(IXLCell cell)
        {
            if (cell.DataType == XLDataType.DateTime)
                return cell.GetDateTime().ToString("HH:mm");
            
            if (cell.DataType == XLDataType.Number)
                return DateTime.FromOADate(cell.GetDouble()).ToString("HH:mm");
            
            return cell.GetString().Trim();
        }

        private string ComputeTimeRange(string start, int duration)
        {
            string[] formats = { "HH:mm", "H:mm", "h:mm tt", "HH:mm:ss" };
            if (DateTime.TryParseExact(start, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var s))
            {
                var e = s.AddMinutes(duration);
                return $"{s:HH:mm}-{e:HH:mm}";
            }
            return start;
        }

        private async Task BatchInsert<T>(List<T> items) where T : BaseModel, new()
        {
            const int BATCH_SIZE = 500;
            for (int i = 0; i < items.Count; i += BATCH_SIZE)
            {
                var batch = items.Skip(i).Take(BATCH_SIZE).ToList();
                await _supabase.Client.From<T>().Insert(batch);
            }
        }
    }
}