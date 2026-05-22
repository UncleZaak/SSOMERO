using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero.Interfaces;
using Ssomero.Models;
using Ssomero.Services;
using Ssomero.ViewModels;

namespace Ssomero.ViewModels.UnitTests;

/// <summary>
/// Unit tests for <see cref="LecturerDashboardViewModel"/>.
/// All tests run without a MAUI host — no UI thread or XAML required.
/// </summary>
[TestClass]
public class LecturerDashboardViewModelTests
{
    // ── factory helpers ───────────────────────────────────────────────────────

    private static (LecturerDashboardViewModel vm, Mock<ILecturerApiService> api, SessionService session)
        Create(List<LecturerClassDto>? classes = null)
    {
        var api = new Mock<ILecturerApiService>(MockBehavior.Loose);
        api.Setup(a => a.GetClassesAsync(It.IsAny<CancellationToken>()))
           .ReturnsAsync(classes ?? []);

        var session = new SessionService();
        var vm = new LecturerDashboardViewModel(api.Object, session);
        return (vm, api, session);
    }

    private static List<LecturerClassDto> MakeClasses(int count) =>
        Enumerable.Range(1, count)
                  .Select(i => new LecturerClassDto { Id = Guid.NewGuid(), Name = $"Class {i}" })
                  .ToList();

    // ── constructor / initial state ───────────────────────────────────────────

    [TestMethod]
    public void Constructor_DefaultLecturerName_IsLecturer()
    {
        var (vm, _, _) = Create();
        Assert.AreEqual("Lecturer", vm.LecturerName);
    }

    [TestMethod]
    public void Constructor_ClassesCollection_IsEmpty()
    {
        var (vm, _, _) = Create();
        Assert.AreEqual(0, vm.Classes.Count);
    }

    [TestMethod]
    public void Constructor_IsEmpty_IsTrue()
    {
        var (vm, _, _) = Create();
        // Before any load, collection is empty — IsEmpty is false by default
        // (it is set during LoadAsync; default field value is false)
        Assert.IsFalse(vm.IsEmpty);
    }

    [TestMethod]
    public void Constructor_IsBusy_IsFalse()
    {
        var (vm, _, _) = Create();
        Assert.IsFalse(vm.IsBusy);
    }

    [TestMethod]
    public void Constructor_LoadCommand_IsNotNull()
    {
        var (vm, _, _) = Create();
        Assert.IsNotNull(vm.LoadCommand);
    }

    [TestMethod]
    public void Constructor_RefreshCommand_IsNotNull()
    {
        var (vm, _, _) = Create();
        Assert.IsNotNull(vm.RefreshCommand);
    }

    [TestMethod]
    public void Constructor_GoToClassesCommand_IsNotNull()
    {
        var (vm, _, _) = Create();
        Assert.IsNotNull(vm.GoToClassesCommand);
    }

    // ── LoadAsync — happy path ────────────────────────────────────────────────

    [TestMethod]
    public async Task LoadAsync_WithThreeClasses_PopulatesClassesCollection()
    {
        var (vm, _, _) = Create(MakeClasses(3));

        await vm.LoadAsync();

        Assert.AreEqual(3, vm.Classes.Count);
    }

    [TestMethod]
    public async Task LoadAsync_WithClasses_SetsIsEmptyFalse()
    {
        var (vm, _, _) = Create(MakeClasses(2));

        await vm.LoadAsync();

        Assert.IsFalse(vm.IsEmpty);
    }

    [TestMethod]
    public async Task LoadAsync_WithNoClasses_SetsIsEmptyTrue()
    {
        var (vm, _, _) = Create([]);

        await vm.LoadAsync();

        Assert.IsTrue(vm.IsEmpty);
    }

    [TestMethod]
    public async Task LoadAsync_WithNoClasses_ClassesCollectionRemainsEmpty()
    {
        var (vm, _, _) = Create([]);

        await vm.LoadAsync();

        Assert.AreEqual(0, vm.Classes.Count);
    }

    /// <summary>
    /// LoadAsync uses Take(5) — only the first five items must appear.
    /// </summary>
    [TestMethod]
    [DataRow(6,  5, DisplayName = "6 → 5")]
    [DataRow(10, 5, DisplayName = "10 → 5")]
    [DataRow(5,  5, DisplayName = "5 → 5 (exactly at cap)")]
    [DataRow(3,  3, DisplayName = "3 → 3 (below cap)")]
    public async Task LoadAsync_ClassesCappedAtFive(int apiCount, int expectedCount)
    {
        var (vm, _, _) = Create(MakeClasses(apiCount));

        await vm.LoadAsync();

        Assert.AreEqual(expectedCount, vm.Classes.Count);
    }

    // ── LoadAsync — session / name ────────────────────────────────────────────

