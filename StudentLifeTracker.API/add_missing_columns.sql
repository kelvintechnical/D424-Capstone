-- Add missing columns to Courses table
-- Run this script against your StudentLifeTrackerDb database

USE StudentLifeTrackerDb;
GO

-- Check if columns exist before adding them
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Courses') AND name = 'CreditHours')
BEGIN
    ALTER TABLE Courses
    ADD CreditHours INT NOT NULL DEFAULT 3;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Courses') AND name = 'CurrentGrade')
BEGIN
    ALTER TABLE Courses
    ADD CurrentGrade FLOAT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Courses') AND name = 'LetterGrade')
BEGIN
    ALTER TABLE Courses
    ADD LetterGrade NVARCHAR(2) NULL;
END
GO

PRINT 'Columns added successfully';

