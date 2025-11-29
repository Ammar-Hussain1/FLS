using ClosedXML.Excel;
using FLS_API.DL;
using FLS_API.DL.Models;
using System.Text.RegularExpressions;
using System.Linq;

namespace FLS_API.BL
{
    public class TimetableService : ITimetableService
    {
        private readonly SupabaseService _supabase;

        public TimetableService(SupabaseService supabase)
        {
            _supabase = supabase;
        }

        public async Task<List<TimetableEntry>> GetTimetableAsync()
        {
            var response = await _supabase.Client.From<TimetableEntry>().Get();
            return response.Models;
        }

        public async Task ParseAndSaveTimetableAsync(Stream fileStream)
        {
            using var workbook = new XLWorkbook(fileStream);
            
            // Find the "Combined TT" sheet or fallback to first sheet
            var sheet = workbook.Worksheets.FirstOrDefault(ws => ws.Name.Contains("Combined TT")) 
                     ?? workbook.Worksheets.First();

            var entries = new List<TimetableEntry>();
            
            // Define time slots mapping (Column Index -> Time Slot)
            var timeSlotMapping = new Dictionary<int, string>();
            // Find header row dynamically (scan first 10 rows)
            IXLRow headerRow = null;
            for (int r = 1; r <= 10; r++)
            {
                var row = sheet.Row(r);
                if (row.CellsUsed().Any(c => Regex.IsMatch(c.Value.ToString(), @"\d{2}:\d{2}-\d{2}:\d{2}")))
                {
                    headerRow = row;
                    break;
                }
            }

            if (headerRow == null)
            {
                // Fallback to Row 3 if not found
                headerRow = sheet.Row(3);
            }
            
            // Iterate columns to find time slots
            foreach (var cell in headerRow.CellsUsed())
            {
                var value = cell.Value.ToString();
                // Match time format like 08:30-10:00
                if (Regex.IsMatch(value, @"\d{2}:\d{2}-\d{2}:\d{2}"))
                {
                    // If merged, use the start column index
                    var colIndex = cell.Address.ColumnNumber;
                    timeSlotMapping[colIndex] = value;
                }
            }

            // Start reading data from next row
            var rowStart = headerRow.RowNumber() + 1;
            var rowEnd = sheet.LastRowUsed().RowNumber();
            
            string currentDay = "";

            for (int r = rowStart; r <= rowEnd; r++)
            {
                var row = sheet.Row(r);
                
                // Read Day (Column A) - handle merged cells
                var dayCell = row.Cell(1);
                if (dayCell.IsMerged())
                {
                    currentDay = dayCell.MergedRange().FirstCell().Value.ToString();
                }
                else if (!dayCell.IsEmpty())
                {
                    currentDay = dayCell.Value.ToString();
                }

                // Read Room (Column B)
                var room = row.Cell(2).Value.ToString();
                if (string.IsNullOrWhiteSpace(room)) continue; // Skip empty rows

                // Iterate time slots
                foreach (var kvp in timeSlotMapping)
                {
                    var colIndex = kvp.Key;
                    var timeSlot = kvp.Value;
                    
                    var cell = row.Cell(colIndex);
                    
                    // Handle merged cells for course data (e.g. 3 hour labs)
                    string cellValue;
                    if (cell.IsMerged())
                    {
                        cellValue = cell.MergedRange().FirstCell().Value.ToString().Trim();
                    }
                    else
                    {
                        cellValue = cell.Value.ToString().Trim();
                    }
                    
                    if (!string.IsNullOrWhiteSpace(cellValue))
                    {
                        // Parse "Course (Section): Instructor"
                        // Regex: ^(.*?) \((.*?)\): (.*)$
                        var match = Regex.Match(cellValue, @"^(.*?) \((.*?)\): (.*)$");
                        
                        if (match.Success)
                        {
                            entries.Add(new TimetableEntry
                            {
                                Day = currentDay,
                                Room = room,
                                TimeSlot = timeSlot,
                                CourseName = match.Groups[1].Value.Trim(),
                                Section = match.Groups[2].Value.Trim(),
                                Instructor = match.Groups[3].Value.Trim(),
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                        else
                        {
                            // Fallback for non-standard format
                            entries.Add(new TimetableEntry
                            {
                                Day = currentDay,
                                Room = room,
                                TimeSlot = timeSlot,
                                CourseName = cellValue, // Store raw value
                                Section = "",
                                Instructor = "",
                                CreatedAt = DateTime.UtcNow
                            });
                        }
                    }
                }
            }

            // Save to Supabase
            if (entries.Any())
            {
                // Delete all existing entries before inserting new ones
                // Using a condition that is always true (Id > -1) to simulate Truncate
                await _supabase.Client.From<TimetableEntry>()
                    .Where(x => x.Id > -1)
                    .Delete();
                
                // Chunk inserts to avoid payload limits
                var batchSize = 100;
                for (int i = 0; i < entries.Count; i += batchSize)
                {
                    var chunk = entries.Skip(i).Take(batchSize).ToList();
                    await _supabase.Client.From<TimetableEntry>().Insert(chunk);
                }
            }
        }
    }
}
