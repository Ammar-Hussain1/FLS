-- Playlist System Database Schema for Supabase
-- Run this in your Supabase SQL Editor

-- ============================================
-- 1. Community Playlists Table
-- ============================================
CREATE TABLE IF NOT EXISTS community_playlists (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,
    url TEXT NOT NULL,
    course_id INTEGER NOT NULL,
    likes INTEGER DEFAULT 0,
    submitted_by_user_id INTEGER,
    approved_by_admin_id INTEGER,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    approved_at TIMESTAMP WITH TIME ZONE
);

-- Create indexes for better query performance
CREATE INDEX IF NOT EXISTS idx_community_playlists_course ON community_playlists(course_id);
CREATE INDEX IF NOT EXISTS idx_community_playlists_likes ON community_playlists(likes DESC);

-- ============================================
-- 2. Playlist Requests Table
-- ============================================
CREATE TABLE IF NOT EXISTS playlist_requests (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL,
    playlist_name VARCHAR(255) NOT NULL,
    url TEXT NOT NULL,
    course_id INTEGER NOT NULL,
    user_id INTEGER NOT NULL,
    status VARCHAR(20) DEFAULT 'Pending' CHECK (status IN ('Pending', 'Approved', 'Rejected')),
    submitted_date TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    reviewed_date TIMESTAMP WITH TIME ZONE,
    reviewed_by_admin_id INTEGER,
    rejection_reason TEXT
);

-- Create indexes
CREATE INDEX IF NOT EXISTS idx_playlist_requests_status ON playlist_requests(status);
CREATE INDEX IF NOT EXISTS idx_playlist_requests_user ON playlist_requests(user_id);
CREATE INDEX IF NOT EXISTS idx_playlist_requests_course ON playlist_requests(course_id);

-- ============================================
-- 3. User Courses Table (Enrollments)
-- ============================================
CREATE TABLE IF NOT EXISTS user_courses (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id INTEGER NOT NULL,
    course_id INTEGER NOT NULL,
    section VARCHAR(50),
    enrolled_date TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    UNIQUE(user_id, course_id)
);

-- Create indexes
CREATE INDEX IF NOT EXISTS idx_user_courses_user ON user_courses(user_id);
CREATE INDEX IF NOT EXISTS idx_user_courses_course ON user_courses(course_id);

-- ============================================
-- Sample Data (Optional - for testing)
-- ============================================

-- Sample user course enrollments (adjust user_id as needed)
-- Uncomment to insert sample data:
/*
INSERT INTO user_courses (user_id, course_id, section) VALUES
(1, 1, 'A'),
(1, 2, 'B'),
(1, 4, 'A'),
(1, 8, 'C');
*/

-- Sample community playlists (already approved)
-- Uncomment to insert sample data:
/*
INSERT INTO community_playlists (name, url, course_id, likes, submitted_by_user_id, approved_by_admin_id, approved_at) VALUES
('Python Programming for Beginners', 'https://www.youtube.com/playlist?list=PL-osiE80TeTskrapNbzXhwoFUiLCjGgY7', 1, 245, 1, 1, NOW()),
('C++ Fundamentals', 'https://www.youtube.com/playlist?list=PLlrATfBNZ98dudnM48yfGUldqGD0S4FFb', 1, 189, 1, 1, NOW()),
('Data Structures Masterclass', 'https://www.youtube.com/playlist?list=PLBlnK6fEyqRj9lld8sWIUNwlKfdUoPd1Y', 2, 428, 1, 1, NOW()),
('HTML & CSS Complete Guide', 'https://www.youtube.com/playlist?list=PL4cUxeGkcC9ivBf_eKCPIAYXWzLlPAm6G', 4, 678, 1, 1, NOW()),
('AI Fundamentals', 'https://www.youtube.com/playlist?list=PLZHQObOWTQDNU6R1_67000Dx_ZCJB-3pi', 8, 812, 1, 1, NOW());
*/

-- Sample pending playlist requests
-- Uncomment to insert sample data:
/*
INSERT INTO playlist_requests (name, playlist_name, url, course_id, user_id, status) VALUES
('Advanced Python Techniques', 'Advanced Python', 'https://www.youtube.com/playlist?list=PLeo1K3hjS3uu7CxAacxVndI4bE_o3BDtO', 1, 1, 'Pending'),
('Graph Algorithms Visualization', 'Graph Algorithms', 'https://www.youtube.com/playlist?list=PLrmLmBdmIlpsHaNTPP_jHHDx_os9ItYXr', 2, 1, 'Pending');
*/
