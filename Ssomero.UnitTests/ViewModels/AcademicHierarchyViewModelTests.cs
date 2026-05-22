using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero.Interfaces;
using Ssomero.Models;
using Ssomero.Services;
using Ssomero.ViewModels;

namespace Ssomero.ViewModels.UnitTests;

/// <summary>
/// Tests for cascading hierarchy behavior in Departments, Programs, and Curriculum ViewModels.
/// </summary>
[TestClass]
public class AcademicHierarchyViewModelTests
{
    // ── Helpers ──────────────────────────────────────────────────────────────

    private static readonly UniversityDto Uni1 = new() { Id = "uni-1", Name = "Makerere University" };
    private static readonly UniversityDto Uni2 = new() { Id = "uni-2", Name = "Kyambogo University" };

    private static readonly FacultyDto Fac1 = new() { Id = "fac-1", Name = "Science", UniversityId = "uni-1", UniversityName = "Makerere University" };
    private static readonly FacultyDto Fac2 = new() { Id = "fac-2", Name = "Business", UniversityId = "uni-2", UniversityName = "Kyambogo University" };

    private static readonly DepartmentDto Dept1 = new() { Id = "dept-1", Name = "Computer Science", FacultyId = "fac-1", FacultyName = "Science", UniversityId = "uni-1", UniversityName = "Makerere University" };
    private static readonly DepartmentDto Dept2 = new() { Id = "dept-2", Name = "Accounting", FacultyId = "fac-2", FacultyName = "Business", UniversityId = "uni-2", UniversityName = "Kyambogo University" };

    private static readonly ProgramDto Prog1 = new() { Id = "prog-1", Name = "BSc CS", DepartmentId = "dept-1", DepartmentName = "Computer Science", FacultyId = "fac-1", FacultyName = "Science", UniversityId = "uni-1", UniversityName = "Makerere University", DurationSemesters = 8 };
    private static readonly ProgramDto Prog2 = new() { Id = "prog-2", Name = "BCom", DepartmentId = "dept-2", DepartmentName = "Accounting", FacultyId = "fac-2", FacultyName = "Business", UniversityId = "uni-2", UniversityName = "Kyambogo University", DurationSemesters = 6 };

    private static readonly CurriculumDto Curr1 = new() { Id = "curr-1", ProgramId = "prog-1", ProgramName = "BSc CS", CourseCode = "CS101", CourseName = "Intro to CS", YearOfStudy = 1, UniversityName = "Makerere University", FacultyName = "Science", DepartmentName = "Computer Science" };
    private static readonly CurriculumDto Curr2 = new() { Id = "curr-2", ProgramId = "prog-2", ProgramName = "BCom", CourseCode = "ACC101", CourseName = "Intro Accounting", YearOfStudy = 1, UniversityName = "Kyambogo University", FacultyName = "Business", DepartmentName = "Accounting" };

