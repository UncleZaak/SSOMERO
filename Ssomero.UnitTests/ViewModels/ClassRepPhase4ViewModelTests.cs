using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero.Interfaces;
using Ssomero.Models;
using Ssomero.ViewModels;

namespace Ssomero.ViewModels.UnitTests;

// ══════════════════════════════════════════════════════════════════════════════
// ClassRepAnnouncementsViewModelTests
// ══════════════════════════════════════════════════════════════════════════════

[TestClass]
public class ClassRepAnnouncementsViewModelTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static Mock<IClassAnnouncementApiService> AnnouncementMock(
        List<ClassAnnouncementModel>? announcements = null) =>
        new Mock<IClassAnnouncementApiService>()
            .Also(m => m
                .Setup(s => s.GetAnnouncementsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(announcements ?? []));

    private static Mock<IClassRepApiService> ClassRepMock(
        List<ClassRepSubclassModel>? subclasses = null) =>
        new Mock<IClassRepApiService>()
            .Also(m => m
                .Setup(s => s.GetSubclassesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(subclasses ?? []));

    private static ClassRepAnnouncementsViewModel Build(
        Mock<IClassAnnouncementApiService>? am = null,
        Mock<IClassRepApiService>? cm = null) =>
        new((am ?? AnnouncementMock()).Object, (cm ?? ClassRepMock()).Object);

    // ── Initial state ─────────────────────────────────────────────────────────

    [TestMethod]
    public void InitialState_IsCorrect()
    {
        var vm = Build();

        Assert.IsFalse(vm.IsBusy);
        Assert.IsEmpty(vm.Announcements);
        Assert.AreEqual(0, vm.AvailableClasses.Count);
        Assert.IsFalse(vm.HasError);
        Assert.AreEqual("Announcements", vm.Title);
        Assert.IsTrue(vm.IsEmpty);
    }

    // ── LoadAsync ─────────────────────────────────────────────────────────────

    [TestMethod]
    public async Task LoadAsync_PopulatesCollections()
    {
        var announcements = new List<ClassAnnouncementModel>
        {
            new() { Id = Guid.NewGuid(), Title = "Week 1 Notice", Message = "Hello", CreatedAt = DateTime.UtcNow },
        };
        var subclasses = new List<ClassRepSubclassModel>
        {
            new() { Id = Guid.NewGuid(), Name = "Group A" },
        };

        var vm = Build(AnnouncementMock(announcements), ClassRepMock(subclasses));
        await vm.LoadAsync();

        Assert.AreEqual(1, vm.Announcements.Count);
        Assert.AreEqual(1, vm.AvailableClasses.Count);
        Assert.IsFalse(vm.HasError);
        Assert.IsFalse(vm.IsBusy);
    }

    [TestMethod]
    public async Task LoadAsync_WhenBusy_DoesNotReenter()
    {
        int callCount = 0;
        var tcs = new TaskCompletionSource<List<ClassAnnouncementModel>>();

        var am = new Mock<IClassAnnouncementApiService>();
        am.Setup(s => s.GetAnnouncementsAsync(It.IsAny<CancellationToken>()))
          .Returns(() => { callCount++; return tcs.Task; });

        var cm = ClassRepMock();
        var vm = new ClassRepAnnouncementsViewModel(am.Object, cm.Object);
        var t1 = vm.LoadAsync();           // starts, increments callCount, blocks on tcs
        await Task.Yield();                // yield so t1 sets IsBusy = true
        var t2 = vm.LoadAsync();           // IsBusy guard: returns immediately
        tcs.SetResult([]);
        await Task.WhenAll(t1, t2);

        Assert.AreEqual(1, callCount);
    }

    // ── Validation ────────────────────────────────────────────────────────────

    [TestMethod]
    public async Task CreateAnnouncement_EmptyTitle_DoesNotCallApi()
    {
        var am = AnnouncementMock();
        var vm = Build(am);
        vm.AnnouncementTitle   = string.Empty;
        vm.AnnouncementMessage = "Hello";
        vm.SelectedClass       = new ClassRepSubclassModel { Id = Guid.NewGuid(), Name = "A" };

        vm.CreateAnnouncementCommand.Execute(null);
        await Task.Delay(50);

        am.Verify(s => s.CreateAnnouncementAsync(It.IsAny<CreateClassAnnouncementRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task CreateAnnouncement_EmptyMessage_DoesNotCallApi()
    {
        var am = AnnouncementMock();
        var vm = Build(am);
        vm.AnnouncementTitle   = "Title";
        vm.AnnouncementMessage = string.Empty;
        vm.SelectedClass       = new ClassRepSubclassModel { Id = Guid.NewGuid(), Name = "A" };

        vm.CreateAnnouncementCommand.Execute(null);
        await Task.Delay(50);

        am.Verify(s => s.CreateAnnouncementAsync(It.IsAny<CreateClassAnnouncementRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task CreateAnnouncement_NullClass_DoesNotCallApi()
    {
        var am = AnnouncementMock();
        var vm = Build(am);
        vm.AnnouncementTitle   = "Title";
        vm.AnnouncementMessage = "Message";
        vm.SelectedClass       = null;

        vm.CreateAnnouncementCommand.Execute(null);
        await Task.Delay(50);

        am.Verify(s => s.CreateAnnouncementAsync(It.IsAny<CreateClassAnnouncementRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── Create success ────────────────────────────────────────────────────────

    [TestMethod]
    public async Task CreateAnnouncement_Success_AddsToTopOfCollection_ClearsForm()
    {
        var newItem = new ClassAnnouncementModel
        {
            Id = Guid.NewGuid(), Title = "New Notice", Message = "Text", CreatedAt = DateTime.UtcNow,
        };
        var am = AnnouncementMock();
        am.Setup(s => s.CreateAnnouncementAsync(It.IsAny<CreateClassAnnouncementRequest>(), It.IsAny<CancellationToken>()))
          .ReturnsAsync(newItem);

        var vm = Build(am);
        await vm.LoadAsync();

        vm.AnnouncementTitle   = "New Notice";
        vm.AnnouncementMessage = "Text";
        vm.SelectedClass       = new ClassRepSubclassModel { Id = Guid.NewGuid(), Name = "B" };

        vm.CreateAnnouncementCommand.Execute(null);
        await Task.Delay(100);

        Assert.AreEqual(1, vm.Announcements.Count);
        Assert.AreEqual("New Notice", vm.Announcements[0].Title);
        Assert.AreEqual(string.Empty, vm.AnnouncementTitle);
        Assert.AreEqual(string.Empty, vm.AnnouncementMessage);
        Assert.IsNull(vm.SelectedClass);
    }

    [TestMethod]
    public async Task CreateAnnouncement_ApiReturnsNull_DoesNotAdd()
    {
        var am = AnnouncementMock();
        am.Setup(s => s.CreateAnnouncementAsync(It.IsAny<CreateClassAnnouncementRequest>(), It.IsAny<CancellationToken>()))
          .ReturnsAsync((ClassAnnouncementModel?)null);

        var vm = Build(am);
        vm.AnnouncementTitle   = "X";
        vm.AnnouncementMessage = "Y";
        vm.SelectedClass       = new ClassRepSubclassModel { Id = Guid.NewGuid() };

        vm.CreateAnnouncementCommand.Execute(null);
        await Task.Delay(100);

            Assert.AreEqual(0, vm.Announcements.Count);
    }

    // ── CancelPendingRequests ─────────────────────────────────────────────────

    [TestMethod]
    public void CancelPendingRequests_DoesNotThrow()
    {
        var vm = Build();
        vm.CancelPendingRequests();
    }

    [TestMethod]
    public async Task CancelPendingRequests_AfterLoad_DoesNotThrow()
    {
        var vm = Build();
        await vm.LoadAsync();
        vm.CancelPendingRequests();
    }

    // ── Commands not null ─────────────────────────────────────────────────────

    [TestMethod]
    public void AllCommands_AreNotNull()
    {
        var vm = Build();
        Assert.IsNotNull(vm.LoadCommand);
        Assert.IsNotNull(vm.RefreshCommand);
        Assert.IsNotNull(vm.CreateAnnouncementCommand);
        Assert.IsNotNull(vm.DeleteAnnouncementCommand);
    }
}

// ══════════════════════════════════════════════════════════════════════════════
// ClassRepAnalyticsViewModelTests
// ══════════════════════════════════════════════════════════════════════════════

[TestClass]
public class ClassRepAnalyticsViewModelTests
{
    private static ClassRepAnalyticsModel SampleAnalytics() => new()
    {
        TotalStudents        = 50,
        TotalSubclasses      = 3,
        AssignedLecturers    = 4,
        AverageAttendanceRate = 82.5,
        AttendanceTrend      = Enumerable.Range(1, 8).Select(i => new TrendPointModel { Label = $"W{i}", Value = 75 + i }).ToList(),
        StudentGrowthTrend   = Enumerable.Range(1, 8).Select(i => new TrendPointModel { Label = $"W{i}", Value = 50 }).ToList(),
    };

    private static ClassRepAnalyticsViewModel Build(
        Mock<IClassAnnouncementApiService>? mock = null)
    {
        if (mock is null)
        {
            mock = new Mock<IClassAnnouncementApiService>();
            mock.Setup(s => s.GetAnalyticsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(SampleAnalytics());
        }
        return new ClassRepAnalyticsViewModel(mock.Object);
    }

    // ── Initial state ─────────────────────────────────────────────────────────

    [TestMethod]
    public void InitialState_IsCorrect()
    {
        var vm = Build();

        Assert.IsFalse(vm.IsBusy);
        Assert.IsNull(vm.Analytics);
        Assert.IsNull(vm.AttendanceTrendChart);
        Assert.IsNull(vm.StudentGrowthChart);
        Assert.IsFalse(vm.HasError);
        Assert.AreEqual("Class Analytics", vm.Title);
    }

    // ── LoadAsync ─────────────────────────────────────────────────────────────

    [TestMethod]
    public async Task LoadAsync_PopulatesAnalytics()
    {
        var vm = Build();
        await vm.LoadAsync();

        Assert.IsNotNull(vm.Analytics);
        Assert.AreEqual(50, vm.Analytics!.TotalStudents);
        Assert.AreEqual(3,  vm.Analytics.TotalSubclasses);
        Assert.IsFalse(vm.HasError);
        Assert.IsFalse(vm.IsBusy);
    }

    [TestMethod]
    public async Task LoadAsync_BuildsCharts()
    {
        var vm = Build();
        await vm.LoadAsync();

        Assert.IsNotNull(vm.AttendanceTrendChart);
        Assert.IsNotNull(vm.StudentGrowthChart);
    }

    [TestMethod]
    public async Task LoadAsync_WhenApiReturnsNull_SetsError()
    {
        var m = new Mock<IClassAnnouncementApiService>();
        m.Setup(s => s.GetAnalyticsAsync(It.IsAny<CancellationToken>())).ReturnsAsync((ClassRepAnalyticsModel?)null);

        var vm = Build(m);
        await vm.LoadAsync();

        Assert.IsTrue(vm.HasError);
        Assert.IsFalse(vm.IsBusy);
    }

    [TestMethod]
    public async Task LoadAsync_ServiceThrows_SetsError()
    {
        var m = new Mock<IClassAnnouncementApiService>();
        m.Setup(s => s.GetAnalyticsAsync(It.IsAny<CancellationToken>()))
         .ThrowsAsync(new Exception("network error"));

        // Build directly — do NOT use the Build() helper (it re-registers GetAnalyticsAsync)
        var vm = new ClassRepAnalyticsViewModel(m.Object);
        await vm.LoadAsync();

        Assert.IsTrue(vm.HasError, $"Expected HasError=true, ErrorMessage='{vm.ErrorMessage}'");
        Assert.IsFalse(vm.IsBusy);
    }

    [TestMethod]
    public async Task LoadAsync_WhenBusy_DoesNotReenter()
    {
        int count = 0;
        var tcs = new TaskCompletionSource<ClassRepAnalyticsModel?>();
        var m = new Mock<IClassAnnouncementApiService>();
        m.Setup(s => s.GetAnalyticsAsync(It.IsAny<CancellationToken>()))
         .Returns(() => { count++; return tcs.Task; });

        var vm = new ClassRepAnalyticsViewModel(m.Object);
        var t1 = vm.LoadAsync();          // starts, increments count, blocks on tcs
        await Task.Yield();               // yield so t1 can run and set IsBusy = true
        var t2 = vm.LoadAsync();          // IsBusy guard: returns immediately
        tcs.SetResult(null);
        await Task.WhenAll(t1, t2);

        Assert.AreEqual(1, count);
    }

    // ── Computed labels on model ───────────────────────────────────────────────

    [TestMethod]
    public void AnalyticsModel_AttendanceRateLabel_IsFormatted()
    {
        var m = new ClassRepAnalyticsModel { AverageAttendanceRate = 82.5 };
        Assert.AreEqual("82.5%", m.AttendanceRateLabel);
    }

    [TestMethod]
    public void AnalyticsModel_RateColor_HighRate_IsGreen()
    {
        var m = new ClassRepAnalyticsModel { AverageAttendanceRate = 80 };
        Assert.AreEqual("#22C55E", m.RateColor);
    }

    [TestMethod]
    public void AnalyticsModel_RateColor_MidRate_IsAmber()
    {
        var m = new ClassRepAnalyticsModel { AverageAttendanceRate = 60 };
        Assert.AreEqual("#F59E0B", m.RateColor);
    }

    [TestMethod]
    public void AnalyticsModel_RateColor_LowRate_IsRed()
    {
        var m = new ClassRepAnalyticsModel { AverageAttendanceRate = 30 };
        Assert.AreEqual("#EF4444", m.RateColor);
    }

    // ── CancelPendingRequests ─────────────────────────────────────────────────

    [TestMethod]
    public void CancelPendingRequests_DoesNotThrow() => Build().CancelPendingRequests();

    // ── Commands ──────────────────────────────────────────────────────────────

    [TestMethod]
    public void Commands_AreNotNull()
    {
        var vm = Build();
        Assert.IsNotNull(vm.LoadCommand);
        Assert.IsNotNull(vm.RefreshCommand);
    }

    // ── TimeAgo helper on ClassAnnouncementModel ──────────────────────────────

    [TestMethod]
    public void ClassAnnouncementModel_TimeAgo_JustNow()
    {
        var m = new ClassAnnouncementModel { CreatedAt = DateTime.UtcNow };
        Assert.AreEqual("just now", m.TimeAgo);
    }

    [TestMethod]
    public void ClassAnnouncementModel_TimeAgo_MinutesAgo()
    {
        var m = new ClassAnnouncementModel { CreatedAt = DateTime.UtcNow.AddMinutes(-30) };
        Assert.AreEqual("30m ago", m.TimeAgo);
    }

    [TestMethod]
    public void ClassAnnouncementModel_TimeAgo_HoursAgo()
    {
        var m = new ClassAnnouncementModel { CreatedAt = DateTime.UtcNow.AddHours(-5) };
        Assert.AreEqual("5h ago", m.TimeAgo);
    }
}

// ── Extension helper (avoids separate method chain setup calls) ───────────────
internal static class MockExtensions
{
    public static Mock<T> Also<T>(this Mock<T> mock, Action<Mock<T>> setup)
        where T : class
    {
        setup(mock);
        return mock;
    }
}
