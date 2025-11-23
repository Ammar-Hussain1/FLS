-- ProjectFLS Complete Database Schema

-- =============================================
-- 1. User Management & Authentication
-- =============================================
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(20) DEFAULT 'User', -- 'Admin' or 'User'
    CreatedAt DATETIME DEFAULT GETUTCDATE()
);

-- Need New table for api key
CREATE TABLE ApiKeys (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    ApiKey NVARCHAR(255) NOT NULL,
    CreatedAt DATETIME DEFAULT GETUTCDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
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
    Description NVARCHAR(500),
    Credits INT
);

--Separate table for sections because each course has mulltiple sections and instructors
CREATE TABLE Sections (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CourseId INT NOT NULL,
    Section NVARCHAR(10) NOT NULL, -- e.g., 'A', 'B'
    InstructorId INT,
    FOREIGN KEY (CourseId) REFERENCES Courses(Id) ON DELETE CASCADE,
    FOREIGN KEY (InstructorId) REFERENCES Users(Id) ON DELETE SET NULL
);

-- Link Users to Courses (Enrollment)
CREATE TABLE UserCourses (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    CourseId INT NOT NULL,
    SectionId INT NOT NULL,
    EnrolledAt DATETIME DEFAULT GETUTCDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (CourseId) REFERENCES Courses(Id) ON DELETE CASCADE,
    FOREIGN KEY (SectionId) REFERENCES Sections(Id) ON DELETE CASCADE
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
    Status NVARCHAR(50), -- 'Pending', 'Approved', 'Rejected'
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
    Rating INT DEFAULT 0, -- Likes count
    Status NVARCHAR(50), -- 'Pending', 'Approved', 'Rejected'
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
    CourseId INT NOT NULL,
    SectionId INT NOT NULL, -- Linked to Sections Table
    Day NVARCHAR(20) NOT NULL, -- 'Monday', 'Tuesday'...
    Time NVARCHAR(50) NOT NULL, -- '09:00 AM - 10:30 AM'
    Subject NVARCHAR(100),
    Room NVARCHAR(50),
    InstructorId INT,
    FOREIGN KEY (SectionId) REFERENCES Sections(Id) ON DELETE CASCADE,
    FOREIGN KEY (InstructorId) REFERENCES Users(Id) ON DELETE NO ACTION,
    FOREIGN KEY (CourseId) REFERENCES Courses(Id) ON DELETE NO ACTION
);
