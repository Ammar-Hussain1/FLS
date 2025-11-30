using FLS_API.DL.Models;
using FLS_API.DL.DTOs;
using FLS_API.DTOs;
using FLS_API.Models;
using Supabase;
using Supabase.Realtime;
using System.Text;

namespace FLS_API.BL
{
    public class SupabaseService
    {
        private readonly Supabase.Client _client;

        public SupabaseService(Models.SupabaseOptions options)
        {
            if (string.IsNullOrEmpty(options.Url))
                throw new ArgumentException("Supabase URL is required", nameof(options));
            
            var apiKey = !string.IsNullOrEmpty(options.AnonKey) ? options.AnonKey : options.ServiceRoleKey;
            
            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentException("Supabase API key (AnonKey or ServiceRoleKey) is required", nameof(options));

            _client = new Supabase.Client(
                options.Url,
                apiKey
            );
            
            _client.InitializeAsync().GetAwaiter().GetResult();
        }

        public Supabase.Client Client => _client;

        public async Task<ServiceResult<UserResponseDTO>> SignUpUserAsync(string fullName, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email))
                return ServiceResult<UserResponseDTO>.Failure("Email cannot be null or empty.", "INVALID_EMAIL");
            
            if (string.IsNullOrWhiteSpace(password))
                return ServiceResult<UserResponseDTO>.Failure("Password cannot be null or empty.", "INVALID_PASSWORD");
            
            if (string.IsNullOrWhiteSpace(fullName))
                return ServiceResult<UserResponseDTO>.Failure("Full name cannot be null or empty.", "INVALID_FULLNAME");

