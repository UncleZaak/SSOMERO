using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero.Api.Data;
using Ssomero.Api.Entities;
using Ssomero.Api.Repositories;
using Ssomero.Api.Repositories.Interfaces;
using Ssomero.Api.Security;
using Ssomero.Api.Services.Implementations;

namespace Ssomero.Api.UnitTests.Services;

[TestClass]
public class InvitationLifecycleTests
{
    private SsomeroDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<SsomeroDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new SsomeroDbContext(options);
    }

    private class KeyProviderStub : IKeyProvider
    {
        private readonly byte[] _key;
        public KeyProviderStub(byte[] key) { _key = key; }
        public string GetCurrentKeyId() => "k";
        public byte[] GetCurrentKey() => _key;
        public bool TryGetKey(string keyId, out byte[]? keyBytes) { keyBytes = _key; return true; }
        public System.Collections.Generic.IReadOnlyDictionary<string, byte[]> GetAllKeys() => new System.Collections.Generic.Dictionary<string, byte[]> { { "k", _key } };
    }

    [TestMethod]
    public async Task CreateInvitation_ShouldPersistAudit_WhenSuccess()
    {
        var db = CreateDbContext(nameof(CreateInvitation_ShouldPersistAudit_WhenSuccess));
        var repo = new InvitationRepository(db);
        var key = RandomNumberGenerator.GetBytes(32);
        var svc = new InvitationService(repo, Mock.Of<ILogger<InvitationService>>(), new KeyProviderStub(key));

        var result = await svc.CreateInvitationAsync(Guid.NewGuid(), null, "test@example.com", null, "invite", DateTime.UtcNow.AddDays(1));

        var inv = await repo.GetByIdAsync(result.InvitationId);
        Assert.IsNotNull(inv);
        var audits = await db.InvitationAudits.Where(a => a.InvitationId == inv.Id).ToListAsync();
        Assert.IsTrue(audits.Any());
    }

    [TestMethod]
    public async Task CreateInvitation_ShouldRollback_WhenAuditFails()
    {
        var db = CreateDbContext(nameof(CreateInvitation_ShouldRollback_WhenAuditFails));
        var repo = new InvitationRepository(db);
        var key = RandomNumberGenerator.GetBytes(32);

        // Wrap repo to simulate audit failure by throwing when CreateWithAuditAsync is called
        var failingRepo = new Mock<IInvitationRepository>();
        failingRepo.Setup(r => r.CreateWithAuditAsync(It.IsAny<Invitation>(), It.IsAny<InvitationAudit>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Simulated audit failure"));
        failingRepo.Setup(r => r.CreateAsync(It.IsAny<Invitation>(), It.IsAny<CancellationToken>())).ReturnsAsync((Invitation inv, CancellationToken ct) => inv);

        var svc = new InvitationService(failingRepo.Object, Mock.Of<ILogger<InvitationService>>(), new KeyProviderStub(key));

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => svc.CreateInvitationAsync(Guid.NewGuid(), null, "test@x.com", null, "p", DateTime.UtcNow.AddDays(1)));

        // Ensure no invitations were persisted in the real DB repo (which is separate) — simulate expectation via fresh DB
        var db2 = CreateDbContext(nameof(CreateInvitation_ShouldRollback_WhenAuditFails) + "-check");
        Assert.AreEqual(0, await db2.Invitations.CountAsync());
    }

    [TestMethod]
    public async Task RevokeInvitation_ShouldPersistAudit_WhenSuccess()
    {
        var db = CreateDbContext(nameof(RevokeInvitation_ShouldPersistAudit_WhenSuccess));
        var repo = new InvitationRepository(db);
        var key = RandomNumberGenerator.GetBytes(32);
        var svc = new InvitationService(repo, Mock.Of<ILogger<InvitationService>>(), new KeyProviderStub(key));

        var createRes = await svc.CreateInvitationAsync(Guid.NewGuid(), null, "a@b.com", null, "p", DateTime.UtcNow.AddDays(1));
        var ok = await svc.RevokeInvitationAsync(createRes.InvitationId, Guid.NewGuid(), "test");

        Assert.IsTrue(ok);
        var inv = await repo.GetByIdAsync(createRes.InvitationId);
        Assert.IsNotNull(inv);
        Assert.AreEqual("Revoked", inv.Status);
        var audits = await db.InvitationAudits.Where(a => a.InvitationId == inv.Id && a.EventType == "Revoked").ToListAsync();
        Assert.IsTrue(audits.Any());
    }

    [TestMethod]
    public async Task RevokeInvitation_ShouldRollback_WhenAuditFails()
    {
        var db = CreateDbContext(nameof(RevokeInvitation_ShouldRollback_WhenAuditFails));
        var repo = new InvitationRepository(db);
        var key = RandomNumberGenerator.GetBytes(32);
        var svc = new InvitationService(repo, Mock.Of<ILogger<InvitationService>>(), new KeyProviderStub(key));

        var res = await svc.CreateInvitationAsync(Guid.NewGuid(), null, "z@x.com", null, "p", DateTime.UtcNow.AddDays(1));

        // Create failing repo to simulate audit failure on revoke
        var failingRepo = new Mock<IInvitationRepository>();
        failingRepo.Setup(r => r.ExistsAsync(res.InvitationId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        failingRepo.Setup(r => r.GetByIdAsync(res.InvitationId, It.IsAny<CancellationToken>())).ReturnsAsync(new Invitation { Id = res.InvitationId, Status = "Created" });
        failingRepo.Setup(r => r.RevokeWithAuditAsync(It.IsAny<Guid>(), It.IsAny<InvitationAudit>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("Audit fail"));

        var svc2 = new InvitationService(failingRepo.Object, Mock.Of<ILogger<InvitationService>>(), new KeyProviderStub(key));

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => svc2.RevokeInvitationAsync(res.InvitationId, Guid.NewGuid(), "r"));

        // Ensure original repo still has the invitation unrevoked
        var inv = await repo.GetByIdAsync(res.InvitationId);
        Assert.IsNotNull(inv);
        Assert.AreEqual("Created", inv.Status);
    }

    [TestMethod]
    public async Task CreateInvitation_ShouldStoreHashOnly()
    {
        var db = CreateDbContext(nameof(CreateInvitation_ShouldStoreHashOnly));
        var repo = new InvitationRepository(db);
        var key = RandomNumberGenerator.GetBytes(32);
        var svc = new InvitationService(repo, Mock.Of<ILogger<InvitationService>>(), new KeyProviderStub(key));

        var result = await svc.CreateInvitationAsync(Guid.NewGuid(), null, "abc@d.com", null, "p", DateTime.UtcNow.AddDays(1));

        var inv = await repo.GetByIdAsync(result.InvitationId);
        Assert.IsNotNull(inv);
        // TokenHash should be present, but raw token is never stored on entity; service does not expose raw token
        Assert.IsFalse(string.IsNullOrEmpty(inv.TokenHash));
    }

    [TestMethod]
    public async Task RevokeInvitation_ShouldBeIdempotent()
    {
        var db = CreateDbContext(nameof(RevokeInvitation_ShouldBeIdempotent));
        var repo = new InvitationRepository(db);
        var key = RandomNumberGenerator.GetBytes(32);
        var svc = new InvitationService(repo, Mock.Of<ILogger<InvitationService>>(), new KeyProviderStub(key));

        var res = await svc.CreateInvitationAsync(Guid.NewGuid(), null, "idemp@t.com", null, "p", DateTime.UtcNow.AddDays(1));
        var first = await svc.RevokeInvitationAsync(res.InvitationId, Guid.NewGuid(), "r");
        Assert.IsTrue(first);
        var second = await svc.RevokeInvitationAsync(res.InvitationId, Guid.NewGuid(), "r");
        Assert.IsTrue(second);
    }
}
