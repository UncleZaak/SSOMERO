using System;
using System.Threading.Tasks;
using Ssomero.Api.Entities;
using Ssomero.Api.Repositories.Interfaces;
using Ssomero.Api.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ssomero.Api.UnitTests.Services;

[TestClass]
public class InvitationEmailQueueServiceTests
{
    private class FakeRepo : IInvitationDeliveryRepository
    {
        public InvitationDelivery? LastCreated;

        public Task AddAuditAsync(InvitationDeliveryAudit audit, System.Threading.CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }

        public Task<InvitationDelivery> CreateAsync(InvitationDelivery delivery, System.Threading.CancellationToken ct = default)
        {
            delivery.Id = Guid.NewGuid();
            LastCreated = delivery;
            return Task.FromResult(delivery);
        }

        public Task<InvitationDelivery?> GetByIdAsync(Guid id, System.Threading.CancellationToken ct = default)
        {
            return Task.FromResult(LastCreated?.Id == id ? LastCreated : null as InvitationDelivery);
        }

        public Task UpdateAsync(InvitationDelivery delivery, System.Threading.CancellationToken ct = default)
        {
            LastCreated = delivery;
            return Task.CompletedTask;
        }
    }

    [TestMethod]
    public async Task QueueInvitationEmail_ShouldCreateDeliveryRecord()
    {
        var repo = new FakeRepo();
        var fakeJobClient = new FakeBackgroundJobClient();
        var svc = new InvitationEmailQueueService(repo, fakeJobClient, NullLogger.Instance);

        var invitationId = Guid.NewGuid();
        var recipient = "test@example.com";
        var subject = "Test Subject";
        var body = "<p>Hello</p>";

        var id = await svc.QueueInvitationEmailAsync(invitationId, recipient, subject, body);

        Assert.IsNotNull(repo.LastCreated);
        Assert.AreEqual(recipient, repo.LastCreated.Recipient);
        Assert.AreEqual(subject, repo.LastCreated.Subject);
        Assert.AreEqual("Queued", repo.LastCreated.Status);
        Assert.AreEqual(id, repo.LastCreated.Id);
    }
}

// Minimal NullLogger shim to avoid adding package references
internal class NullLogger : Microsoft.Extensions.Logging.ILogger<InvitationEmailQueueService>
{
    public static readonly NullLogger Instance = new NullLogger();
    public IDisposable BeginScope<TState>(TState state) => null!;
    public bool IsEnabled(Microsoft.Extensions.Logging.LogLevel logLevel) => false;
    public void Log<TState>(Microsoft.Extensions.Logging.LogLevel logLevel, Microsoft.Extensions.Logging.EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
}

// Fake IBackgroundJobClient for unit tests - records enqueued jobs without using Hangfire JobStorage
internal class FakeBackgroundJobClient : Ssomero.Api.Services.IJobClient
{
    public bool EnqueueCalled { get; private set; }

    public string Enqueue<T>(System.Linq.Expressions.Expression<Func<T, Task>> methodCall)
    {
        EnqueueCalled = true;
        return Guid.NewGuid().ToString();
    }
}