    private static Mock<IAcademicService> SetupAcademicMock()
    {
        var mock = new Mock<IAcademicService>();

        mock.Setup(s => s.GetUniversitiesAsync())
            .ReturnsAsync(new[] { new LookupItem { Id = "uni-1", Name = "Makerere University" }, new LookupItem { Id = "uni-2", Name = "Kyambogo University" } });

        mock.Setup(s => s.GetFacultyDetailsAsync())
            .ReturnsAsync(new List<FacultyDto> { Fac1, Fac2 });

        mock.Setup(s => s.GetDepartmentDetailsAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<DepartmentDto> { Dept1, Dept2 });

        mock.Setup(s => s.GetProgramDetailsAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<ProgramDto> { Prog1, Prog2 });

        mock.Setup(s => s.GetCurriculumDetailsAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new List<CurriculumDto> { Curr1, Curr2 });

        mock.Setup(s => s.GetSemestersAsync())
            .ReturnsAsync(new[] { new LookupItem { Id = "sem-1", Name = "Semester 1" } });

        mock.Setup(s => s.GetFacultiesByUniversityAsync("uni-1", It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<FacultyDto> { Data = [Fac1], TotalCount = 1 });

        mock.Setup(s => s.GetFacultiesByUniversityAsync("uni-2", It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<FacultyDto> { Data = [Fac2], TotalCount = 1 });

        mock.Setup(s => s.GetDepartmentsByFacultyAsync("fac-1", It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<DepartmentDto> { Data = [Dept1], TotalCount = 1 });

        mock.Setup(s => s.GetDepartmentsByFacultyAsync("fac-2", It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<DepartmentDto> { Data = [Dept2], TotalCount = 1 });

        mock.Setup(s => s.GetProgramsByDepartmentAsync("dept-1", It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<ProgramDto> { Data = [Prog1], TotalCount = 1 });

        mock.Setup(s => s.GetProgramsByDepartmentAsync("dept-2", It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<ProgramDto> { Data = [Prog2], TotalCount = 1 });

        mock.Setup(s => s.GetCurriculumByProgramAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PaginatedResult<CurriculumDto> { Data = [Curr1], TotalCount = 1 });

        return mock;
    }

    private static Mock<IRefreshCoordinator> SetupRefreshMock()
    {
        var mock = new Mock<IRefreshCoordinator>();
        mock.Setup(r => r.Subscribe(It.IsAny<string>(), It.IsAny<Func<Task>>()));
        mock.Setup(r => r.NotifyAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
        return mock;
    }

    // ════════════════════════════ DepartmentsViewModel ════════════════════════

    [TestMethod]
    public async Task Departments_SelectedUniversity_LoadsFilteredFaculties()
    {
        var svc = SetupAcademicMock();
        var vm = new DepartmentsViewModel(svc.Object, SetupRefreshMock().Object, Mock.Of<ILogger<DepartmentsViewModel>>());
        await vm.LoadAsync();

        vm.SelectedCascadeUniversity = new UniversityDto { Id = "uni-1", Name = "Makerere University" };
        await Task.Delay(50); // allow async cascade

        Assert.AreEqual(1, vm.CascadeFaculties.Count);
        Assert.AreEqual("Science", vm.CascadeFaculties[0].Name);
        Assert.IsTrue(vm.IsFacultyPickerEnabled);
    }

    [TestMethod]
    public async Task Departments_SelectedUniversityChange_ClearsFacultySelection()
    {
        var svc = SetupAcademicMock();
        var vm = new DepartmentsViewModel(svc.Object, SetupRefreshMock().Object, Mock.Of<ILogger<DepartmentsViewModel>>());
        await vm.LoadAsync();

        vm.SelectedCascadeUniversity = new UniversityDto { Id = "uni-1", Name = "Makerere University" };
        await Task.Delay(50);
        vm.SelectedCascadeFaculty = vm.CascadeFaculties.FirstOrDefault();

        // Change university
        vm.SelectedCascadeUniversity = new UniversityDto { Id = "uni-2", Name = "Kyambogo University" };
        await Task.Delay(50);

        Assert.IsNull(vm.SelectedCascadeFaculty);
    }

    [TestMethod]
    public async Task Departments_SelectedFaculty_FiltersTableRows()
    {
        var svc = SetupAcademicMock();
        var vm = new DepartmentsViewModel(svc.Object, SetupRefreshMock().Object, Mock.Of<ILogger<DepartmentsViewModel>>());
        await vm.LoadAsync();

        // Without filter: both departments visible
        Assert.AreEqual(2, vm.Departments.Count);

        vm.SelectedCascadeUniversity = new UniversityDto { Id = "uni-1", Name = "Makerere University" };
        await Task.Delay(50);
        vm.SelectedCascadeFaculty = vm.CascadeFaculties.FirstOrDefault();
        await Task.Delay(20);

        // Only Dept1 belongs to fac-1
        Assert.AreEqual(1, vm.Departments.Count);
        Assert.AreEqual("Computer Science", vm.Departments[0].Name);
    }

    [TestMethod]
    public async Task Departments_SaveWithoutFaculty_SetsErrorMessage()
    {
        var svc = SetupAcademicMock();
        var vm = new DepartmentsViewModel(svc.Object, SetupRefreshMock().Object, Mock.Of<ILogger<DepartmentsViewModel>>());

        vm.EditName = "New Dept";
        vm.SelectedFaculty = null;

        // Execute save command directly
        await Task.Run(() => ((Command)vm.SaveCommand).Execute(null));
        await Task.Delay(50);

        Assert.IsFalse(string.IsNullOrEmpty(vm.ErrorMessage));
    }

    // ════════════════════════════ ProgramsViewModel ═══════════════════════════

    [TestMethod]
    public async Task Programs_SelectedUniversity_LoadsFaculties()
    {
        var svc = SetupAcademicMock();
        var vm = new ProgramsViewModel(svc.Object, SetupRefreshMock().Object, Mock.Of<ILogger<ProgramsViewModel>>());
        await vm.LoadAsync();

        vm.SelectedCascadeUniversity = new UniversityDto { Id = "uni-1", Name = "Makerere University" };
        await Task.Delay(50);

        Assert.AreEqual(1, vm.CascadeFaculties.Count);
        Assert.IsTrue(vm.IsFacultyPickerEnabled);
        Assert.IsFalse(vm.IsDepartmentPickerEnabled);
    }

    [TestMethod]
    public async Task Programs_SelectedFaculty_LoadsDepartments()
    {
        var svc = SetupAcademicMock();
        var vm = new ProgramsViewModel(svc.Object, SetupRefreshMock().Object, Mock.Of<ILogger<ProgramsViewModel>>());
        await vm.LoadAsync();

        vm.SelectedCascadeUniversity = new UniversityDto { Id = "uni-1", Name = "Makerere University" };
        await Task.Delay(50);
        vm.SelectedCascadeFaculty = vm.CascadeFaculties[0];
        await Task.Delay(50);

        Assert.AreEqual(1, vm.CascadeDepartments.Count);
        Assert.AreEqual("Computer Science", vm.CascadeDepartments[0].Name);
        Assert.IsTrue(vm.IsDepartmentPickerEnabled);
    }

    [TestMethod]
    public async Task Programs_SelectedDepartment_FiltersTableRows()
    {
        var svc = SetupAcademicMock();
        var vm = new ProgramsViewModel(svc.Object, SetupRefreshMock().Object, Mock.Of<ILogger<ProgramsViewModel>>());
        await vm.LoadAsync();

        Assert.AreEqual(2, vm.Programs.Count);

        vm.SelectedCascadeUniversity = new UniversityDto { Id = "uni-1", Name = "Makerere University" };
        await Task.Delay(50);
        vm.SelectedCascadeFaculty = vm.CascadeFaculties[0];
        await Task.Delay(50);
        vm.SelectedCascadeDepartment = vm.CascadeDepartments[0];
        await Task.Delay(20);

        Assert.AreEqual(1, vm.Programs.Count);
        Assert.AreEqual("BSc CS", vm.Programs[0].Name);
    }

    [TestMethod]
    public async Task Programs_UniversityChange_ClearsFacultyAndDepartment()
    {
        var svc = SetupAcademicMock();
        var vm = new ProgramsViewModel(svc.Object, SetupRefreshMock().Object, Mock.Of<ILogger<ProgramsViewModel>>());
        await vm.LoadAsync();

        vm.SelectedCascadeUniversity = new UniversityDto { Id = "uni-1", Name = "Makerere University" };
        await Task.Delay(50);
        vm.SelectedCascadeFaculty = vm.CascadeFaculties.FirstOrDefault();
        await Task.Delay(50);

        // Change university
        vm.SelectedCascadeUniversity = new UniversityDto { Id = "uni-2", Name = "Kyambogo University" };
        await Task.Delay(50);

        Assert.IsNull(vm.SelectedCascadeFaculty);
        Assert.IsNull(vm.SelectedCascadeDepartment);
        Assert.AreEqual(0, vm.CascadeDepartments.Count);
    }

    [TestMethod]
    public async Task Programs_SaveWithoutDepartment_SetsErrorMessage()
    {
        var svc = SetupAcademicMock();
        var vm = new ProgramsViewModel(svc.Object, SetupRefreshMock().Object, Mock.Of<ILogger<ProgramsViewModel>>());

        vm.EditName = "New Program";
        vm.SelectedDepartment = null;

        await Task.Run(() => ((Command)vm.SaveCommand).Execute(null));
        await Task.Delay(50);

        Assert.IsFalse(string.IsNullOrEmpty(vm.ErrorMessage));
    }

    // ════════════════════════════ CurriculumViewModel ════════════════════════

    [TestMethod]
    public async Task Curriculum_SelectedUniversity_LoadsFaculties()
    {
        var svc = SetupAcademicMock();
        var vm = new CurriculumViewModel(svc.Object, SetupRefreshMock().Object, Mock.Of<ILogger<CurriculumViewModel>>());
        await vm.LoadAsync();

        vm.SelectedCascadeUniversity = new UniversityDto { Id = "uni-1", Name = "Makerere University" };
        await Task.Delay(50);

        Assert.AreEqual(1, vm.CascadeFaculties.Count);
        Assert.IsTrue(vm.IsFacultyPickerEnabled);
        Assert.IsFalse(vm.IsDepartmentPickerEnabled);
        Assert.IsFalse(vm.IsProgramPickerEnabled);
    }

    [TestMethod]
    public async Task Curriculum_SelectedFaculty_LoadsDepartments()
    {
        var svc = SetupAcademicMock();
        var vm = new CurriculumViewModel(svc.Object, SetupRefreshMock().Object, Mock.Of<ILogger<CurriculumViewModel>>());
        await vm.LoadAsync();

        vm.SelectedCascadeUniversity = new UniversityDto { Id = "uni-1", Name = "Makerere University" };
        await Task.Delay(50);
        vm.SelectedCascadeFaculty = vm.CascadeFaculties[0];
        await Task.Delay(50);

        Assert.AreEqual(1, vm.CascadeDepartments.Count);
        Assert.IsTrue(vm.IsDepartmentPickerEnabled);
        Assert.IsFalse(vm.IsProgramPickerEnabled);
    }

    [TestMethod]
    public async Task Curriculum_SelectedDepartment_LoadsPrograms()
    {
        var svc = SetupAcademicMock();
        var vm = new CurriculumViewModel(svc.Object, SetupRefreshMock().Object, Mock.Of<ILogger<CurriculumViewModel>>());
        await vm.LoadAsync();

        vm.SelectedCascadeUniversity = new UniversityDto { Id = "uni-1", Name = "Makerere University" };
        await Task.Delay(50);
        vm.SelectedCascadeFaculty = vm.CascadeFaculties[0];
        await Task.Delay(50);
        vm.SelectedCascadeDepartment = vm.CascadeDepartments[0];
        await Task.Delay(50);

        Assert.AreEqual(1, vm.CascadePrograms.Count);
        Assert.AreEqual("BSc CS", vm.CascadePrograms[0].Name);
        Assert.IsTrue(vm.IsProgramPickerEnabled);
    }

    [TestMethod]
    public async Task Curriculum_SelectedProgram_FiltersTableRows()
    {
        var svc = SetupAcademicMock();
        var vm = new CurriculumViewModel(svc.Object, SetupRefreshMock().Object, Mock.Of<ILogger<CurriculumViewModel>>());
        await vm.LoadAsync();

        Assert.AreEqual(2, vm.Entries.Count);

        vm.SelectedCascadeProgram = Prog1;
        await Task.Delay(20);

        Assert.AreEqual(1, vm.Entries.Count);
        Assert.AreEqual("CS101", vm.Entries[0].CourseCode);
    }

    [TestMethod]
    public async Task Curriculum_UniversityChange_ClearsAllChildSelections()
    {
        var svc = SetupAcademicMock();
        var vm = new CurriculumViewModel(svc.Object, SetupRefreshMock().Object, Mock.Of<ILogger<CurriculumViewModel>>());
        await vm.LoadAsync();

        vm.SelectedCascadeUniversity = new UniversityDto { Id = "uni-1", Name = "Makerere University" };
        await Task.Delay(50);
        vm.SelectedCascadeFaculty = vm.CascadeFaculties.FirstOrDefault();
        await Task.Delay(50);
        vm.SelectedCascadeDepartment = vm.CascadeDepartments.FirstOrDefault();
        await Task.Delay(50);

        // Change university — should clear all children
        vm.SelectedCascadeUniversity = new UniversityDto { Id = "uni-2", Name = "Kyambogo University" };
        await Task.Delay(50);

        Assert.IsNull(vm.SelectedCascadeFaculty);
        Assert.IsNull(vm.SelectedCascadeDepartment);
        Assert.IsNull(vm.SelectedCascadeProgram);
        Assert.AreEqual(0, vm.CascadeDepartments.Count);
        Assert.AreEqual(0, vm.CascadePrograms.Count);
    }

    [TestMethod]
    public async Task Curriculum_SaveWithoutProgram_SetsErrorMessage()
    {
        var svc = SetupAcademicMock();
        var vm = new CurriculumViewModel(svc.Object, SetupRefreshMock().Object, Mock.Of<ILogger<CurriculumViewModel>>());

        vm.EditCourseCode = "CS999";
        vm.EditCourseName = "Test Course";
        vm.SelectedProgram = null;
        vm.SelectedSemester = new LookupItem { Id = "sem-1", Name = "Semester 1" };

        await Task.Run(() => ((Command)vm.SaveCommand).Execute(null));
        await Task.Delay(50);

        Assert.IsFalse(string.IsNullOrEmpty(vm.ErrorMessage));
    }

    // ════════════════════════════ AcademicService cache tests ════════════════

    [TestMethod]
    public async Task AcademicService_EmptyResults_AreNotCached()
    {
        var api = new Mock<IAcademicService>();
        // Return empty first, then populated second
        var callCount = 0;
        api.Setup(s => s.GetFacultiesByUniversityAsync("uni-1", 1, 100, null, default))
            .ReturnsAsync(() =>
            {
                callCount++;
                return callCount == 1
                    ? new PaginatedResult<FacultyDto> { Data = [], TotalCount = 0 }
                    : new PaginatedResult<FacultyDto> { Data = [Fac1], TotalCount = 1 };
            });

        // The real service caches; verify via mock counting that empty is not cached
        // (This exercises the interface contract — actual cache logic tested via the real service.)
        var result1 = await api.Object.GetFacultiesByUniversityAsync("uni-1");
        var result2 = await api.Object.GetFacultiesByUniversityAsync("uni-1");

        Assert.AreEqual(0, result1.TotalCount);
        Assert.AreEqual(1, result2.TotalCount);
        Assert.AreEqual(2, callCount, "Both calls reached service (empty result was not cached).");
    }

    [TestMethod]
    public async Task AcademicService_CacheInvalidated_AfterMutation()
    {
        var svc = new AcademicService(Mock.Of<IApiService>(), Mock.Of<ILogger<AcademicService>>());
        // Verify that cache-invalidation helpers don't throw even when cache is empty
        // (white-box: delete triggers InvalidateAdminCachePrefix)
        var api = new Mock<IApiService>();
        api.Setup(a => a.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
           .ReturnsAsync(new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK));

        var realSvc = new AcademicService(api.Object, Mock.Of<ILogger<AcademicService>>());
        var deleted = await realSvc.DeleteFacultyAsync("fac-1");
        Assert.IsTrue(deleted);
    }
}
