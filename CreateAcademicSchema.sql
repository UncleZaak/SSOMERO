IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305083917_CreateAcademicSchema'
)
BEGIN
    CREATE TABLE [Students] (
        [Id] uniqueidentifier NOT NULL,
        [Email] nvarchar(256) NOT NULL,
        [NormalizedEmail] nvarchar(256) NOT NULL,
        [PasswordHash] nvarchar(512) NULL,
        [UniversityId] uniqueidentifier NOT NULL,
        [ProgrammeId] uniqueidentifier NOT NULL,
        [AcademicYearId] uniqueidentifier NOT NULL,
        [SemesterId] uniqueidentifier NOT NULL,
        [Role] int NOT NULL,
        [RowVersion] rowversion NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_Students] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305083917_CreateAcademicSchema'
)
BEGIN
    CREATE TABLE [Universities] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(256) NOT NULL,
        [Code] nvarchar(50) NULL,
        [RowVersion] rowversion NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_Universities] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305083917_CreateAcademicSchema'
)
BEGIN
    CREATE TABLE [Programmes] (
        [Id] uniqueidentifier NOT NULL,
        [UniversityId] uniqueidentifier NOT NULL,
        [Name] nvarchar(256) NOT NULL,
        [Code] nvarchar(50) NULL,
        [RowVersion] rowversion NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_Programmes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Programmes_Universities_UniversityId] FOREIGN KEY ([UniversityId]) REFERENCES [Universities] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305083917_CreateAcademicSchema'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] uniqueidentifier NOT NULL,
        [Email] nvarchar(256) NOT NULL,
        [NormalizedEmail] nvarchar(256) NOT NULL,
        [PasswordHash] nvarchar(512) NULL,
        [Role] int NOT NULL,
        [UniversityId] uniqueidentifier NULL,
        [RowVersion] rowversion NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Users_Universities_UniversityId] FOREIGN KEY ([UniversityId]) REFERENCES [Universities] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305083917_CreateAcademicSchema'
)
BEGIN
    CREATE TABLE [AcademicClasses] (
        [Id] uniqueidentifier NOT NULL,
        [ProgrammeId] uniqueidentifier NOT NULL,
        [Name] nvarchar(128) NOT NULL,
        [AcademicYear] nvarchar(64) NOT NULL,
        [Semester] nvarchar(64) NOT NULL,
        [RowVersion] rowversion NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_AcademicClasses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AcademicClasses_Programmes_ProgrammeId] FOREIGN KEY ([ProgrammeId]) REFERENCES [Programmes] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305083917_CreateAcademicSchema'
)
BEGIN
    CREATE TABLE [Courses] (
        [Id] uniqueidentifier NOT NULL,
        [ProgrammeId] uniqueidentifier NOT NULL,
        [Code] nvarchar(50) NOT NULL,
        [Title] nvarchar(256) NOT NULL,
        [Description] nvarchar(4000) NULL,
        [Credits] int NULL,
        [RowVersion] rowversion NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_Courses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Courses_Programmes_ProgrammeId] FOREIGN KEY ([ProgrammeId]) REFERENCES [Programmes] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305083917_CreateAcademicSchema'
)
BEGIN
    CREATE TABLE [RefreshTokens] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [TokenHash] nvarchar(512) NOT NULL,
        [ExpiresAt] datetime2 NOT NULL,
        [CreatedByIp] nvarchar(64) NULL,
        [RevokedAt] datetime2 NULL,
        [ReplacedByTokenHash] nvarchar(512) NULL,
        [IsRevoked] bit NOT NULL,
        [RowVersion] rowversion NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RefreshTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305083917_CreateAcademicSchema'
)
BEGIN
    CREATE TABLE [Announcements] (
        [Id] uniqueidentifier NOT NULL,
        [ClassId] uniqueidentifier NOT NULL,
        [PostedByUserId] uniqueidentifier NOT NULL,
        [Title] nvarchar(256) NOT NULL,
        [Content] nvarchar(max) NOT NULL,
        [PostedAt] datetime2 NOT NULL,
        [RowVersion] rowversion NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_Announcements] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Announcements_AcademicClasses_ClassId] FOREIGN KEY ([ClassId]) REFERENCES [AcademicClasses] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Announcements_Users_PostedByUserId] FOREIGN KEY ([PostedByUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305083917_CreateAcademicSchema'
)
BEGIN
    CREATE TABLE [Enrollments] (
        [Id] uniqueidentifier NOT NULL,
        [StudentId] uniqueidentifier NOT NULL,
        [ClassId] uniqueidentifier NOT NULL,
        [EnrolledAt] datetime2 NOT NULL,
        [Status] int NOT NULL,
        [RowVersion] rowversion NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_Enrollments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Enrollments_AcademicClasses_ClassId] FOREIGN KEY ([ClassId]) REFERENCES [AcademicClasses] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Enrollments_Users_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305083917_CreateAcademicSchema'
)
BEGIN
    CREATE TABLE [ClassCourses] (
        [Id] uniqueidentifier NOT NULL,
        [ClassId] uniqueidentifier NOT NULL,
        [CourseId] uniqueidentifier NOT NULL,
        [LecturerId] uniqueidentifier NULL,
        [RowVersion] rowversion NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_ClassCourses] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ClassCourses_AcademicClasses_ClassId] FOREIGN KEY ([ClassId]) REFERENCES [AcademicClasses] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ClassCourses_Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [Courses] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ClassCourses_Users_LecturerId] FOREIGN KEY ([LecturerId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305083917_CreateAcademicSchema'
)
BEGIN
    CREATE TABLE [Assessments] (
        [Id] uniqueidentifier NOT NULL,
        [ClassCourseId] uniqueidentifier NOT NULL,
        [Title] nvarchar(256) NOT NULL,
        [Description] nvarchar(4000) NULL,
        [DueDate] datetime2 NULL,
        [MaxScore] decimal(18,2) NOT NULL,
        [CourseId] uniqueidentifier NULL,
        [RowVersion] rowversion NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_Assessments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Assessments_ClassCourses_ClassCourseId] FOREIGN KEY ([ClassCourseId]) REFERENCES [ClassCourses] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Assessments_Courses_CourseId] FOREIGN KEY ([CourseId]) REFERENCES [Courses] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305083917_CreateAcademicSchema'
)
BEGIN
    CREATE TABLE [Submissions] (
        [Id] uniqueidentifier NOT NULL,
        [AssessmentId] uniqueidentifier NOT NULL,
        [StudentId] uniqueidentifier NOT NULL,
        [SubmittedAt] datetime2 NOT NULL,
        [Score] decimal(18,2) NULL,
        [Feedback] nvarchar(max) NULL,
        [FileName] nvarchar(512) NOT NULL,
        [ContentType] nvarchar(256) NOT NULL,
        [FileSize] bigint NOT NULL,
        [StoragePath] nvarchar(1000) NOT NULL,
        [RowVersion] rowversion NULL,
        [IsDeleted] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [CreatedBy] uniqueidentifier NULL,
        [UpdatedAt] datetime2 NULL,
        [UpdatedBy] uniqueidentifier NULL,
        CONSTRAINT [PK_Submissions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Submissions_Assessments_AssessmentId] FOREIGN KEY ([AssessmentId]) REFERENCES [Assessments] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Submissions_Users_StudentId] FOREIGN KEY ([StudentId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305083917_CreateAcademicSchema'
)
BEGIN
    CREATE INDEX [IX_AcademicClasses_ProgrammeId] ON [AcademicClasses] ([ProgrammeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305083917_CreateAcademicSchema'
)
BEGIN
    CREATE INDEX [IX_Announcements_ClassId] ON [Announcements] ([ClassId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305083917_CreateAcademicSchema'
)
BEGIN
    CREATE INDEX [IX_Announcements_PostedByUserId] ON [Announcements] ([PostedByUserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305083917_CreateAcademicSchema'
)
BEGIN
    CREATE INDEX [IX_Assessments_ClassCourseId] ON [Assessments] ([ClassCourseId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305083917_CreateAcademicSchema'
)
BEGIN
    CREATE INDEX [IX_Assessments_CourseId] ON [Assessments] ([CourseId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305083917_CreateAcademicSchema'
)
BEGIN
    CREATE INDEX [IX_ClassCourses_ClassId] ON [ClassCourses] ([ClassId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305083917_CreateAcademicSchema'
)
BEGIN
    CREATE INDEX [IX_ClassCourses_CourseId] ON [ClassCourses] ([CourseId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305083917_CreateAcademicSchema'
)
BEGIN
    CREATE INDEX [IX_ClassCourses_LecturerId] ON [ClassCourses] ([LecturerId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305083917_CreateAcademicSchema'
)
BEGIN
    CREATE INDEX [IX_Courses_ProgrammeId] ON [Courses] ([ProgrammeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305083917_CreateAcademicSchema'
)
BEGIN
    CREATE INDEX [IX_Enrollments_ClassId] ON [Enrollments] ([ClassId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305083917_CreateAcademicSchema'
)
BEGIN
    CREATE UNIQUE INDEX [UX_Enrollments_Student_Class] ON [Enrollments] ([StudentId], [ClassId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305083917_CreateAcademicSchema'
)
BEGIN
    CREATE INDEX [IX_Programmes_UniversityId] ON [Programmes] ([UniversityId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305083917_CreateAcademicSchema'
)
BEGIN
    CREATE INDEX [IX_RefreshTokens_TokenHash] ON [RefreshTokens] ([TokenHash]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305083917_CreateAcademicSchema'
)
BEGIN
    CREATE INDEX [IX_RefreshTokens_UserId] ON [RefreshTokens] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305083917_CreateAcademicSchema'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Students_NormalizedEmail] ON [Students] ([NormalizedEmail]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305083917_CreateAcademicSchema'
)
BEGIN
    CREATE INDEX [IX_Submissions_AssessmentId_StudentId] ON [Submissions] ([AssessmentId], [StudentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305083917_CreateAcademicSchema'
)
BEGIN
    CREATE INDEX [IX_Submissions_StudentId] ON [Submissions] ([StudentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305083917_CreateAcademicSchema'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Users_NormalizedEmail] ON [Users] ([NormalizedEmail]) WHERE [IsDeleted] = 0');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305083917_CreateAcademicSchema'
)
BEGIN
    CREATE INDEX [IX_Users_UniversityId] ON [Users] ([UniversityId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260305083917_CreateAcademicSchema'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260305083917_CreateAcademicSchema', N'8.0.0');
END;
GO

COMMIT;
GO

