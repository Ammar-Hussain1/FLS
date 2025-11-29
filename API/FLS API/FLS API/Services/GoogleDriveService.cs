using Google.Apis.Drive.v3;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;

namespace FLS_API.BL
{
    public class GoogleDriveService
    {
        private readonly DriveService _service;
        private readonly string _rootFolderId;
        private readonly string? _sharedDriveId;

        public GoogleDriveService(IConfiguration config)
        {
            _rootFolderId = config["GoogleDrive:RootFolderId"];
            if (string.IsNullOrEmpty(_rootFolderId))
                throw new ArgumentException("Google Drive root folder ID is required", nameof(config));
            
            // Optional: Shared Drive ID for Google Workspace (REQUIRED for service accounts)
            _sharedDriveId = config["GoogleDrive:SharedDriveId"];
            if (string.IsNullOrEmpty(_sharedDriveId))
            {
                Console.WriteLine("[WARNING] GoogleDrive:SharedDriveId is not configured. Service accounts require Shared Drives to upload files.");
            }
            
            GoogleCredential credential;
            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                            .CreateScoped(DriveService.Scope.Drive);
            }

            _service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "FLS Google Drive API"
            });
        }

        public async Task<string> CreateFolderAsync(string name, string parentFolderId = null)
        {
            if (parentFolderId == null)
                parentFolderId = _rootFolderId;

            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = name,
                MimeType = "application/vnd.google-apps.folder",
                Parents = new List<string> { parentFolderId }
            };

            var request = _service.Files.Create(fileMetadata);
            request.Fields = "id, name";
            
            // Support shared drives (REQUIRED for service accounts)
            request.SupportsAllDrives = true;
            request.SupportsTeamDrives = true;

            var file = await request.ExecuteAsync();
            return file.Id;
        }

        public async Task<string?> GetFolderIdByNameAsync(string name, string parentFolderId = null)
        {
            if (parentFolderId == null)
                parentFolderId = _rootFolderId;

            var query = $"mimeType='application/vnd.google-apps.folder' and name='{name}' and '{parentFolderId}' in parents and trashed=false";

            var request = _service.Files.List();
            request.Q = query;
            request.Fields = "files(id, name)";
            
            // Support shared drives (REQUIRED for service accounts)
            request.SupportsAllDrives = true;
            request.SupportsTeamDrives = true;
            if (!string.IsNullOrEmpty(_sharedDriveId))
            {
                request.DriveId = _sharedDriveId;
                request.IncludeItemsFromAllDrives = true;
                request.Corpora = "drive";
            }
            
            var result = await request.ExecuteAsync();

            return result.Files.Count > 0 ? result.Files[0].Id : null;
        }

        public async Task<string> EnsureFolderAsync(string name, string parentFolderId = null)
        {
            var existing = await GetFolderIdByNameAsync(name, parentFolderId);
            if (existing != null)
                return existing;

            return await CreateFolderAsync(name, parentFolderId);
        }

        public async Task<string> CreateNestedFoldersAsync(string[] names)
        {
            string currentParent = _rootFolderId;

            foreach (var name in names)
            {
                currentParent = await EnsureFolderAsync(name, currentParent);
            }

            return currentParent;
        }
        public async Task<Dictionary<string, string>> CreateCourseStructureAsync(string courseName)
        {
            string courseFolderId = await EnsureFolderAsync(courseName);

            string[] subFolders = new[]
            {
                "assignments",
                "books",
                "course outline",
                "Midterm 1",
                "Midterm 2",
                "quizzes",
                "Final"
            };

            var result = new Dictionary<string, string>();
            result["CourseRoot"] = courseFolderId;

            foreach (var folder in subFolders)
            {
                string id = await EnsureFolderAsync(folder, courseFolderId);
                result[folder] = id;
            }

            return result;
        }

        /// <summary>
        /// Uploads a file to Google Drive in the specified course folder structure
        /// </summary>
        /// <param name="fileStream">The file stream to upload</param>
        /// <param name="fileName">The name of the file</param>
        /// <param name="mimeType">The MIME type of the file (e.g., "application/pdf")</param>
        /// <param name="courseName">The name of the course</param>
        /// <param name="category">The category folder (assignments, books, etc.)</param>
        /// <returns>The Google Drive web view link</returns>
        public async Task<string> UploadFileToCourseFolderAsync(Stream fileStream, string fileName, string mimeType, string courseName, string category)
        {
            // Ensure course folder exists
            string courseFolderId = await EnsureFolderAsync(courseName);
            
            // Ensure category folder exists within course folder
            string categoryFolderId = await EnsureFolderAsync(category, courseFolderId);
            
            // Log upload location
            var uploadPath = $"Google Drive → Root Folder → {courseName} → {category} → {fileName}";
            Console.WriteLine($"[Google Drive Upload] Uploading file to: {uploadPath}");
            Console.WriteLine($"[Google Drive Upload] Course Folder ID: {courseFolderId}");
            Console.WriteLine($"[Google Drive Upload] Category Folder ID: {categoryFolderId}");
            Console.WriteLine($"[Google Drive Upload] File Name: {fileName}");

            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = fileName,
                Parents = new List<string> { categoryFolderId }
            };

            var request = _service.Files.Create(fileMetadata, fileStream, mimeType);
            request.Fields = "id, name, webViewLink";
            
            // Support shared drives (REQUIRED for service accounts)
            request.SupportsAllDrives = true;
            request.SupportsTeamDrives = true;
            
            Console.WriteLine($"[Google Drive Upload] Using Shared Drive: {!string.IsNullOrEmpty(_sharedDriveId)}");
            if (!string.IsNullOrEmpty(_sharedDriveId))
            {
                Console.WriteLine($"[Google Drive Upload] Shared Drive ID: {_sharedDriveId}");
            }

            var uploadProgress = await request.UploadAsync();
            
            if (uploadProgress.Status != Google.Apis.Upload.UploadStatus.Completed)
            {
                throw new Exception($"File upload failed: {uploadProgress.Exception?.Message ?? "Unknown error"}");
            }

            var uploadedFile = request.ResponseBody;
            return uploadedFile.WebViewLink ?? $"https://drive.google.com/file/d/{uploadedFile.Id}/view";
        }

    }

}