            try
            {
                var authResult = await _client.Auth.SignUp(email, password);

                if (authResult.User == null)
                {
                    return ServiceResult<UserResponseDTO>.Failure("Unable to create Supabase Auth user.", "AUTH_CREATION_FAILED");
                }

                var user = new User
                {
                    Id = authResult.User.Id,
                    FullName = fullName.Trim(),
                    Email = email.Trim().ToLowerInvariant(),
                    Role = "User",
                    CreatedAt = DateTime.UtcNow
                };
                
                try
                {
                    await _client.From<User>().Insert(user);
                }
                catch (Exception ex)
                {
                    return ServiceResult<UserResponseDTO>.Failure($"Failed to insert user into database: {ex.Message}", "DATABASE_INSERT_FAILED");
                }
                
                return ServiceResult<UserResponseDTO>.Success(new UserResponseDTO
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt
                });
            }
            catch (Exception ex)
            {
                return ServiceResult<UserResponseDTO>.Failure(ex.Message, "SIGNUP_FAILED");
            }
        }

        public async Task<ServiceResult<UserResponseDTO>> SignInUserAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email))
                return ServiceResult<UserResponseDTO>.Failure("Email cannot be null or empty.", "INVALID_EMAIL");
            
            if (string.IsNullOrWhiteSpace(password))
                return ServiceResult<UserResponseDTO>.Failure("Password cannot be null or empty.", "INVALID_PASSWORD");

            try
            {
                var session = await _client.Auth.SignIn(email, password);
                if (session.User == null)
                {
                    return ServiceResult<UserResponseDTO>.Failure("Invalid email or password.", "INVALID_CREDENTIALS");
                }

                var user = await GetUserByEmailAsync(email);
                if (user == null)
                {
                    return ServiceResult<UserResponseDTO>.Failure("User not found in database.", "USER_NOT_FOUND");
                }

                return ServiceResult<UserResponseDTO>.Success(new UserResponseDTO
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt
                });
            }
            catch (Exception ex)
            {
                return ServiceResult<UserResponseDTO>.Failure($"Sign in failed: {ex.Message}", "SIGNIN_FAILED");
            }
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            try
            {
                var response = await _client.From<User>()
                    .Where(u => u.Email == email)
                    .Get();
                return response.Models.FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<UserResponseDTO>> GetAllUsersAsync()
        {
            var response = await _client.From<User>().Get();
            return response.Models.Select(u => new UserResponseDTO
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role,
                CreatedAt = u.CreatedAt
            }).ToList();
        }

        public async Task<UserResponseDTO?> GetUserByIdAsync(string id)
        {
            try
            {
                var response = await _client.From<User>()
                    .Where(u => u.Id == id)
                    .Get();
                var user = response.Models.FirstOrDefault();
                
                if (user == null)
                {
                    return null;
                }
                
                return new UserResponseDTO
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt
                };
            }
            catch
            {
                return null;
            }
        }

        public async Task<ServiceResult<UserResponseDTO>> UpdateUserAsync(string id, UpdateUserDTO dto)
        {
            if (string.IsNullOrWhiteSpace(id))
                return ServiceResult<UserResponseDTO>.Failure("User ID cannot be null or empty.", "INVALID_ID");

            try
            {
                var existing = await _client.From<User>().Where(u => u.Id == id).Get();
                var user = existing.Models.FirstOrDefault();
                if (user == null)
                {
                    return ServiceResult<UserResponseDTO>.Failure("User not found.", "USER_NOT_FOUND");
                }

                if (!string.IsNullOrWhiteSpace(dto.FullName))
                {
                    user.FullName = dto.FullName.Trim();
                }

                if (!string.IsNullOrWhiteSpace(dto.Role))
                {
                    var roleLower = dto.Role.Trim().ToLowerInvariant();
                    if (roleLower == "user" || roleLower == "admin" || roleLower == "User" || roleLower == "Admin")
                    {
                        user.Role = dto.Role.Trim();
                    }
                    else
                    {
                        return ServiceResult<UserResponseDTO>.Failure($"Invalid role value. Must be 'user' or 'admin', got: {dto.Role}", "INVALID_ROLE");
                    }
                }

                await _client.From<User>().Update(user);
                
                return ServiceResult<UserResponseDTO>.Success(new UserResponseDTO
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    Role = user.Role,
                    CreatedAt = user.CreatedAt
                });
            }
            catch (Exception ex)
            {
                return ServiceResult<UserResponseDTO>.Failure($"Update failed: {ex.Message}", "UPDATE_FAILED");
            }
        }

        public async Task<ServiceResult<List<UserCourseDTO>>> GetUserCoursesAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return ServiceResult<List<UserCourseDTO>>.Failure("User ID cannot be null or empty.", "INVALID_USER_ID");

            try
            {
                var userCoursesResponse = await _client.From<UserCourseModel>()
                    .Where(uc => uc.UserId == userId)
                    .Get();

                if (!userCoursesResponse.Models.Any())
                {
                    return ServiceResult<List<UserCourseDTO>>.Success(new List<UserCourseDTO>());
                }

                var courseIds = userCoursesResponse.Models.Select(uc => uc.CourseId).Distinct().ToHashSet();
                
                var allCoursesResponse = await _client.From<Course>().Get();
                var courses = allCoursesResponse.Models
                    .Where(c => courseIds.Contains(c.Id))
                    .ToList();

                var result = userCoursesResponse.Models
                    .Join(courses,
                        uc => uc.CourseId,
                        c => c.Id,
                        (uc, c) => new UserCourseDTO
                        {
                            CourseId = c.Id,
                            CourseCode = c.Code,
                            CourseName = c.Name,
                            Description = c.Description
                        })
                    .DistinctBy(c => c.CourseId)
                    .ToList();

                return ServiceResult<List<UserCourseDTO>>.Success(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<UserCourseDTO>>.Failure($"Failed to get user courses: {ex.Message}", "GET_COURSES_FAILED");
            }
        }

        public async Task<ServiceResult<CourseWithMaterialsDTO>> GetCourseMaterialsAsync(string courseId, string? category = null)
        {
            if (string.IsNullOrWhiteSpace(courseId))
                return ServiceResult<CourseWithMaterialsDTO>.Failure("Course ID cannot be null or empty.", "INVALID_COURSE_ID");

            try
            {
                var courseResponse = await _client.From<Course>()
                    .Where(c => c.Id == courseId)
                    .Get();

                var course = courseResponse.Models.FirstOrDefault();
                if (course == null)
                {
                    return ServiceResult<CourseWithMaterialsDTO>.Failure("Course not found.", "COURSE_NOT_FOUND");
                }

                var materialsQuery = _client.From<CourseMaterial>()
                    .Where(m => m.CourseId == courseId && m.Status == "Approved");

                if (!string.IsNullOrWhiteSpace(category))
                {
                    materialsQuery = materialsQuery.Where(m => m.Category == category);
                }

                var materialsResponse = await materialsQuery.Get();
                var materials = materialsResponse.Models.ToList();
                
                // Fetch user names if needed
                var userIds = materials
                    .Where(m => !string.IsNullOrEmpty(m.UploadedBy))
                    .Select(m => m.UploadedBy!)
                    .Distinct()
                    .ToHashSet();
                
                var usersDict = new Dictionary<string, string>();
                if (userIds.Any())
                {
                    var usersResponse = await _client.From<User>().Get();
                    usersDict = usersResponse.Models
                        .Where(u => userIds.Contains(u.Id))
                        .ToDictionary(u => u.Id, u => u.FullName);
                }
                
                var materialsDto = materials.Select(m => new MaterialResponseDTO
                {
                    Id = m.Id,
                    CourseId = m.CourseId,
                    CourseName = course.Name,
                    Title = m.Title,
                    Category = m.Category,
                    FilePath = m.FilePath,
                    Year = m.Year,
                    Status = m.Status,
                    UploadedAt = m.UploadedAt,
                    UploadedBy = m.UploadedBy,
                    UploadedByName = !string.IsNullOrEmpty(m.UploadedBy) 
                        ? usersDict.GetValueOrDefault(m.UploadedBy, "Unknown User") 
                        : null
                }).ToList();

                var materialsByCategory = materialsDto
                    .GroupBy(m => m.Category)
                    .ToDictionary(g => g.Key, g => g.OrderByDescending(m => m.Year ?? 0).ThenBy(m => m.Title).ToList());

                var result = new CourseWithMaterialsDTO
                {
                    CourseId = course.Id,
                    CourseCode = course.Code,
                    CourseName = course.Name,
                    MaterialsByCategory = materialsByCategory
                };

                return ServiceResult<CourseWithMaterialsDTO>.Success(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<CourseWithMaterialsDTO>.Failure($"Failed to get course materials: {ex.Message}", "GET_MATERIALS_FAILED");
            }
        }

        public async Task<ServiceResult<MaterialResponseDTO>> GetMaterialByIdAsync(string materialId)
        {
            if (string.IsNullOrWhiteSpace(materialId))
                return ServiceResult<MaterialResponseDTO>.Failure("Material ID cannot be null or empty.", "INVALID_MATERIAL_ID");

            try
            {
                var materialResponse = await _client.From<CourseMaterial>()
                    .Where(m => m.Id == materialId && m.Status == "Approved")
                    .Get();

                var material = materialResponse.Models.FirstOrDefault();
                if (material == null)
                {
                    return ServiceResult<MaterialResponseDTO>.Failure("Material not found or not approved.", "MATERIAL_NOT_FOUND");
                }

                var courseResponse = await _client.From<Course>()
                    .Where(c => c.Id == material.CourseId)
                    .Get();
                var course = courseResponse.Models.FirstOrDefault();

                // Fetch user name
                string? userName = null;
                if (!string.IsNullOrEmpty(material.UploadedBy))
                {
                    var userResponse = await _client.From<User>()
                        .Where(u => u.Id == material.UploadedBy)
                        .Get();
                    userName = userResponse.Models.FirstOrDefault()?.FullName ?? "Unknown User";
                }

                var result = new MaterialResponseDTO
                {
                    Id = material.Id,
                    CourseId = material.CourseId,
                    CourseName = course?.Name ?? string.Empty,
                    Title = material.Title,
                    Category = material.Category,
                    FilePath = material.FilePath,
                    Year = material.Year,
                    Status = material.Status,
                    UploadedAt = material.UploadedAt,
                    UploadedBy = material.UploadedBy,
                    UploadedByName = userName
                };

                return ServiceResult<MaterialResponseDTO>.Success(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<MaterialResponseDTO>.Failure($"Failed to get material: {ex.Message}", "GET_MATERIAL_FAILED");
            }
        }

        public async Task<ServiceResult<MaterialResponseDTO>> RequestMaterialAsync(string userId, MaterialRequestDTO request)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return ServiceResult<MaterialResponseDTO>.Failure("User ID cannot be null or empty.", "INVALID_USER_ID");

            if (string.IsNullOrWhiteSpace(request.FileLink))
                return ServiceResult<MaterialResponseDTO>.Failure("File link is required.", "INVALID_FILE_LINK");

            if (string.IsNullOrWhiteSpace(request.CourseName))
                return ServiceResult<MaterialResponseDTO>.Failure("Course name is required.", "INVALID_COURSE_NAME");

            if (string.IsNullOrWhiteSpace(request.FileType))
                return ServiceResult<MaterialResponseDTO>.Failure("File type is required.", "INVALID_FILE_TYPE");

            var validCategories = new[] { "assignments", "books", "course outline", "quizzes", "Midterm 1", "Midterm 2", "Final" };
            if (!validCategories.Contains(request.FileType, StringComparer.OrdinalIgnoreCase))
            {
                return ServiceResult<MaterialResponseDTO>.Failure($"Invalid file type. Must be one of: {string.Join(", ", validCategories)}", "INVALID_FILE_TYPE");
            }

            var linkLower = request.FileLink.ToLowerInvariant();
            if (!linkLower.Contains(".pdf") && !linkLower.Contains("pdf"))
            {
                return ServiceResult<MaterialResponseDTO>.Failure("Only PDF files are allowed. Please ensure the file link points to a PDF document.", "INVALID_FILE_FORMAT");
            }

            try
            {
                var courseResponse = await _client.From<Course>()
                    .Where(c => c.Name == request.CourseName || c.Code == request.CourseName)
                    .Get();

                var course = courseResponse.Models.FirstOrDefault();
                if (course == null)
                {
                    return ServiceResult<MaterialResponseDTO>.Failure("Course not found.", "COURSE_NOT_FOUND");
                }

                var title = ExtractTitleFromLink(request.FileLink) ?? $"Material - {request.FileType}";

                // Fetch user name
                var userResponse = await _client.From<User>()
                    .Where(u => u.Id == userId)
                    .Get();
                var userName = userResponse.Models.FirstOrDefault()?.FullName ?? "Unknown User";

                var material = new CourseMaterial
                {
                    Id = Guid.NewGuid().ToString(),
                    CourseId = course.Id,
                    Title = title,
                    Category = request.FileType,
                    FilePath = request.FileLink,
                    Year = request.Year,
                    Status = "Pending",
                    UploadedAt = DateTime.UtcNow,
                    UploadedBy = userId
                };

                await _client.From<CourseMaterial>().Insert(material);

                var result = new MaterialResponseDTO
                {
                    Id = material.Id,
                    CourseId = material.CourseId,
                    CourseName = course.Name,
                    Title = material.Title,
                    Category = material.Category,
                    FilePath = material.FilePath,
                    Year = material.Year,
                    Status = material.Status,
                    UploadedAt = material.UploadedAt,
                    UploadedBy = material.UploadedBy,
                    UploadedByName = userName
                };

                return ServiceResult<MaterialResponseDTO>.Success(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<MaterialResponseDTO>.Failure($"Failed to request material: {ex.Message}", "REQUEST_MATERIAL_FAILED");
            }
        }

        public async Task<ServiceResult<List<MaterialResponseDTO>>> GetUserMaterialRequestsAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return ServiceResult<List<MaterialResponseDTO>>.Failure("User ID cannot be null or empty.", "INVALID_USER_ID");

            try
            {
                var materialsResponse = await _client.From<CourseMaterial>()
                    .Where(m => m.UploadedBy == userId)
                    .Get();

                var materials = materialsResponse.Models.ToList();
                
                if (!materials.Any())
                {
                    return ServiceResult<List<MaterialResponseDTO>>.Success(new List<MaterialResponseDTO>());
                }
                
                // Fetch courses
                var allCoursesResponse = await _client.From<Course>().Get();
                var courseIds = materials.Select(m => m.CourseId).Distinct().ToHashSet();
                var courses = allCoursesResponse.Models
                    .Where(c => courseIds.Contains(c.Id))
                    .ToDictionary(c => c.Id, c => c.Name);

                // Fetch user name
                var userResponse = await _client.From<User>()
                    .Where(u => u.Id == userId)
                    .Get();
                var userName = userResponse.Models.FirstOrDefault()?.FullName ?? "Unknown User";

                var result = materials
                    .OrderByDescending(m => m.UploadedAt)
                    .Select(m => new MaterialResponseDTO
                    {
                        Id = m.Id,
                        CourseId = m.CourseId,
                        CourseName = courses.GetValueOrDefault(m.CourseId, string.Empty),
                        Title = m.Title,
                        Category = m.Category,
                        FilePath = m.FilePath,
                        Year = m.Year,
                        Status = m.Status,
                        UploadedAt = m.UploadedAt,
                        UploadedBy = m.UploadedBy,
                        UploadedByName = userName
                    }).ToList();

                return ServiceResult<List<MaterialResponseDTO>>.Success(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<MaterialResponseDTO>>.Failure($"Failed to get material requests: {ex.Message}", "GET_REQUESTS_FAILED");
            }
        }

        public async Task<ServiceResult<List<MaterialResponseDTO>>> GetPendingMaterialRequestsAsync()
        {
            try
            {
                var materialsResponse = await _client.From<CourseMaterial>()
                    .Where(m => m.Status == "Pending")
                    .Get();

                var materials = materialsResponse.Models.ToList();
                
                if (!materials.Any())
                {
                    return ServiceResult<List<MaterialResponseDTO>>.Success(new List<MaterialResponseDTO>());
                }
                
                // Fetch courses
                var allCoursesResponse = await _client.From<Course>().Get();
                var courseIds = materials.Select(m => m.CourseId).Distinct().ToHashSet();
                var courses = allCoursesResponse.Models
                    .Where(c => courseIds.Contains(c.Id))
                    .ToDictionary(c => c.Id, c => c.Name);

                // Fetch users to get names
                var userIds = materials
                    .Where(m => !string.IsNullOrEmpty(m.UploadedBy))
                    .Select(m => m.UploadedBy!)
                    .Distinct()
                    .ToHashSet();
                
                var usersResponse = await _client.From<User>().Get();
                var users = usersResponse.Models
                    .Where(u => userIds.Contains(u.Id))
                    .ToDictionary(u => u.Id, u => u.FullName);

                var result = materials
                    .OrderByDescending(m => m.UploadedAt)
                    .Select(m => new MaterialResponseDTO
                    {
                        Id = m.Id,
                        CourseId = m.CourseId,
                        CourseName = courses.GetValueOrDefault(m.CourseId, string.Empty),
                        Title = m.Title,
                        Category = m.Category,
                        FilePath = m.FilePath,
                        Year = m.Year,
                        Status = m.Status,
                        UploadedAt = m.UploadedAt,
                        UploadedBy = m.UploadedBy,
                        UploadedByName = !string.IsNullOrEmpty(m.UploadedBy) 
                            ? users.GetValueOrDefault(m.UploadedBy, "Unknown User") 
                            : null
                    }).ToList();

                return ServiceResult<List<MaterialResponseDTO>>.Success(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<MaterialResponseDTO>>.Failure($"Failed to get pending requests: {ex.Message}", "GET_PENDING_REQUESTS_FAILED");
            }
        }

        private string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return "file.pdf";

            var extension = Path.GetExtension(fileName);
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            var invalidChars = Path.GetInvalidFileNameChars().Concat(new[] { '`', '[', ']', '{', '}', '(', ')', '&', '#', '%', '!', '@', '$', '^', '*', '+', '=', '|', '\\', '/', ':', ';', '"', '\'', '<', '>', '?', ',' }).ToArray();
            
            var sanitized = new string(nameWithoutExtension
                .Select(c => invalidChars.Contains(c) ? '_' : c)
                .ToArray());

            while (sanitized.Contains("__"))
            {
                sanitized = sanitized.Replace("__", "_");
            }

            sanitized = sanitized.Trim('_');

            if (string.IsNullOrWhiteSpace(sanitized))
                sanitized = "file";

            return sanitized + extension;
        }

        private string SanitizePathSegment(string segment)
        {
            if (string.IsNullOrWhiteSpace(segment))
                return "unknown";

            var invalidChars = Path.GetInvalidFileNameChars().Concat(new[] { '`', '[', ']', '{', '}', '(', ')', '&', '#', '%', '!', '@', '$', '^', '*', '+', '=', '|', '\\', '/', ':', ';', '"', '\'', '<', '>', '?', ',' }).ToArray();
            
            var sanitized = new string(segment
                .Select(c => invalidChars.Contains(c) ? '_' : c)
                .ToArray());

            while (sanitized.Contains("__"))
            {
                sanitized = sanitized.Replace("__", "_");
            }

            sanitized = sanitized.Trim('_');

            if (string.IsNullOrWhiteSpace(sanitized))
                sanitized = "unknown";

            return sanitized;
        }

        public async Task<ServiceResult<string>> UploadFileToStorageAsync(Stream fileStream, string fileName, string contentType, string courseName, string category)
        {
            try
            {
                const string bucketName = "fls";
                
                var sanitizedCourseName = SanitizePathSegment(courseName);
                var sanitizedCategory = SanitizePathSegment(category);
                var sanitizedFileName = SanitizeFileName(fileName);
                
                var filePath = $"{sanitizedCourseName}/{sanitizedCategory}/{sanitizedFileName}";
                
                Console.WriteLine($"[Storage Upload] Uploading to path: {filePath}");

                using var memoryStream = new MemoryStream();
                await fileStream.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();

                await _client.Storage.From(bucketName).Upload(fileBytes, filePath, new Supabase.Storage.FileOptions
                {
                    ContentType = contentType,
                    Upsert = false
                });

                var publicUrl = _client.Storage.From(bucketName).GetPublicUrl(filePath);
                return ServiceResult<string>.Success(publicUrl);
            }
            catch (Exception ex)
            {
                return ServiceResult<string>.Failure($"Failed to upload file to storage: {ex.Message}", "STORAGE_UPLOAD_FAILED");
            }
        }

        public async Task<ServiceResult<Stream>> DownloadFileFromStorageAsync(string filePath)
        {
            try
            {
                const string bucketName = "fls";
                var fileBytes = await _client.Storage.From(bucketName).Download(filePath, (EventHandler<float>?)null);
                
                var stream = new MemoryStream(fileBytes);
                return ServiceResult<Stream>.Success(stream);
            }
            catch (Exception ex)
            {
                return ServiceResult<Stream>.Failure($"Failed to download file from storage: {ex.Message}", "STORAGE_DOWNLOAD_FAILED");
            }
        }

        public async Task<ServiceResult<bool>> DeleteFileFromStorageAsync(string filePath)
        {
            try
            {
                const string bucketName = "fls";
                var paths = new List<string> { filePath };
                await _client.Storage.From(bucketName).Remove(paths);
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.Failure($"Failed to delete file from storage: {ex.Message}", "STORAGE_DELETE_FAILED");
            }
        }

        public async Task<ServiceResult<MaterialResponseDTO>> ApproveMaterialRequestAsync(string materialId)
        {
            if (string.IsNullOrWhiteSpace(materialId))
                return ServiceResult<MaterialResponseDTO>.Failure("Material ID cannot be null or empty.", "INVALID_MATERIAL_ID");

            try
            {
                var materialResponse = await _client.From<CourseMaterial>()
                    .Where(m => m.Id == materialId)
                    .Get();

                var material = materialResponse.Models.FirstOrDefault();
                if (material == null)
                {
                    return ServiceResult<MaterialResponseDTO>.Failure("Material not found.", "MATERIAL_NOT_FOUND");
                }
                var courseResponse = await _client.From<Course>()
                    .Where(c => c.Id == material.CourseId)
                    .Get();
                var course = courseResponse.Models.FirstOrDefault();

                if (course == null)
                {
                    return ServiceResult<MaterialResponseDTO>.Failure("Course not found.", "COURSE_NOT_FOUND");
                }

                var fileExtension = Path.GetExtension(material.Title).ToLowerInvariant();
                if (fileExtension != ".pdf")
                {
                    return ServiceResult<MaterialResponseDTO>.Failure("Only PDF files are allowed. Invalid file extension.", "INVALID_FILE_FORMAT");
                }

                if (string.IsNullOrEmpty(material.FilePath) || !material.FilePath.Contains("supabase.co/storage"))
                {
                    return ServiceResult<MaterialResponseDTO>.Failure("File must be stored in Supabase Storage.", "INVALID_FILE_PATH");
                }

                material.Status = "Approved";
                await _client.From<CourseMaterial>().Update(material);

                // Fetch user name
                string? userName = null;
                if (!string.IsNullOrEmpty(material.UploadedBy))
                {
                    var userResponse = await _client.From<User>()
                        .Where(u => u.Id == material.UploadedBy)
                        .Get();
                    userName = userResponse.Models.FirstOrDefault()?.FullName ?? "Unknown User";
                }

                var result = new MaterialResponseDTO
                {
                    Id = material.Id,
                    CourseId = material.CourseId,
                    CourseName = course.Name,
                    Title = material.Title,
                    Category = material.Category,
                    FilePath = material.FilePath,
                    Year = material.Year,
                    Status = material.Status,
                    UploadedAt = material.UploadedAt,
                    UploadedBy = material.UploadedBy,
                    UploadedByName = userName
                };

                return ServiceResult<MaterialResponseDTO>.Success(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<MaterialResponseDTO>.Failure($"Failed to approve material: {ex.Message}", "APPROVE_MATERIAL_FAILED");
            }
        }

        public async Task<ServiceResult<MaterialResponseDTO>> RejectMaterialRequestAsync(string materialId)
        {
            if (string.IsNullOrWhiteSpace(materialId))
                return ServiceResult<MaterialResponseDTO>.Failure("Material ID cannot be null or empty.", "INVALID_MATERIAL_ID");

            try
            {
                var materialResponse = await _client.From<CourseMaterial>()
                    .Where(m => m.Id == materialId)
                    .Get();

                var material = materialResponse.Models.FirstOrDefault();
                if (material == null)
                {
                    return ServiceResult<MaterialResponseDTO>.Failure("Material not found.", "MATERIAL_NOT_FOUND");
                }

                material.Status = "Rejected";
                await _client.From<CourseMaterial>().Update(material);

                var courseResponse = await _client.From<Course>()
                    .Where(c => c.Id == material.CourseId)
                    .Get();
                var course = courseResponse.Models.FirstOrDefault();

                // Fetch user name
                string? userName = null;
                if (!string.IsNullOrEmpty(material.UploadedBy))
                {
                    var userResponse = await _client.From<User>()
                        .Where(u => u.Id == material.UploadedBy)
                        .Get();
                    userName = userResponse.Models.FirstOrDefault()?.FullName ?? "Unknown User";
                }

                var result = new MaterialResponseDTO
                {
                    Id = material.Id,
                    CourseId = material.CourseId,
                    CourseName = course?.Name ?? string.Empty,
                    Title = material.Title,
                    Category = material.Category,
                    FilePath = material.FilePath,
                    Year = material.Year,
                    Status = material.Status,
                    UploadedAt = material.UploadedAt,
                    UploadedBy = material.UploadedBy,
                    UploadedByName = userName
                };

                return ServiceResult<MaterialResponseDTO>.Success(result);
            }
            catch (Exception ex)
            {
                return ServiceResult<MaterialResponseDTO>.Failure($"Failed to reject material: {ex.Message}", "REJECT_MATERIAL_FAILED");
            }
        }

        private string? ExtractTitleFromLink(string link)
        {
            if (string.IsNullOrWhiteSpace(link))
                return null;

            try
            {
                var uri = new Uri(link);
                var segments = uri.Segments;
                if (segments.Length > 0)
                {
                    var fileName = segments[segments.Length - 1];
                    return Uri.UnescapeDataString(fileName);
                }
            }
            catch
            {
                return null;
            }
            return null;
        }
    }
}
