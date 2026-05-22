USE [SSOMERO];
GO

SET XACT_ABORT ON;
BEGIN TRANSACTION;
BEGIN TRY

-- Universities
IF OBJECT_ID('dbo.Universities','U') IS NULL
BEGIN
  CREATE TABLE dbo.Universities
  (
      Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
      Name NVARCHAR(256) NOT NULL,
      Code NVARCHAR(50) NULL,
      IsDeleted BIT NOT NULL DEFAULT(0),
      RowVersion ROWVERSION NOT NULL
  );
END
GO

-- Programmes
IF OBJECT_ID('dbo.Programmes','U') IS NULL
BEGIN
  CREATE TABLE dbo.Programmes
  (
      Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
      Name NVARCHAR(256) NOT NULL,
      Code NVARCHAR(50) NULL,
      UniversityId UNIQUEIDENTIFIER NOT NULL,
      IsDeleted BIT NOT NULL DEFAULT(0),
      RowVersion ROWVERSION NOT NULL
  );

  ALTER TABLE dbo.Programmes
    ADD CONSTRAINT FK_Programmes_Universities FOREIGN KEY (UniversityId) REFERENCES dbo.Universities(Id) ON DELETE NO ACTION;
END
GO

-- Users
IF OBJECT_ID('dbo.Users','U') IS NULL
BEGIN
  CREATE TABLE dbo.Users
  (
      Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
      Email NVARCHAR(256) NOT NULL,
      NormalizedEmail NVARCHAR(256) NOT NULL,
      PasswordHash NVARCHAR(512) NULL,
      UniversityId UNIQUEIDENTIFIER NULL,
      Role INT NOT NULL,
      CreatedAt DATETIME2 NOT NULL,
      UpdatedAt DATETIME2 NULL,
      IsDeleted BIT NOT NULL DEFAULT(0),
      RowVersion ROWVERSION NOT NULL
  );

  ALTER TABLE dbo.Users
    ADD CONSTRAINT FK_Users_Universities FOREIGN KEY (UniversityId) REFERENCES dbo.Universities(Id) ON DELETE NO ACTION;

  CREATE UNIQUE INDEX IX_Users_NormalizedEmail ON dbo.Users(NormalizedEmail) WHERE IsDeleted = 0;
END
GO

-- Courses
IF OBJECT_ID('dbo.Courses','U') IS NULL
BEGIN
  CREATE TABLE dbo.Courses
  (
      Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
      Code NVARCHAR(50) NOT NULL,
      Title NVARCHAR(256) NOT NULL,
      Description NVARCHAR(4000) NULL,
      Credits INT NULL,
      ProgrammeId UNIQUEIDENTIFIER NOT NULL,
      IsDeleted BIT NOT NULL DEFAULT(0),
      RowVersion ROWVERSION NOT NULL
  );

  ALTER TABLE dbo.Courses
    ADD CONSTRAINT FK_Courses_Programmes FOREIGN KEY (ProgrammeId) REFERENCES dbo.Programmes(Id) ON DELETE NO ACTION;

  CREATE INDEX IX_Courses_ProgrammeId ON dbo.Courses(ProgrammeId);
END
GO

-- AcademicClasses
IF OBJECT_ID('dbo.AcademicClasses','U') IS NULL
BEGIN
  CREATE TABLE dbo.AcademicClasses
  (
      Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
      Name NVARCHAR(128) NOT NULL,
      AcademicYear NVARCHAR(64) NOT NULL,
      Semester NVARCHAR(64) NOT NULL,
      ProgrammeId UNIQUEIDENTIFIER NOT NULL,
      IsDeleted BIT NOT NULL DEFAULT(0),
      RowVersion ROWVERSION NOT NULL
  );

  ALTER TABLE dbo.AcademicClasses
    ADD CONSTRAINT FK_AcademicClasses_Programmes FOREIGN KEY (ProgrammeId) REFERENCES dbo.Programmes(Id) ON DELETE NO ACTION;
END
GO

-- ClassCourses
IF OBJECT_ID('dbo.ClassCourses','U') IS NULL
BEGIN
  CREATE TABLE dbo.ClassCourses
  (
      Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
      ClassId UNIQUEIDENTIFIER NOT NULL,
      CourseId UNIQUEIDENTIFIER NOT NULL,
      IsDeleted BIT NOT NULL DEFAULT(0),
      RowVersion ROWVERSION NOT NULL
  );

  ALTER TABLE dbo.ClassCourses
    ADD CONSTRAINT FK_ClassCourses_AcademicClasses FOREIGN KEY (ClassId) REFERENCES dbo.AcademicClasses(Id) ON DELETE NO ACTION;

  ALTER TABLE dbo.ClassCourses
    ADD CONSTRAINT FK_ClassCourses_Courses FOREIGN KEY (CourseId) REFERENCES dbo.Courses(Id) ON DELETE NO ACTION;
END
GO

