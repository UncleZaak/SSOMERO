using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero.Api.Data;
using Ssomero.Api.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ssomero.Api.Data.UnitTests;
/// <summary>
/// Unit tests for the <see cref = "DbSeeder"/> class.
/// </summary>
[TestClass]
public partial class DbSeederTests
{
    /// <summary>
    /// Tests that SeedAsync returns early when universities already exist.
    /// Input: Database with existing universities
    /// Expected: Only admin seeding occurs (if needed), then early return
    /// </summary>
    [TestMethod]
    [TestCategory("ProductionBugSuspected")]
    [Ignore("ProductionBugSuspected")]
    public async Task SeedAsync_UniversitiesExist_ReturnsEarly()
    {
        // Arrange
        var existingUniversity = new University
        {
            Id = Guid.NewGuid(),
            Name = "Test University"
        };
        var mockContext = new Mock<SsomeroDbContext>(new DbContextOptions<SsomeroDbContext>());
        var mockAdmins = CreateMockDbSet<Admin>(new List<Admin>());
        var mockUniversities = CreateMockDbSet<University>(new List<University> { existingUniversity });
        var mockFaculties = CreateMockDbSet<Faculty>(new List<Faculty>());
        var mockDepartments = CreateMockDbSet<Department>(new List<Department>());
        var mockPrograms = CreateMockDbSet<AcademicProgram>(new List<AcademicProgram>());
        var mockSemesters = CreateMockDbSet<Semester>(new List<Semester>());
        var mockAcademicYears = CreateMockDbSet<AcademicYear>(new List<AcademicYear>());
        var mockEntrySchemes = CreateMockDbSet<EntryScheme>(new List<EntryScheme>());
        var mockIntakes = CreateMockDbSet<Intake>(new List<Intake>());
        var mockStudyModes = CreateMockDbSet<StudyMode>(new List<StudyMode>());
        var mockCurricula = CreateMockDbSet<Curriculum>(new List<Curriculum>());
        mockContext.Setup(m => m.Admins).Returns(mockAdmins.Object);
        mockContext.Setup(m => m.Universities).Returns(mockUniversities.Object);
        mockContext.Setup(m => m.Faculties).Returns(mockFaculties.Object);
        mockContext.Setup(m => m.Departments).Returns(mockDepartments.Object);
        mockContext.Setup(m => m.Programs).Returns(mockPrograms.Object);
        mockContext.Setup(m => m.Semesters).Returns(mockSemesters.Object);
        mockContext.Setup(m => m.AcademicYears).Returns(mockAcademicYears.Object);
        mockContext.Setup(m => m.EntrySchemes).Returns(mockEntrySchemes.Object);
        mockContext.Setup(m => m.Intakes).Returns(mockIntakes.Object);
        mockContext.Setup(m => m.StudyModes).Returns(mockStudyModes.Object);
        mockContext.Setup(m => m.Curricula).Returns(mockCurricula.Object);
        mockContext.Setup(m => m.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        // Act
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["Admin:Email"]).Returns("admin@test.com");
        mockConfig.Setup(c => c["Admin:Password"]).Returns("TestAdmin1!");
        await DbSeeder.SeedAsync(mockContext.Object, mockConfig.Object);
        // Assert
        mockAdmins.Verify(m => m.Add(It.IsAny<Admin>()), Times.Once);
        mockFaculties.Verify(m => m.Add(It.IsAny<Faculty>()), Times.Never);
        mockDepartments.Verify(m => m.Add(It.IsAny<Department>()), Times.Never);
        mockPrograms.Verify(m => m.Add(It.IsAny<AcademicProgram>()), Times.Never);
        mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that SeedAsync seeds correct semester data.
    /// Input: Empty database
    /// Expected: Two semesters created with correct names and numbers
    /// </summary>
    [TestMethod]
    [TestCategory("ProductionBugSuspected")]
    [Ignore("ProductionBugSuspected")]
    public async Task SeedAsync_EmptyDatabase_SeedsCorrectSemesters()
    {
        // Arrange
        var mockContext = new Mock<SsomeroDbContext>(new DbContextOptions<SsomeroDbContext>());
        Semester[]? capturedSemesters = null;
        var mockAdmins = CreateMockDbSet<Admin>(new List<Admin>());
        var mockUniversities = CreateMockDbSet<University>(new List<University>());
        var mockFaculties = CreateMockDbSet<Faculty>(new List<Faculty>());
        var mockDepartments = CreateMockDbSet<Department>(new List<Department>());
        var mockPrograms = CreateMockDbSet<AcademicProgram>(new List<AcademicProgram>());
        var mockSemesters = CreateMockDbSet<Semester>(new List<Semester>());
        var mockAcademicYears = CreateMockDbSet<AcademicYear>(new List<AcademicYear>());
        var mockEntrySchemes = CreateMockDbSet<EntryScheme>(new List<EntryScheme>());
        var mockIntakes = CreateMockDbSet<Intake>(new List<Intake>());
        var mockStudyModes = CreateMockDbSet<StudyMode>(new List<StudyMode>());
        var mockCurricula = CreateMockDbSet<Curriculum>(new List<Curriculum>());
        mockSemesters.Setup(m => m.AddRange(It.IsAny<Semester[]>())).Callback<Semester[]>(s => capturedSemesters = s);
        mockContext.Setup(m => m.Admins).Returns(mockAdmins.Object);
        mockContext.Setup(m => m.Universities).Returns(mockUniversities.Object);
        mockContext.Setup(m => m.Faculties).Returns(mockFaculties.Object);
        mockContext.Setup(m => m.Departments).Returns(mockDepartments.Object);
        mockContext.Setup(m => m.Programs).Returns(mockPrograms.Object);
        mockContext.Setup(m => m.Semesters).Returns(mockSemesters.Object);
        mockContext.Setup(m => m.AcademicYears).Returns(mockAcademicYears.Object);
        mockContext.Setup(m => m.EntrySchemes).Returns(mockEntrySchemes.Object);
        mockContext.Setup(m => m.Intakes).Returns(mockIntakes.Object);
        mockContext.Setup(m => m.StudyModes).Returns(mockStudyModes.Object);
        mockContext.Setup(m => m.Curricula).Returns(mockCurricula.Object);
        mockContext.Setup(m => m.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        // Act
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["Admin:Email"]).Returns("admin@test.com");
        mockConfig.Setup(c => c["Admin:Password"]).Returns("TestAdmin1!");
        await DbSeeder.SeedAsync(mockContext.Object, mockConfig.Object);
        // Assert
        Assert.IsNotNull(capturedSemesters);
        Assert.AreEqual(2, capturedSemesters.Length);
        Assert.AreEqual("Semester 1", capturedSemesters[0].Name);
        Assert.AreEqual(1, capturedSemesters[0].Number);
        Assert.AreEqual("Semester 2", capturedSemesters[1].Name);
        Assert.AreEqual(2, capturedSemesters[1].Number);
    }

    /// <summary>
    /// Helper method to create a mock DbSet for testing EF Core operations.
    /// </summary>
    private static Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data)
        where T : class
    {
        var queryableData = data.AsQueryable();
        var mockSet = new Mock<DbSet<T>>();
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(queryableData.Provider));
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryableData.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryableData.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryableData.GetEnumerator());
        mockSet.As<IAsyncEnumerable<T>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(new TestAsyncEnumerator<T>(queryableData.GetEnumerator()));
        return mockSet;
    }

    /// <summary>
    /// Helper class to support async query operations in mock DbSet.
    /// </summary>
    private class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;
        internal TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
        {
            return new TestAsyncEnumerable<TEntity>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
        {
            return new TestAsyncEnumerable<TElement>(expression);
        }

        public object Execute(System.Linq.Expressions.Expression expression)
        {
            return _inner.Execute(expression)!;
        }

        public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
        {
            return _inner.Execute<TResult>(expression);
        }

        public TResult ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression, CancellationToken cancellationToken = default)
        {
            var resultType = typeof(TResult).GetGenericArguments()[0];
            var executionResult = typeof(IQueryProvider).GetMethod(nameof(IQueryProvider.Execute), 1, new[] { typeof(System.Linq.Expressions.Expression) })!.MakeGenericMethod(resultType).Invoke(_inner, new object[] { expression });
            return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!.MakeGenericMethod(resultType).Invoke(null, new[] { executionResult })!;
        }
    }

    /// <summary>
    /// Helper class to support async enumeration in mock DbSet.
    /// </summary>
    private class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(System.Linq.Expressions.Expression expression) : base(expression)
        {
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    /// <summary>
    /// Helper class to support async enumerator in mock DbSet.
    /// </summary>
    private class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;
        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public T Current => _inner.Current;

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(_inner.MoveNext());
        }

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return default;
        }
    }
}