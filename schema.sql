-- ProjectFLS Complete Database Schema

-- =============================================
-- 1. User Management & Authentication
-- =============================================
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(20) DEFAULT 'User', -- 'Admin' or 'User'
    ApiKey NVARCHAR(255) NULL, -- User's personal API Key for LLM (Optional)
    CreatedAt DATETIME DEFAULT GETUTCDATE()
);

-- =============================================
-- 2. Chatbot System (Memory & Logs)
-- =============================================
CREATE TABLE ChatLogs (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    Role NVARCHAR(10) NOT NULL, -- 'user' or 'bot'
    Message NVARCHAR(MAX) NOT NULL,
    Timestamp DATETIME DEFAULT GETUTCDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
CREATE INDEX IX_ChatLogs_UserId ON ChatLogs(UserId);

CREATE TABLE UserMemories (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    MemoryContent NVARCHAR(MAX) NOT NULL, -- Long-term facts
    CreatedAt DATETIME DEFAULT GETUTCDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
);
CREATE INDEX IX_UserMemories_UserId ON UserMemories(UserId);

-- =============================================
-- 3. Course Management
-- =============================================
CREATE TABLE Courses (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Code NVARCHAR(20) NOT NULL, -- e.g., 'CS101'
    Name NVARCHAR(100) NOT NULL, -- e.g., 'Intro to Computing'
    Section NVARCHAR(10), -- e.g., 'A', 'B' (As per "Add new courses section" requirement)
    Description NVARCHAR(500),
    Credits INT,
    Instructor NVARCHAR(100)
);

-- Link Users to Courses (Enrollment)
CREATE TABLE UserCourses (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    CourseId INT NOT NULL,
    EnrolledAt DATETIME DEFAULT GETUTCDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (CourseId) REFERENCES Courses(Id) ON DELETE CASCADE
);

-- =============================================
-- 4. Course Materials (Past Papers, Books, etc.)
-- =============================================
CREATE TABLE CourseMaterials (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CourseId INT NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Type NVARCHAR(50) NOT NULL, -- 'Mid1', 'Mid2', 'Final', 'Quiz', 'Assignment', 'Book', 'Outline'
    FilePath NVARCHAR(500), -- Local path or URL (Github link)
    Year INT, -- For Past Papers
    Semester NVARCHAR(50), -- e.g., 'Fall 2024'
    UploadedAt DATETIME DEFAULT GETUTCDATE(),
    FOREIGN KEY (CourseId) REFERENCES Courses(Id) ON DELETE CASCADE
);

-- =============================================
-- 5. Playlists (Community & Admin)
-- =============================================
CREATE TABLE Playlists (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CourseId INT NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Url NVARCHAR(500) NOT NULL,
    Description NVARCHAR(500),
    Rating INT DEFAULT 0, -- Likes count
    IsApproved BIT DEFAULT 0, -- 0 = Pending (Request), 1 = Approved (Community)
    SubmittedByUserId INT, -- To track who requested it
    CreatedAt DATETIME DEFAULT GETUTCDATE(),
    FOREIGN KEY (CourseId) REFERENCES Courses(Id) ON DELETE CASCADE,
    FOREIGN KEY (SubmittedByUserId) REFERENCES Users(Id)
);

-- =============================================
-- 6. Time Table
-- =============================================
CREATE TABLE TimeTables (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Section NVARCHAR(10) NOT NULL, -- Linked to Course Section
    Day NVARCHAR(20) NOT NULL, -- 'Monday', 'Tuesday'...
    Time NVARCHAR(50) NOT NULL, -- '09:00 AM - 10:30 AM'
    Subject NVARCHAR(100),
    Room NVARCHAR(50)
);
