-- Safe reset of the SSOMERO database (drops and recreates)
IF DB_ID(N'SSOMERO') IS NOT NULL
BEGIN
    ALTER DATABASE [SSOMERO] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [SSOMERO];
END
GO

CREATE DATABASE [SSOMERO];
GO

USE [SSOMERO];
GO

-- Required session settings for indexed views / filtered indexes / computed columns
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

-- Create tables in dependency order

-- 1. Universities
CREATE TABLE [Universities] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [Name] NVARCHAR(256) NOT NULL,
    [Code] NVARCHAR(50) NULL,
    [RowVersion] ROWVERSION NULL,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [CreatedBy] UNIQUEIDENTIFIER NULL,
    [UpdatedAt] DATETIME2 NULL,
    [UpdatedBy] UNIQUEIDENTIFIER NULL
);
GO

-- 2. Users
CREATE TABLE [Users] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [Email] NVARCHAR(256) NOT NULL,
    [NormalizedEmail] NVARCHAR(256) NOT NULL,
    [PasswordHash] NVARCHAR(512) NULL,
    [Role] INT NOT NULL,
    [RowVersion] ROWVERSION NULL,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [CreatedBy] UNIQUEIDENTIFIER NULL,
    [UpdatedAt] DATETIME2 NULL,
    [UpdatedBy] UNIQUEIDENTIFIER NULL
);
GO

-- Unique filtered index on Users.NormalizedEmail for active (non-deleted) users
CREATE UNIQUE INDEX IX_Users_NormalizedEmail ON [Users] ([NormalizedEmail])
WHERE [IsDeleted] = 0;
GO

-- 3. Programmes
CREATE TABLE [Programmes] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [UniversityId] UNIQUEIDENTIFIER NOT NULL,
    [Name] NVARCHAR(256) NOT NULL,
    [Code] NVARCHAR(50) NULL,
    [RowVersion] ROWVERSION NULL,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [CreatedBy] UNIQUEIDENTIFIER NULL,
    [UpdatedAt] DATETIME2 NULL,
    [UpdatedBy] UNIQUEIDENTIFIER NULL,
    CONSTRAINT FK_Programmes_Universities_UniversityId FOREIGN KEY ([UniversityId]) REFERENCES [Universities]([Id]) ON DELETE NO ACTION
);
GO

-- 4. AcademicClasses
CREATE TABLE [AcademicClasses] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [ProgrammeId] UNIQUEIDENTIFIER NOT NULL,
    [Name] NVARCHAR(256) NOT NULL,
    [Code] NVARCHAR(50) NULL,
    [RowVersion] ROWVERSION NULL,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [CreatedBy] UNIQUEIDENTIFIER NULL,
    [UpdatedAt] DATETIME2 NULL,
    [UpdatedBy] UNIQUEIDENTIFIER NULL,
    CONSTRAINT FK_AcademicClasses_Programmes_ProgrammeId FOREIGN KEY ([ProgrammeId]) REFERENCES [Programmes]([Id]) ON DELETE NO ACTION
);
GO

-- 5. Courses
CREATE TABLE [Courses] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [ProgrammeId] UNIQUEIDENTIFIER NOT NULL,
    [Title] NVARCHAR(256) NOT NULL,
    [Code] NVARCHAR(50) NULL,
    [Credits] INT NOT NULL DEFAULT 0,
    [RowVersion] ROWVERSION NULL,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [CreatedBy] UNIQUEIDENTIFIER NULL,
    [UpdatedAt] DATETIME2 NULL,
    [UpdatedBy] UNIQUEIDENTIFIER NULL,
    CONSTRAINT FK_Courses_Programmes_ProgrammeId FOREIGN KEY ([ProgrammeId]) REFERENCES [Programmes]([Id]) ON DELETE NO ACTION
);
GO

-- 6. Students
CREATE TABLE [Students] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [Email] NVARCHAR(256) NOT NULL,
    [NormalizedEmail] NVARCHAR(256) NOT NULL,
    [PasswordHash] NVARCHAR(512) NULL,
    [UniversityId] UNIQUEIDENTIFIER NOT NULL,
    [ProgrammeId] UNIQUEIDENTIFIER NOT NULL,
    [AcademicYearId] UNIQUEIDENTIFIER NULL,
    [SemesterId] UNIQUEIDENTIFIER NULL,
    [Role] INT NOT NULL,
    [RowVersion] ROWVERSION NULL,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [CreatedBy] UNIQUEIDENTIFIER NULL,
    [UpdatedAt] DATETIME2 NULL,
    [UpdatedBy] UNIQUEIDENTIFIER NULL,
    CONSTRAINT FK_Students_Universities_UniversityId FOREIGN KEY ([UniversityId]) REFERENCES [Universities]([Id]) ON DELETE NO ACTION,
    CONSTRAINT FK_Students_Programmes_ProgrammeId FOREIGN KEY ([ProgrammeId]) REFERENCES [Programmes]([Id]) ON DELETE NO ACTION
);
GO

