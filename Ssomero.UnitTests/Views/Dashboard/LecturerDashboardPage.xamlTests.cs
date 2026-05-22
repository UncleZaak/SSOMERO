using System.Collections.Generic;

using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero.Interfaces;
using Ssomero.Services;
using Ssomero.ViewModels;
using Ssomero.Views.Dashboard;

namespace Ssomero.Views.Dashboard.UnitTests;

/// <summary>
/// Page-level tests for <see cref="LecturerDashboardPage"/>.
/// Focused on wiring (BindingContext) and lifecycle side-effects (OnDisappearing).
/// OnAppearing animation and async-void behavior are covered by
/// <see cref="Ssomero.ViewModels.UnitTests.LecturerDashboardViewModelTests"/>.
/// </summary>
[TestClass]
public class LecturerDashboardPageTests
{
    // ── helpers ──────────────────────────────────────────────────────────────

    private static LecturerDashboardViewModel CreateRealViewModel() =>
        new(new Mock<ILecturerApiService>().Object, new SessionService());

    private static Mock<LecturerDashboardViewModel> CreateMockViewModel() =>
        new(MockBehavior.Loose, new Mock<ILecturerApiService>().Object, new SessionService());

    // ── constructor ──────────────────────────────────────────────────────────

    /// <summary>
    /// BindingContext must be the exact VM instance passed to the constructor.
    /// </summary>
    [TestMethod]
    public void Constructor_SetsBindingContextToProvidedViewModel()
    {
        var vm = CreateRealViewModel();

        var page = new TestableLecturerDashboardPage(vm);

        Assert.AreSame(vm, page.BindingContext);
    }

    // ── OnDisappearing ───────────────────────────────────────────────────────

    /// <summary>
    /// OnDisappearing must call CancelPendingRequests exactly once to abort in-flight requests.
    /// </summary>
    [TestMethod]
    public void OnDisappearing_CallsCancelPendingRequestsOnce()
    {
        var mockVm = CreateMockViewModel();
        var page = new TestableLecturerDashboardPage(mockVm.Object);

        page.TestOnDisappearing();

        mockVm.Verify(v => v.CancelPendingRequests(), Times.Once);
    }

    /// <summary>
    /// Each navigation away must independently cancel pending requests —
    /// the method must be safe to call consecutively.
    /// </summary>
    [TestMethod]
    [DataRow(1, DisplayName = "Single disappear")]
    [DataRow(3, DisplayName = "Three consecutive disappears")]
    public void OnDisappearing_CalledNTimes_CancelPendingRequestsCalledNTimes(int callCount)
    {
        var mockVm = CreateMockViewModel();
        var page = new TestableLecturerDashboardPage(mockVm.Object);

        for (var i = 0; i < callCount; i++)
            page.TestOnDisappearing();

        mockVm.Verify(v => v.CancelPendingRequests(), Times.Exactly(callCount));
    }

    /// <summary>
    /// OnDisappearing must not throw when CancelPendingRequests completes normally.
    /// </summary>
    [TestMethod]
    public void OnDisappearing_DoesNotThrow_WhenCancelSucceeds()
    {
        var mockVm = CreateMockViewModel();
        var page = new TestableLecturerDashboardPage(mockVm.Object);

        // Act — must not throw
        page.TestOnDisappearing();
    }

    // ── test double ──────────────────────────────────────────────────────────

    /// <summary>
    /// Exposes <c>protected override void OnDisappearing()</c> without triggering
    /// MAUI animation/UI-thread code.
    /// </summary>
    private sealed class TestableLecturerDashboardPage : LecturerDashboardPage
    {
        public TestableLecturerDashboardPage(LecturerDashboardViewModel vm) : base(vm) { }

        public void TestOnDisappearing() => OnDisappearing();
    }
}