using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Ssomero.Api.Entities;

namespace Ssomero.Api.Data;

/// <summary>
/// Seeds the database with initial academic master data.
/// Called at startup; idempotent (skips if data already exists).
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(SsomeroDbContext db, IConfiguration config)
    {
        await SeedAdminAsync(db, config);
        await SeedLookupsAsync(db);
        await SeedAcademicStructureAsync(db);
        await SeedSampleUsersAsync(db);
    }

    // ---------------------------------------------------------------
    private static async Task SeedAdminAsync(SsomeroDbContext db, IConfiguration config)
    {
        if (await db.Admins.AnyAsync()) return;

        // Read from appsettings / environment variables.
        // Set Admin:Email and Admin:Password in appsettings.Development.json (dev)
        // or as ADMIN_EMAIL / ADMIN_PASSWORD environment variables (production).
        var email = config["Admin:Email"]
            ?? Environment.GetEnvironmentVariable("ADMIN_EMAIL");
        var password = config["Admin:Password"]
            ?? Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidOperationException(
                "Admin seed email is not configured. " +
                "Set 'Admin:Email' in appsettings or the ADMIN_EMAIL environment variable.");

        if (string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException(
                "Admin seed password is not configured. " +
                "Set 'Admin:Password' in appsettings or the ADMIN_PASSWORD environment variable.");

        db.Admins.Add(new Admin
        {
            Id = Guid.NewGuid(),
            Email = email.ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    // ---------------------------------------------------------------
    private static async Task SeedLookupsAsync(SsomeroDbContext db)
    {
        if (!await db.Semesters.AnyAsync())
        {
            db.Semesters.AddRange(
                new Semester { Id = Guid.NewGuid(), Name = "Semester 1", Number = 1 },
                new Semester { Id = Guid.NewGuid(), Name = "Semester 2", Number = 2 }
            );
        }

        if (!await db.AcademicYears.AnyAsync())
        {
            db.AcademicYears.AddRange(
                new AcademicYear { Id = Guid.NewGuid(), Name = "2023/2024" },
                new AcademicYear { Id = Guid.NewGuid(), Name = "2024/2025" },
                new AcademicYear { Id = Guid.NewGuid(), Name = "2025/2026" }
            );
        }

        if (!await db.EntrySchemes.AnyAsync())
        {
            db.EntrySchemes.AddRange(
                new EntryScheme { Id = Guid.NewGuid(), Name = "Direct Entry" },
                new EntryScheme { Id = Guid.NewGuid(), Name = "Diploma Entry" },
                new EntryScheme { Id = Guid.NewGuid(), Name = "Mature Age Entry" }
            );
        }

        if (!await db.Intakes.AnyAsync())
        {
            db.Intakes.AddRange(
                new Intake { Id = Guid.NewGuid(), Name = "August 2024" },
                new Intake { Id = Guid.NewGuid(), Name = "January 2025" },
                new Intake { Id = Guid.NewGuid(), Name = "August 2025" }
            );
        }

        if (!await db.StudyModes.AnyAsync())
        {
            db.StudyModes.AddRange(
                new StudyMode { Id = Guid.NewGuid(), Name = "Day" },
                new StudyMode { Id = Guid.NewGuid(), Name = "Evening" },
                new StudyMode { Id = Guid.NewGuid(), Name = "Weekend" },
                new StudyMode { Id = Guid.NewGuid(), Name = "Distance" }
            );
        }

        await db.SaveChangesAsync();
    }

    // ---------------------------------------------------------------
    private static async Task SeedAcademicStructureAsync(SsomeroDbContext db)
    {
        if (await db.Universities.AnyAsync()) return;

        var sem1 = await db.Semesters.FirstAsync(s => s.Number == 1);
        var sem2 = await db.Semesters.FirstAsync(s => s.Number == 2);

        // ---- University 1: Makerere ----
        var mak = new University { Id = Guid.NewGuid(), Name = "Makerere University" };
        db.Universities.Add(mak);

        var makFacCit = new Faculty { Id = Guid.NewGuid(), Name = "Faculty of Computing and Informatics Technology", UniversityId = mak.Id };
        var makFacEng = new Faculty { Id = Guid.NewGuid(), Name = "Faculty of Engineering", UniversityId = mak.Id };
        var makFacBus = new Faculty { Id = Guid.NewGuid(), Name = "Faculty of Business and Management", UniversityId = mak.Id };
        db.Faculties.AddRange(makFacCit, makFacEng, makFacBus);

        var deptCs  = new Department { Id = Guid.NewGuid(), Name = "Department of Computer Science", FacultyId = makFacCit.Id };
        var deptIt  = new Department { Id = Guid.NewGuid(), Name = "Department of Information Technology", FacultyId = makFacCit.Id };
        var deptEle = new Department { Id = Guid.NewGuid(), Name = "Department of Electrical Engineering", FacultyId = makFacEng.Id };
        var deptBa  = new Department { Id = Guid.NewGuid(), Name = "Department of Business Administration", FacultyId = makFacBus.Id };
        db.Departments.AddRange(deptCs, deptIt, deptEle, deptBa);

        var progBsCs  = new AcademicProgram { Id = Guid.NewGuid(), Name = "Bachelor of Science in Computer Science", DurationSemesters = 8, DepartmentId = deptCs.Id };
        var progBsIt  = new AcademicProgram { Id = Guid.NewGuid(), Name = "Bachelor of Science in Information Technology", DurationSemesters = 8, DepartmentId = deptIt.Id };
        var progBsEle = new AcademicProgram { Id = Guid.NewGuid(), Name = "Bachelor of Science in Electrical Engineering", DurationSemesters = 10, DepartmentId = deptEle.Id };
        var progBba   = new AcademicProgram { Id = Guid.NewGuid(), Name = "Bachelor of Business Administration", DurationSemesters = 8, DepartmentId = deptBa.Id };
        db.Programs.AddRange(progBsCs, progBsIt, progBsEle, progBba);

        // Curriculum for BSc CS — Year 1
        db.Curricula.AddRange(
            new Curriculum { Id = Guid.NewGuid(), ProgramId = progBsCs.Id, YearOfStudy = 1, SemesterId = sem1.Id, CourseCode = "CSC1100", CourseName = "Introduction to Computing" },
            new Curriculum { Id = Guid.NewGuid(), ProgramId = progBsCs.Id, YearOfStudy = 1, SemesterId = sem1.Id, CourseCode = "CSC1101", CourseName = "Programming Fundamentals" },
            new Curriculum { Id = Guid.NewGuid(), ProgramId = progBsCs.Id, YearOfStudy = 1, SemesterId = sem1.Id, CourseCode = "MTH1100", CourseName = "Mathematics for Computing I" },
            new Curriculum { Id = Guid.NewGuid(), ProgramId = progBsCs.Id, YearOfStudy = 1, SemesterId = sem2.Id, CourseCode = "CSC1200", CourseName = "Data Structures" },
            new Curriculum { Id = Guid.NewGuid(), ProgramId = progBsCs.Id, YearOfStudy = 1, SemesterId = sem2.Id, CourseCode = "CSC1201", CourseName = "Object-Oriented Programming" },
            new Curriculum { Id = Guid.NewGuid(), ProgramId = progBsCs.Id, YearOfStudy = 1, SemesterId = sem2.Id, CourseCode = "MTH1200", CourseName = "Discrete Mathematics" }
        );

        // Curriculum for BSc CS — Year 2
        db.Curricula.AddRange(
            new Curriculum { Id = Guid.NewGuid(), ProgramId = progBsCs.Id, YearOfStudy = 2, SemesterId = sem1.Id, CourseCode = "CSC2100", CourseName = "Algorithms and Complexity" },
            new Curriculum { Id = Guid.NewGuid(), ProgramId = progBsCs.Id, YearOfStudy = 2, SemesterId = sem1.Id, CourseCode = "CSC2101", CourseName = "Database Systems" },
            new Curriculum { Id = Guid.NewGuid(), ProgramId = progBsCs.Id, YearOfStudy = 2, SemesterId = sem2.Id, CourseCode = "CSC2200", CourseName = "Operating Systems" },
            new Curriculum { Id = Guid.NewGuid(), ProgramId = progBsCs.Id, YearOfStudy = 2, SemesterId = sem2.Id, CourseCode = "CSC2201", CourseName = "Computer Networks" }
        );

        // Curriculum for BSc IT — Year 1
        db.Curricula.AddRange(
            new Curriculum { Id = Guid.NewGuid(), ProgramId = progBsIt.Id, YearOfStudy = 1, SemesterId = sem1.Id, CourseCode = "IT1100", CourseName = "Fundamentals of Information Systems" },
            new Curriculum { Id = Guid.NewGuid(), ProgramId = progBsIt.Id, YearOfStudy = 1, SemesterId = sem1.Id, CourseCode = "IT1101", CourseName = "Web Technologies I" },
            new Curriculum { Id = Guid.NewGuid(), ProgramId = progBsIt.Id, YearOfStudy = 1, SemesterId = sem2.Id, CourseCode = "IT1200", CourseName = "Database Management" },
            new Curriculum { Id = Guid.NewGuid(), ProgramId = progBsIt.Id, YearOfStudy = 1, SemesterId = sem2.Id, CourseCode = "IT1201", CourseName = "Web Technologies II" }
        );

        // ---- University 2: Kyambogo ----
        var kyu = new University { Id = Guid.NewGuid(), Name = "Kyambogo University" };
        db.Universities.Add(kyu);

        var kyuFacEng = new Faculty { Id = Guid.NewGuid(), Name = "Faculty of Engineering and Technology", UniversityId = kyu.Id };
        var kyuFacEdu = new Faculty { Id = Guid.NewGuid(), Name = "Faculty of Education", UniversityId = kyu.Id };
        db.Faculties.AddRange(kyuFacEng, kyuFacEdu);

        var kyuDeptMech = new Department { Id = Guid.NewGuid(), Name = "Department of Mechanical Engineering", FacultyId = kyuFacEng.Id };
        var kyuDeptEdu  = new Department { Id = Guid.NewGuid(), Name = "Department of Education Management", FacultyId = kyuFacEdu.Id };
        db.Departments.AddRange(kyuDeptMech, kyuDeptEdu);

        db.Programs.AddRange(
            new AcademicProgram { Id = Guid.NewGuid(), Name = "Bachelor of Science in Mechanical Engineering", DurationSemesters = 10, DepartmentId = kyuDeptMech.Id },
            new AcademicProgram { Id = Guid.NewGuid(), Name = "Bachelor of Education", DurationSemesters = 8, DepartmentId = kyuDeptEdu.Id }
        );

        // ---- University 3: Uganda Christian University ----
        var ucu = new University { Id = Guid.NewGuid(), Name = "Uganda Christian University" };
        db.Universities.Add(ucu);

        var ucuFacLaw = new Faculty { Id = Guid.NewGuid(), Name = "Faculty of Law", UniversityId = ucu.Id };
        var ucuFacBus = new Faculty { Id = Guid.NewGuid(), Name = "Faculty of Business and Administration", UniversityId = ucu.Id };
        db.Faculties.AddRange(ucuFacLaw, ucuFacBus);

        var ucuDeptLaw = new Department { Id = Guid.NewGuid(), Name = "Department of Law", FacultyId = ucuFacLaw.Id };
        var ucuDeptAcc = new Department { Id = Guid.NewGuid(), Name = "Department of Accounting and Finance", FacultyId = ucuFacBus.Id };
        db.Departments.AddRange(ucuDeptLaw, ucuDeptAcc);

        db.Programs.AddRange(
            new AcademicProgram { Id = Guid.NewGuid(), Name = "Bachelor of Laws (LLB)", DurationSemesters = 8, DepartmentId = ucuDeptLaw.Id },
            new AcademicProgram { Id = Guid.NewGuid(), Name = "Bachelor of Accounting and Finance", DurationSemesters = 8, DepartmentId = ucuDeptAcc.Id }
        );

        await db.SaveChangesAsync();
    }

    // ---------------------------------------------------------------
    private static async Task SeedSampleUsersAsync(SsomeroDbContext db)
    {
        if (await db.Lecturers.IgnoreQueryFilters().AnyAsync()) return;

        var mak = await db.Universities.FirstOrDefaultAsync(u => u.Name == "Makerere University");
        if (mak is null) return;

        // Sample lecturers
        db.Lecturers.AddRange(
            new Lecturer
            {
                Id = Guid.NewGuid(),
                FirstName = "Alice",
                LastName = "Nakamya",
                Email = "alice.nakamya@mak.ac.ug",
                Phone = "+256700000001",
                StaffId = "MAK/LEC/001",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Lecturer123!"),
                IsVerified = true,
                IsApproved = true,
                Status = UserStatus.Active,
                UniversityId = mak.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-60)
            },
            new Lecturer
            {
                Id = Guid.NewGuid(),
                FirstName = "Brian",
                LastName = "Ssekandi",
                Email = "brian.ssekandi@mak.ac.ug",
                Phone = "+256700000002",
                StaffId = "MAK/LEC/002",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Lecturer123!"),
                IsVerified = true,
                IsApproved = false,
                Status = UserStatus.Active,
                UniversityId = mak.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            }
        );

        // Sample student
        var prog = await db.Programs.FirstOrDefaultAsync(p => p.Name.Contains("Computer Science"));
        var entryScheme = await db.EntrySchemes.FirstOrDefaultAsync();
        var intake = await db.Intakes.FirstOrDefaultAsync();
        var studyMode = await db.StudyModes.FirstOrDefaultAsync();
        var acYear = await db.AcademicYears.FirstOrDefaultAsync();

        if (prog is not null && entryScheme is not null && intake is not null && studyMode is not null && acYear is not null)
        {
            var dept = await db.Departments.FirstOrDefaultAsync(d => d.Name.Contains("Computer Science"));
            var fac  = dept is not null ? await db.Faculties.FindAsync(dept.FacultyId) : null;
            var sem  = await db.Semesters.FirstAsync(s => s.Number == 1);

            if (dept is not null && fac is not null)
            {
                var studentId = Guid.NewGuid();
                db.Students.Add(new Student
                {
                    Id = studentId,
                    FirstName = "Catherine",
                    SecondName = "Namutebi",
                    Email = "catherine.namutebi@students.mak.ac.ug",
                    Phone = "+256700000010",
                    Gender = "Female",
                    Dob = new DateOnly(2002, 5, 14),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Student123!"),
                    IsVerified = true,
                    Status = UserStatus.Active,
                    UniversityId = mak.Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-45)
                });

                db.AcademicProfiles.Add(new AcademicProfile
                {
                    Id = Guid.NewGuid(),
                    StudentId = studentId,
                    UniversityId = mak.Id,
                    FacultyId = fac.Id,
                    DepartmentId = dept.Id,
                    ProgramId = prog.Id,
                    EntrySchemeId = entryScheme.Id,
                    IntakeId = intake.Id,
                    StudyModeId = studyMode.Id,
                    AcademicYearId = acYear.Id,
                    SemesterId = sem.Id,
                    YearOfStudy = 1
                });
            }
        }

        await db.SaveChangesAsync();
    }
}
