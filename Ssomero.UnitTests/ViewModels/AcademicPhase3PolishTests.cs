using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero.Interfaces;
using Ssomero.Models;
using Ssomero.ViewModels;

namespace Ssomero.ViewModels.UnitTests;

/// <summary>
/// Phase 3 — DisplayLabel formatting, alphabetical sort, empty-state signals, validation messages.
/// </summary>
[TestClass]
public class AcademicPhase3PolishTests
{
    // ── DisplayLabel ─────────────────────────────────────────────────────────

    [TestMethod]
    public void FacultyDto_DisplayLabel_IncludesUniversityName()
    {
        var dto = new FacultyDto { Name = "Science", UniversityName = "Makerere University" };
        Assert.AreEqual("Science (Makerere University)", dto.DisplayLabel);
    }

    [TestMethod]
    public void FacultyDto_DisplayLabel_FallsBackToName_WhenUniversityMissing()
    {
        var dto = new FacultyDto { Name = "Science", UniversityName = "" };
        Assert.AreEqual("Science", dto.DisplayLabel);
    }

    [TestMethod]
    public void DepartmentDto_DisplayLabel_IncludesFacultyAndUniversity()
    {
        var dto = new DepartmentDto { Name = "Computer Science", FacultyName = "Science", UniversityName = "Makerere University" };
        Assert.AreEqual("Computer Science (Science - Makerere University)", dto.DisplayLabel);
    }

    [TestMethod]
    public void DepartmentDto_DisplayLabel_OnlyFaculty_WhenUniversityMissing()
    {
        var dto = new DepartmentDto { Name = "Computer Science", FacultyName = "Science", UniversityName = "" };
        Assert.AreEqual("Computer Science (Science)", dto.DisplayLabel);
    }

    [TestMethod]
    public void DepartmentDto_DisplayLabel_FallsBackToName_WhenBothMissing()
    {
        var dto = new DepartmentDto { Name = "Computer Science", FacultyName = "", UniversityName = "" };
        Assert.AreEqual("Computer Science", dto.DisplayLabel);
    }

    [TestMethod]
    public void ProgramDto_DisplayLabel_IncludesDepartmentName()
    {
        var dto = new ProgramDto { Name = "BSc CS", DepartmentName = "Computer Science" };
        Assert.AreEqual("BSc CS (Computer Science)", dto.DisplayLabel);
    }

    [TestMethod]
    public void CurriculumDto_DisplayLabel_FormatsCodeAndName()
    {
        var dto = new CurriculumDto { CourseCode = "CS101", CourseName = "Intro to CS" };
        Assert.AreEqual("CS101 - Intro to CS", dto.DisplayLabel);
    }