-- Assessments
IF OBJECT_ID('dbo.Assessments','U') IS NULL
BEGIN
  CREATE TABLE dbo.Assessments
  (
      Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
      Title NVARCHAR(256) NOT NULL,
      Description NVARCHAR(4000) NULL,
      MaxScore DECIMAL(18,2) NOT NULL,
      ClassCourseId UNIQUEIDENTIFIER NOT NULL,
      IsDeleted BIT NOT NULL DEFAULT(0),
      RowVersion ROWVERSION NOT NULL
  );

  ALTER TABLE dbo.Assessments
    ADD CONSTRAINT FK_Assessments_ClassCourses FOREIGN KEY (ClassCourseId) REFERENCES dbo.ClassCourses(Id) ON DELETE NO ACTION;
END
GO

-- Enrollments
IF OBJECT_ID('dbo.Enrollments','U') IS NULL
BEGIN
  CREATE TABLE dbo.Enrollments
  (
      Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
      StudentId UNIQUEIDENTIFIER NOT NULL,
      ClassId UNIQUEIDENTIFIER NOT NULL,
      EnrolledAt DATETIME2 NOT NULL,
      Status INT NOT NULL,
      IsDeleted BIT NOT NULL DEFAULT(0),
      RowVersion ROWVERSION NOT NULL
  );

  ALTER TABLE dbo.Enrollments
    ADD CONSTRAINT FK_Enrollments_Students FOREIGN KEY (StudentId) REFERENCES dbo.Students(Id) ON DELETE NO ACTION;

  ALTER TABLE dbo.Enrollments
    ADD CONSTRAINT FK_Enrollments_AcademicClasses FOREIGN KEY (ClassId) REFERENCES dbo.AcademicClasses(Id) ON DELETE NO ACTION;

  CREATE UNIQUE INDEX UX_Enrollments_Student_Class ON dbo.Enrollments(StudentId, ClassId);
END
GO

-- Submissions
IF OBJECT_ID('dbo.Submissions','U') IS NULL
BEGIN
  CREATE TABLE dbo.Submissions
  (
      Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
      AssessmentId UNIQUEIDENTIFIER NOT NULL,
      StudentId UNIQUEIDENTIFIER NOT NULL,
      FileName NVARCHAR(512) NOT NULL,
      ContentType NVARCHAR(256) NOT NULL,
      FileSize BIGINT NOT NULL,
      StoragePath NVARCHAR(1000) NOT NULL,
      IsDeleted BIT NOT NULL DEFAULT(0),
      RowVersion ROWVERSION NOT NULL
  );

  ALTER TABLE dbo.Submissions
    ADD CONSTRAINT FK_Submissions_Assessments FOREIGN KEY (AssessmentId) REFERENCES dbo.Assessments(Id) ON DELETE NO ACTION;

  ALTER TABLE dbo.Submissions
    ADD CONSTRAINT FK_Submissions_Students FOREIGN KEY (StudentId) REFERENCES dbo.Students(Id) ON DELETE NO ACTION;

  CREATE INDEX IX_Submissions_Assessment_Student ON dbo.Submissions(AssessmentId, StudentId);
END
GO

-- Announcements
IF OBJECT_ID('dbo.Announcements','U') IS NULL
BEGIN
  CREATE TABLE dbo.Announcements
  (
      Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
      Title NVARCHAR(256) NOT NULL,
      Content NVARCHAR(MAX) NOT NULL,
      PostedAt DATETIME2 NOT NULL,
      ClassId UNIQUEIDENTIFIER NOT NULL,
      PostedByUserId UNIQUEIDENTIFIER NOT NULL,
      IsDeleted BIT NOT NULL DEFAULT(0),
      RowVersion ROWVERSION NOT NULL
  );

  ALTER TABLE dbo.Announcements
    ADD CONSTRAINT FK_Announcements_Classes FOREIGN KEY (ClassId) REFERENCES dbo.AcademicClasses(Id) ON DELETE NO ACTION;

  ALTER TABLE dbo.Announcements
    ADD CONSTRAINT FK_Announcements_Users FOREIGN KEY (PostedByUserId) REFERENCES dbo.Users(Id) ON DELETE NO ACTION;
END
GO

-- RefreshTokens
IF OBJECT_ID('dbo.RefreshTokens','U') IS NULL
BEGIN
  CREATE TABLE dbo.RefreshTokens
  (
      Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
      UserId UNIQUEIDENTIFIER NOT NULL,
      TokenHash NVARCHAR(512) NOT NULL,
      ExpiresAt DATETIME2 NOT NULL,
      CreatedAt DATETIME2 NOT NULL,
      CreatedByIp NVARCHAR(64) NULL,
      ReplacedByTokenHash NVARCHAR(512) NULL,
      IsRevoked BIT NOT NULL DEFAULT(0),
      RowVersion ROWVERSION NOT NULL
  );

  ALTER TABLE dbo.RefreshTokens
    ADD CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE NO ACTION;

  CREATE INDEX IX_RefreshTokens_UserId ON dbo.RefreshTokens(UserId);
  CREATE INDEX IX_RefreshTokens_TokenHash ON dbo.RefreshTokens(TokenHash);
END
GO

COMMIT TRANSACTION;
END TRY
BEGIN CATCH
  ROLLBACK TRANSACTION;
  DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
  RAISERROR('Schema creation failed: %s', 16, 1, @ErrMsg);
END CATCH;
GO