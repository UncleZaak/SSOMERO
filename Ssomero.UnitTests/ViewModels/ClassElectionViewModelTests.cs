using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero.Interfaces;
using Ssomero.Models;
using Ssomero.Services;
using Ssomero.ViewModels;

namespace Ssomero.ViewModels.UnitTests;

[TestClass]
public class ClassElectionViewModelTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    private static ClassElectionModel ActiveElection(int seconds = 45) => new()
    {
        Id               = Guid.NewGuid(),
        ClassId          = Guid.NewGuid(),
        ClassName        = "CS301",
        Status           = "Active",
        SecondsRemaining = seconds,
        CanVote          = true,
        HasVoted         = false,
        Candidates       =
        [
            new() { StudentId = Guid.NewGuid(), StudentName = "Alice Smith",  StudentNumber = "2021/001", VoteCount = 3 },
            new() { StudentId = Guid.NewGuid(), StudentName = "Bob Johnson",  StudentNumber = "2021/002", VoteCount = 1 },
        ]
    };

    private static ClassElectionModel CompletedElection() => new()
    {
        Id               = Guid.NewGuid(),
        ClassId          = Guid.NewGuid(),
        ClassName        = "CS301",
        Status           = "Completed",
        SecondsRemaining = 0,
        CanVote          = false,
        HasVoted         = true,
        WinnerName       = "Alice Smith",
        WinnerStudentId  = Guid.NewGuid(),
        Candidates       =
        [
            new() { StudentId = Guid.NewGuid(), StudentName = "Alice Smith", StudentNumber = "2021/001", VoteCount = 3 },
            new() { StudentId = Guid.NewGuid(), StudentName = "Bob Johnson", StudentNumber = "2021/002", VoteCount = 1 },
        ]
    };

    private static Mock<IClassElectionApiService> MockNoElection()
    {
        var m = new Mock<IClassElectionApiService>();
        m.Setup(s => s.GetActiveElectionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
         .ReturnsAsync((ClassElectionModel?)null);
        m.Setup(s => s.GetMyClassesAsync(It.IsAny<CancellationToken>()))
         .ReturnsAsync([]);
        return m;
    }

    private static Mock<IClassElectionApiService> MockActiveElection(ClassElectionModel? e = null)
    {
        var election = e ?? ActiveElection();
        var m = new Mock<IClassElectionApiService>();
        m.Setup(s => s.GetActiveElectionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
         .ReturnsAsync(election);
        m.Setup(s => s.GetMyClassesAsync(It.IsAny<CancellationToken>()))
         .ReturnsAsync([]);
        return m;
    }

    private static ClassElectionViewModel BuildVm(Mock<IClassElectionApiService> mock, Guid? classId = null)
    {
        var vm = new ClassElectionViewModel(mock.Object); // uses minimal constructor
        vm.ClassId   = classId ?? Guid.NewGuid();
        vm.ClassName = "CS301";
        return vm;
    }

    private static ClassElectionViewModel BuildVmFull(
        Mock<IClassElectionApiService> mock,
        Mock<INotificationService>? notifMock = null,
        Guid? classId = null)
    {
        var vm = new ClassElectionViewModel(
            mock.Object,
            notifMock?.Object,
            null); // SessionService is null in tests
        vm.ClassId   = classId ?? Guid.NewGuid();
        vm.ClassName = "CS301";
        return vm;
    }

    // ── Initial state ─────────────────────────────────────────────────────────

    [TestMethod]
    public void InitialState_IsBusyFalse_NoElection()
    {
        var vm = BuildVm(MockNoElection());

        Assert.IsFalse(vm.IsBusy);
        Assert.IsNull(vm.CurrentElection);
        Assert.IsFalse(vm.HasElection);
        Assert.IsTrue(vm.CanStartElection);
        Assert.IsFalse(vm.IsVotingOpen);
        Assert.IsFalse(vm.IsCompleted);
        Assert.IsFalse(vm.HasError);
        Assert.AreEqual(0, vm.Candidates.Count);
    }

    [TestMethod]
    public void InitialState_TitleIsClassRepElections()
    {
        var vm = BuildVm(MockNoElection());
        Assert.AreEqual("Class Rep Elections", vm.Title);
    }

    // ── LoadAsync — no election ───────────────────────────────────────────────

    [TestMethod]
    public async Task LoadAsync_NoElection_HasElectionFalse_CanStartTrue()
    {
        var vm = BuildVm(MockNoElection());
        await vm.LoadAsync();

        Assert.IsFalse(vm.HasElection);
        Assert.IsNull(vm.CurrentElection);
        Assert.IsTrue(vm.CanStartElection);
        Assert.IsFalse(vm.IsBusy);
        Assert.IsFalse(vm.HasError);
    }

    [TestMethod]
    public async Task LoadAsync_EmptyClassId_TriesClassResolution()
    {
        var mock = new Mock<IClassElectionApiService>();
        mock.Setup(s => s.GetMyClassesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        var vm = new ClassElectionViewModel(mock.Object); // ClassId stays Guid.Empty
        await vm.LoadAsync();

        mock.Verify(s => s.GetMyClassesAsync(It.IsAny<CancellationToken>()), Times.Once);
        mock.Verify(s => s.GetActiveElectionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── LoadAsync — active election ───────────────────────────────────────────

    [TestMethod]
    public async Task LoadAsync_ActiveElection_PopulatesCandidates()
    {
        var election = ActiveElection();
        var vm       = BuildVm(MockActiveElection(election));
        await vm.LoadAsync();

        Assert.IsTrue(vm.HasElection);
        Assert.IsNotNull(vm.CurrentElection);
        Assert.IsTrue(vm.CurrentElection!.IsActive);
        Assert.AreEqual(2, vm.Candidates.Count);
        Assert.IsFalse(vm.IsBusy);
        Assert.IsFalse(vm.HasError);
    }

    [TestMethod]
    public async Task LoadAsync_ActiveElection_SecondsRemainingSet()
    {
        var election = ActiveElection(seconds: 42);
        var vm       = BuildVm(MockActiveElection(election));
        await vm.LoadAsync();

        Assert.AreEqual(42, vm.SecondsRemaining);
        Assert.AreEqual("42s remaining", vm.CountdownLabel);
    }

    // ── LoadAsync — busy guard ────────────────────────────────────────────────

    [TestMethod]
    public async Task LoadAsync_WhenBusy_DoesNotCallApiAgain()
    {
        var mock = MockNoElection();
        var vm   = BuildVm(mock);
        vm.IsBusy = true;

        await vm.LoadAsync();

        mock.Verify(s => s.GetActiveElectionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── StartElectionAsync ────────────────────────────────────────────────────

    [TestMethod]
    public async Task StartElectionAsync_Success_SetsElection()
    {
        var started  = ActiveElection();
        var mock     = MockNoElection();
        mock.Setup(s => s.StartElectionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(started);

        var vm = BuildVm(mock);
        await vm.StartElectionAsync();

        Assert.IsNotNull(vm.CurrentElection);
        Assert.IsTrue(vm.HasElection);
        Assert.IsFalse(vm.IsBusy);
        Assert.IsFalse(vm.HasError);
    }

    [TestMethod]
    public async Task StartElectionAsync_ApiReturnsNull_SetsError()
    {
        var mock = MockNoElection();
        mock.Setup(s => s.StartElectionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ClassElectionModel?)null);

        var vm = BuildVm(mock);
        await vm.StartElectionAsync();

        Assert.IsNull(vm.CurrentElection);
        Assert.IsTrue(vm.HasError);
        Assert.IsFalse(string.IsNullOrEmpty(vm.ErrorMessage));
    }

    // ── VoteAsync ─────────────────────────────────────────────────────────────

    [TestMethod]
    public async Task VoteAsync_Success_UpdatesElection()
    {
        var election  = ActiveElection();
        var afterVote = ActiveElection();
        afterVote.CanVote  = false;
        afterVote.HasVoted = true;
        afterVote.Candidates[0].VoteCount = 4;

        var mock = MockActiveElection(election);
        mock.Setup(s => s.VoteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(afterVote);

        var vm = BuildVm(mock);
        await vm.LoadAsync();

        var candidate = vm.Candidates[0];
        await vm.VoteAsync(candidate);

        Assert.IsFalse(vm.CurrentElection!.CanVote);
        Assert.IsTrue(vm.CurrentElection.HasVoted);
        Assert.IsFalse(vm.IsBusy);
    }

    [TestMethod]
    public async Task VoteAsync_ApiReturnsNull_SetsError()
    {
        var election = ActiveElection();
        var mock     = MockActiveElection(election);
        mock.Setup(s => s.VoteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ClassElectionModel?)null);

        var vm = BuildVm(mock);
        await vm.LoadAsync();

        await vm.VoteAsync(vm.Candidates[0]);

        Assert.IsTrue(vm.HasError);
    }

    [TestMethod]
    public async Task VoteAsync_NoCurrentElection_DoesNothing()
    {
        var mock = MockNoElection();
        var vm   = BuildVm(mock);

        // No LoadAsync, so CurrentElection is null
        await vm.VoteAsync(new ElectionCandidateModel { StudentId = Guid.NewGuid(), StudentName = "Test" });

        mock.Verify(s => s.VoteAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── Completed election ────────────────────────────────────────────────────

    [TestMethod]
    public async Task LoadAsync_CompletedElection_IsCompletedTrue_WinnerSet()
    {
        var completed = CompletedElection();
        var mock      = new Mock<IClassElectionApiService>();
        mock.Setup(s => s.GetActiveElectionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(completed);

        var vm = BuildVm(mock);
        await vm.LoadAsync();

        Assert.IsTrue(vm.IsCompleted);
        Assert.IsFalse(vm.IsVotingOpen);
        Assert.AreEqual("🏆 Alice Smith", vm.CurrentElection!.WinnerDisplay);
    }

    // ── Error handling ────────────────────────────────────────────────────────

    [TestMethod]
    public async Task LoadAsync_ApiThrows_SetsErrorMessage()
    {
        var mock = new Mock<IClassElectionApiService>();
        mock.Setup(s => s.GetActiveElectionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Network error"));

        var vm = BuildVm(mock);
        await vm.LoadAsync();

        Assert.IsTrue(vm.HasError);
        Assert.IsTrue(vm.ErrorMessage.Contains("Network error"));
        Assert.IsFalse(vm.IsBusy);
    }

    [TestMethod]
    public async Task StartElectionAsync_ApiThrows_SetsErrorMessage()
    {
        var mock = MockNoElection();
        mock.Setup(s => s.StartElectionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Server unavailable"));

        var vm = BuildVm(mock);
        await vm.StartElectionAsync();

        Assert.IsTrue(vm.HasError);
        Assert.IsFalse(vm.IsBusy);
    }

    // ── Model computed properties ─────────────────────────────────────────────

    [TestMethod]
    public void ElectionCandidateModel_Initials_CorrectForFullName()
    {
        var c = new ElectionCandidateModel { StudentName = "Alice Smith" };
        Assert.AreEqual("AS", c.Initials);
    }

    [TestMethod]
    public void ElectionCandidateModel_VoteCountLabel_Singular()
    {
        var c = new ElectionCandidateModel { VoteCount = 1 };
        Assert.AreEqual("1 vote", c.VoteCountLabel);
    }

    [TestMethod]
    public void ElectionCandidateModel_VoteCountLabel_Plural()
    {
        var c = new ElectionCandidateModel { VoteCount = 5 };
        Assert.AreEqual("5 votes", c.VoteCountLabel);
    }

    [TestMethod]
    public void ClassElectionModel_CountdownLabel_WhenZero()
    {
        var e = new ClassElectionModel { SecondsRemaining = 0 };
        Assert.AreEqual("Voting closed", e.CountdownLabel);
    }

    [TestMethod]
    public void ClassElectionModel_WinnerDisplay_NoWinner()
    {
        var e = new ClassElectionModel { WinnerName = null };
        Assert.AreEqual("No winner yet", e.WinnerDisplay);
    }

    [TestMethod]
    public void ClassElectionModel_IsActive_IsCompleted_StatusParsing()
    {
        var active    = new ClassElectionModel { Status = "Active" };
        var completed = new ClassElectionModel { Status = "Completed" };
        var other     = new ClassElectionModel { Status = "Pending" };

        Assert.IsTrue(active.IsActive);
        Assert.IsFalse(active.IsCompleted);
        Assert.IsTrue(completed.IsCompleted);
        Assert.IsFalse(completed.IsActive);
        Assert.IsFalse(other.IsActive);
        Assert.IsFalse(other.IsCompleted);
    }

    // ── Auto-resolve main class ───────────────────────────────────────────────

    [TestMethod]
    public async Task StartElectionAsync_EmptyClassId_AutoResolvesMainClass()
    {
        var mainClass = new ClassDto { Id = Guid.NewGuid().ToString(), Name = "CS301", ParentClassId = null };
        var started   = ActiveElection();

        var mock = new Mock<IClassElectionApiService>();
        mock.Setup(s => s.GetMyClassesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([mainClass]);
        mock.Setup(s => s.StartElectionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(started);

        var vm = new ClassElectionViewModel(mock.Object); // ClassId = Guid.Empty
        await vm.StartElectionAsync();

        Assert.AreNotEqual(Guid.Empty, vm.ClassId);
        Assert.IsNotNull(vm.CurrentElection);
    }

    [TestMethod]
    public async Task StartElectionAsync_EmptyClassId_NoClassFound_SetsError()
    {
        var mock = new Mock<IClassElectionApiService>();
        mock.Setup(s => s.GetMyClassesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var vm = new ClassElectionViewModel(mock.Object);
        await vm.StartElectionAsync();

        Assert.IsTrue(vm.HasError);
        mock.Verify(s => s.StartElectionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── Election-start notification ───────────────────────────────────────────

    [TestMethod]
    public async Task StartElectionAsync_Success_SendsStartNotification()
    {
        var started     = ActiveElection();
        var mock        = MockNoElection();
        var notifMock   = new Mock<INotificationService>();

        mock.Setup(s => s.StartElectionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(started);

        var vm = BuildVmFull(mock, notifMock);
        await vm.StartElectionAsync();

        notifMock.Verify(n => n.ScheduleNotificationAsync(
            It.IsInRange(210_001, 220_000, Moq.Range.Inclusive),
            "Class Rep Election Started",
            It.IsAny<string>(),
            It.IsAny<DateTime>()), Times.Once);
    }

    [TestMethod]
    public async Task StartElectionAsync_Success_StartNotificationSentOnlyOnce()
    {
        var started   = ActiveElection();
        var mock      = MockNoElection();
        var notifMock = new Mock<INotificationService>();

        mock.Setup(s => s.StartElectionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(started);

        var vm = BuildVmFull(mock, notifMock);
        await vm.StartElectionAsync();
        await vm.StartElectionAsync(); // second call — election already set, guard prevents double send

        notifMock.Verify(n => n.ScheduleNotificationAsync(
            It.IsInRange(210_001, 220_000, Moq.Range.Inclusive),
            "Class Rep Election Started",
            It.IsAny<string>(),
            It.IsAny<DateTime>()), Times.Once);
    }

    // ── Completion notification ───────────────────────────────────────────────

    [TestMethod]
    public async Task LoadAsync_CompletedElection_SendsCompletionNotification()
    {
        var completed = CompletedElection();
        var mock      = new Mock<IClassElectionApiService>();
        var notifMock = new Mock<INotificationService>();

        mock.Setup(s => s.GetActiveElectionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(completed);
        mock.Setup(s => s.GetMyClassesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var vm = BuildVmFull(mock, notifMock);
        await vm.LoadAsync();

        notifMock.Verify(n => n.ScheduleNotificationAsync(
            It.IsInRange(230_001, 240_000, Moq.Range.Inclusive),
            "Election Results",
            It.Is<string>(s => s.Contains("Alice Smith")),
            It.IsAny<DateTime>()), Times.Once);
    }

    [TestMethod]
    public async Task LoadAsync_CompletedElection_CompletionNotificationSentOnlyOnce()
    {
        var completed = CompletedElection();
        var mock      = new Mock<IClassElectionApiService>();
        var notifMock = new Mock<INotificationService>();

        mock.Setup(s => s.GetActiveElectionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(completed);
        mock.Setup(s => s.GetMyClassesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var vm = BuildVmFull(mock, notifMock);
        await vm.LoadAsync();
        await vm.LoadAsync(); // second load — same election, should NOT resend

        notifMock.Verify(n => n.ScheduleNotificationAsync(
            It.IsInRange(230_001, 240_000, Moq.Range.Inclusive),
            "Election Results",
            It.IsAny<string>(),
            It.IsAny<DateTime>()), Times.Once);
    }

    // ── Winner message ────────────────────────────────────────────────────────

    [TestMethod]
    public async Task LoadAsync_CompletedElection_NonWinner_IsCurrentUserWinnerFalse()
    {
        // SessionService is null → no current user → winner check skipped → IsCurrentUserWinner stays false
        var completed = CompletedElection();
        var mock      = new Mock<IClassElectionApiService>();
        var notifMock = new Mock<INotificationService>();

        mock.Setup(s => s.GetActiveElectionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(completed);
        mock.Setup(s => s.GetMyClassesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var vm = BuildVmFull(mock, notifMock);
        await vm.LoadAsync();

        Assert.IsFalse(vm.IsCurrentUserWinner);
    }

    // ── Notification failure resilience ──────────────────────────────────────

    [TestMethod]
    public async Task StartElection_NotificationThrows_DoesNotCrashViewModel()
    {
        var started   = ActiveElection();
        var mock      = MockNoElection();
        var notifMock = new Mock<INotificationService>();

        mock.Setup(s => s.StartElectionAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(started);
        notifMock.Setup(n => n.ScheduleNotificationAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>()))
            .ThrowsAsync(new Exception("Notification service unavailable"));

        var vm = BuildVmFull(mock, notifMock);
        await vm.StartElectionAsync(); // must not throw

        Assert.IsNotNull(vm.CurrentElection);
        Assert.IsFalse(vm.HasError);
    }

    // ── Constructor backward-compatibility ────────────────────────────────────

    [TestMethod]
    public void MinimalConstructor_IsBackwardCompatible()
    {
        var mock = new Mock<IClassElectionApiService>();
        var vm   = new ClassElectionViewModel(mock.Object);
        Assert.IsNotNull(vm);
        Assert.AreEqual("Class Rep Elections", vm.Title);
    }

    [TestMethod]
    public void FullConstructor_NullNotificationAndSession_DoesNotThrow()
    {
        var mock = new Mock<IClassElectionApiService>();
        var vm   = new ClassElectionViewModel(mock.Object, null, null);
        Assert.IsNotNull(vm);
    }
}