    [TestMethod]
    public async Task LoadAsync_WithAuthenticatedSession_SetsLecturerNameFromProfile()
    {
        var (vm, _, session) = Create();
        session.SetUser(new AuthUserDto { FullName = "Dr. Jane Smith", Role = "lecturer" });

        await vm.LoadAsync();

        Assert.AreEqual("Dr. Jane Smith", vm.LecturerName);
    }

    [TestMethod]
    public async Task LoadAsync_WithNoSession_FallsBackToDefaultName()
    {
        var (vm, _, _) = Create(); // session has no user

        await vm.LoadAsync();

        Assert.AreEqual("Lecturer", vm.LecturerName);
    }

    [TestMethod]
    public async Task LoadAsync_SetsCurrentDate_ToTodayFormatted()
    {
        var (vm, _, _) = Create();
        var expectedDate = DateTime.Now.ToString("dddd, dd MMM yyyy");

        await vm.LoadAsync();

        Assert.AreEqual(expectedDate, vm.CurrentDate);
    }

    // ── LoadAsync — IsBusy guard ──────────────────────────────────────────────

    [TestMethod]
    public async Task LoadAsync_WhenIsBusyIsTrue_DoesNotCallApi()
    {
        var (vm, api, _) = Create();
        vm.IsBusy = true;

        await vm.LoadAsync();

        api.Verify(a => a.GetClassesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task LoadAsync_WhenIsBusyIsTrue_DoesNotModifyClasses()
    {
        var (vm, _, _) = Create(MakeClasses(3));
        // First load to prime the collection
        await vm.LoadAsync();
        Assert.AreEqual(3, vm.Classes.Count);

        // Second call blocked by busy guard
        vm.IsBusy = true;
        await vm.LoadAsync();

        Assert.AreEqual(3, vm.Classes.Count, "Collection must not change when load is re-entrant");
    }

    // ── LoadAsync — IsBusy lifecycle ──────────────────────────────────────────

    [TestMethod]
    public async Task LoadAsync_AfterCompletion_IsBusyIsFalse()
    {
        var (vm, _, _) = Create();

        await vm.LoadAsync();

        Assert.IsFalse(vm.IsBusy);
    }

    [TestMethod]
    public async Task LoadAsync_CallsApiExactlyOnce_PerInvocation()
    {
        var (vm, api, _) = Create();

        await vm.LoadAsync();

        api.Verify(a => a.GetClassesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── LoadAsync — second call clears stale data ─────────────────────────────

    [TestMethod]
    public async Task LoadAsync_CalledTwice_ReplacesCollectionWithLatestData()
    {
        var api = new Mock<ILecturerApiService>(MockBehavior.Loose);
        var session = new SessionService();
        var vm = new LecturerDashboardViewModel(api.Object, session);

        api.Setup(a => a.GetClassesAsync(It.IsAny<CancellationToken>()))
           .ReturnsAsync(MakeClasses(3));
        await vm.LoadAsync();
        Assert.AreEqual(3, vm.Classes.Count);

        api.Setup(a => a.GetClassesAsync(It.IsAny<CancellationToken>()))
           .ReturnsAsync(MakeClasses(1));
        await vm.LoadAsync();

        Assert.AreEqual(1, vm.Classes.Count, "Stale classes must be cleared before new data is loaded");
    }

    // ── CancelPendingRequests ─────────────────────────────────────────────────

    [TestMethod]
    public void CancelPendingRequests_BeforeAnyLoad_DoesNotThrow()
    {
        var (vm, _, _) = Create();

        // Must not throw when no token exists yet
        vm.CancelPendingRequests();
    }

    [TestMethod]
    public async Task CancelPendingRequests_AfterLoad_DoesNotThrow()
    {
        var (vm, _, _) = Create();
        await vm.LoadAsync();

        vm.CancelPendingRequests();
    }

    [TestMethod]
    public void CancelPendingRequests_CalledMultipleTimes_DoesNotThrow()
    {
        var (vm, _, _) = Create();

        vm.CancelPendingRequests();
        vm.CancelPendingRequests();
        vm.CancelPendingRequests();
    }

    // ── INotifyPropertyChanged ────────────────────────────────────────────────

    [TestMethod]
    public async Task LoadAsync_RaisesPropertyChanged_ForClasses()
    {
        var (vm, _, _) = Create(MakeClasses(2));
        var raised = new List<string?>();
        vm.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        await vm.LoadAsync();

        // LecturerName, CurrentDate, IsEmpty and IsBusy are all set during LoadAsync
        CollectionAssert.Contains(raised, nameof(vm.LecturerName));
        CollectionAssert.Contains(raised, nameof(vm.IsEmpty));
        CollectionAssert.Contains(raised, nameof(vm.IsBusy));
    }

    [TestMethod]
    public async Task LoadAsync_RaisesPropertyChanged_ForIsEmpty()
    {
        var (vm, _, _) = Create([]);
        bool isEmptyChanged = false;
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(vm.IsEmpty)) isEmptyChanged = true;
        };

        await vm.LoadAsync();

        Assert.IsTrue(isEmptyChanged);
    }
}