    [TestMethod]
    public void CurriculumDto_DisplayTitle_UsesEmDash()
    {
        var dto = new CurriculumDto { CourseCode = "ACC201", CourseName = "Accounting Basics" };
        Assert.AreEqual("ACC201 \u2014 Accounting Basics", dto.DisplayTitle);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static Mock<IAcademicService> BuildSortMock()
    {
        var m = new Mock<IAcademicService>();
        m.Setup(s => s.GetUniversitiesAsync())
            .ReturnsAsync(new[] { new LookupItem { Id = "u1", Name = "Uni" } });
        m.Setup(s => s.GetFacultyDetailsAsync())
            .ReturnsAsync(new List<FacultyDto>
            {
                new() { Id = "f1", Name = "Zoology",  UniversityId = "u1" },
                new() { Id = "f2", Name = "Arts",     UniversityId = "u1" },
                new() { Id = "f3", Name = "Medicine", UniversityId = "u1" }
            });
        m.Setup(s => s.GetDepartmentDetailsAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<DepartmentDto>
            {
                new() { Id = "d1", Name = "Zoology Dept",  FacultyId = "f1", UniversityId = "u1" },
                new() { Id = "d2", Name = "Arts Dept",     FacultyId = "f1", UniversityId = "u1" },
                new() { Id = "d3", Name = "Medicine Dept", FacultyId = "f1", UniversityId = "u1" }
            });
        m.Setup(s => s.GetProgramDetailsAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<ProgramDto>
            {
                new() { Id = "p1", Name = "Zoology",    DepartmentId = "d1", FacultyId = "f1", UniversityId = "u1" },
                new() { Id = "p2", Name = "Accounting", DepartmentId = "d1", FacultyId = "f1", UniversityId = "u1" },
                new() { Id = "p3", Name = "Medicine",   DepartmentId = "d1", FacultyId = "f1", UniversityId = "u1" }
            });
        m.Setup(s => s.GetCurriculumDetailsAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<CurriculumDto>());
        m.Setup(s => s.GetSemestersAsync())
            .ReturnsAsync(Array.Empty<LookupItem>());
        m.Setup(s => s.GetFacultiesByUniversityAsync(
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<FacultyDto> { Data = [], TotalCount = 0 });
        m.Setup(s => s.GetDepartmentsByFacultyAsync(
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<DepartmentDto> { Data = [], TotalCount = 0 });
        m.Setup(s => s.GetProgramsByDepartmentAsync(
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<System.Threading.CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<ProgramDto> { Data = [], TotalCount = 0 });
        return m;
    }

    private static Mock<IRefreshCoordinator> BuildRefreshMock()
    {
        var m = new Mock<IRefreshCoordinator>();
        m.Setup(r => r.Subscribe(It.IsAny<string>(), It.IsAny<Func<Task>>()));
        m.Setup(r => r.NotifyAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        return m;
    }

    // ── Sort ─────────────────────────────────────────────────────────────────

    [TestMethod]
    public async Task Departments_SortedAlphabetically_AfterLoad()
    {
        var vm = new DepartmentsViewModel(BuildSortMock().Object, BuildRefreshMock().Object,
            Mock.Of<ILogger<DepartmentsViewModel>>());
        await vm.LoadAsync();

        var names = vm.Departments.Select(d => d.Name).ToList();
        CollectionAssert.AreEqual(names.OrderBy(n => n).ToList(), names,
            "Departments should be sorted alphabetically.");
    }

    [TestMethod]
    public async Task Programs_SortedAlphabetically_AfterLoad()
    {
        var vm = new ProgramsViewModel(BuildSortMock().Object, BuildRefreshMock().Object,
            Mock.Of<ILogger<ProgramsViewModel>>());
        await vm.LoadAsync();

        var names = vm.Programs.Select(p => p.Name).ToList();
        CollectionAssert.AreEqual(names.OrderBy(n => n).ToList(), names,
            "Programs should be sorted alphabetically.");
    }

    // ── Empty state ──────────────────────────────────────────────────────────

    [TestMethod]
    public async Task Departments_IsEmpty_True_WhenNoData()
    {
        var mock = new Mock<IAcademicService>();
        mock.Setup(s => s.GetUniversitiesAsync()).ReturnsAsync(Array.Empty<LookupItem>());
        mock.Setup(s => s.GetFacultyDetailsAsync()).ReturnsAsync(new List<FacultyDto>());
        mock.Setup(s => s.GetDepartmentDetailsAsync(It.IsAny<string>())).ReturnsAsync(new List<DepartmentDto>());

        var vm = new DepartmentsViewModel(mock.Object, BuildRefreshMock().Object,
            Mock.Of<ILogger<DepartmentsViewModel>>());
        await vm.LoadAsync();

        Assert.IsTrue(vm.IsEmpty, "IsEmpty should be true when no departments exist.");
        Assert.IsFalse(vm.IsEmptySearch, "IsEmptySearch should be false when there is no data.");
    }

    [TestMethod]
    public async Task Departments_IsEmptySearch_True_WhenSearchYieldsNothing()
    {
        var vm = new DepartmentsViewModel(BuildSortMock().Object, BuildRefreshMock().Object,
            Mock.Of<ILogger<DepartmentsViewModel>>());
        await vm.LoadAsync();

        vm.SearchQuery = "zzz_no_match_xyz";

        Assert.IsFalse(vm.IsEmpty, "IsEmpty should stay false — data exists.");
        Assert.IsTrue(vm.IsEmptySearch, "IsEmptySearch should be true when search yields nothing.");
    }

    [TestMethod]
    public async Task Programs_IsEmptySearch_True_WhenSearchYieldsNothing()
    {
        var vm = new ProgramsViewModel(BuildSortMock().Object, BuildRefreshMock().Object,
            Mock.Of<ILogger<ProgramsViewModel>>());
        await vm.LoadAsync();

        vm.SearchQuery = "zzz_no_match_xyz";

        Assert.IsTrue(vm.IsEmptySearch);
        Assert.IsFalse(vm.IsEmpty);
    }

    // ── Validation messages ───────────────────────────────────────────────────

    [TestMethod]
    public async Task Departments_SaveWithoutFaculty_ShowsContextualMessage()
    {
        var vm = new DepartmentsViewModel(BuildSortMock().Object, BuildRefreshMock().Object,
            Mock.Of<ILogger<DepartmentsViewModel>>());
        vm.EditName = "New Dept";
        vm.SelectedFaculty = null;

        await Task.Run(() => ((Command)vm.SaveCommand).Execute(null));
        await Task.Delay(50);

        StringAssert.Contains(vm.ErrorMessage.ToLower(), "faculty");
    }

    [TestMethod]
    public async Task Programs_SaveWithoutDepartment_ShowsContextualMessage()
    {
        var vm = new ProgramsViewModel(BuildSortMock().Object, BuildRefreshMock().Object,
            Mock.Of<ILogger<ProgramsViewModel>>());
        vm.EditName = "New Prog";
        vm.SelectedDepartment = null;

        await Task.Run(() => ((Command)vm.SaveCommand).Execute(null));
        await Task.Delay(50);

        StringAssert.Contains(vm.ErrorMessage.ToLower(), "department");
    }
}