-- 7. Enrollments
CREATE TABLE [Enrollments] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [StudentId] UNIQUEIDENTIFIER NOT NULL,
    [ClassId] UNIQUEIDENTIFIER NOT NULL,
    [EnrolledAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [RowVersion] ROWVERSION NULL,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Enrollments_Students_StudentId FOREIGN KEY ([StudentId]) REFERENCES [Students]([Id]) ON DELETE CASCADE,
    CONSTRAINT FK_Enrollments_AcademicClasses_ClassId FOREIGN KEY ([ClassId]) REFERENCES [AcademicClasses]([Id]) ON DELETE NO ACTION
);
GO

-- Composite unique index to prevent duplicate enrollments
CREATE UNIQUE INDEX UX_Enrollments_Student_Class ON [Enrollments]([StudentId], [ClassId])
WHERE [IsDeleted] = 0;
GO

-- 8. Assessments
CREATE TABLE [Assessments] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [ClassId] UNIQUEIDENTIFIER NOT NULL,
    [Title] NVARCHAR(256) NOT NULL,
    [MaxScore] INT NOT NULL,
    [DueDate] DATETIME2 NULL,
    [RowVersion] ROWVERSION NULL,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Assessments_AcademicClasses_ClassId FOREIGN KEY ([ClassId]) REFERENCES [AcademicClasses]([Id]) ON DELETE NO ACTION
);
GO

-- 9. Submissions
CREATE TABLE [Submissions] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [AssessmentId] UNIQUEIDENTIFIER NOT NULL,
    [StudentId] UNIQUEIDENTIFIER NOT NULL,
    [SubmittedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [Score] DECIMAL(5,2) NULL,
    [RowVersion] ROWVERSION NULL,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Submissions_Assessments_AssessmentId FOREIGN KEY ([AssessmentId]) REFERENCES [Assessments]([Id]) ON DELETE CASCADE,
    CONSTRAINT FK_Submissions_Students_StudentId FOREIGN KEY ([StudentId]) REFERENCES [Students]([Id]) ON DELETE CASCADE
);
GO

-- 10. Announcements
CREATE TABLE [Announcements] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [ClassId] UNIQUEIDENTIFIER NULL,
    [Title] NVARCHAR(256) NOT NULL,
    [Body] NVARCHAR(MAX) NULL,
    [PostedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [RowVersion] ROWVERSION NULL,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Announcements_AcademicClasses_ClassId FOREIGN KEY ([ClassId]) REFERENCES [AcademicClasses]([Id]) ON DELETE SET NULL
);
GO

-- 11. RefreshTokens
CREATE TABLE [RefreshTokens] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [Token] NVARCHAR(512) NOT NULL,
    [Expires] DATETIME2 NOT NULL,
    [Revoked] DATETIME2 NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_RefreshTokens_Users_UserId FOREIGN KEY ([UserId]) REFERENCES [Users]([Id]) ON DELETE CASCADE
);
GO

-- Additional useful indexes
CREATE INDEX IX_Programmes_UniversityId ON [Programmes]([UniversityId]);
CREATE INDEX IX_AcademicClasses_ProgrammeId ON [AcademicClasses]([ProgrammeId]);
CREATE INDEX IX_Courses_ProgrammeId ON [Courses]([ProgrammeId]);
CREATE INDEX IX_Students_University_Programme ON [Students]([UniversityId], [ProgrammeId]);
CREATE INDEX IX_Submissions_AssessmentId ON [Submissions]([AssessmentId]);
GO

-- Verification queries
-- 1) Tables
SELECT TABLE_SCHEMA, TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' AND TABLE_CATALOG = 'SSOMERO' ORDER BY TABLE_SCHEMA, TABLE_NAME;

-- 2) Foreign keys
SELECT fk.name AS ForeignKeyName, OBJECT_NAME(fk.parent_object_id) AS TableName, c1.name AS ColumnName,
       OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable, c2.name AS ReferencedColumn
FROM sys.foreign_keys fk
JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
JOIN sys.columns c1 ON fkc.parent_object_id = c1.object_id AND fkc.parent_column_id = c1.column_id
JOIN sys.columns c2 ON fkc.referenced_object_id = c2.object_id AND fkc.referenced_column_id = c2.column_id
ORDER BY fk.name;

-- 3) Indexes (unique indexes and filtered definitions)
SELECT t.name AS TableName, i.name AS IndexName, i.is_unique, i.filter_definition
FROM sys.indexes i
JOIN sys.tables t ON i.object_id = t.object_id
WHERE i.is_unique = 1 OR i.filter_definition IS NOT NULL
ORDER BY t.name, i.name;

-- End of script
