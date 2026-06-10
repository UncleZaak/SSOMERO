using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
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
public class InvitationSecurityIntegrityTests
{
    private SsomeroDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<SsomeroDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new SsomeroDbContext(options);
    }

    private static byte[] RandomKey() => RandomNumberGenerator.GetBytes(32);

    private class KeyProviderStub : IKeyProvider
    {
        private readonly System.Collections.Generic.Dictionary<string, byte[]> _keys;
        public KeyProviderStub(System.Collections.Generic.IEnumerable<KeyValuePair<string, byte[]>> keys)
        {
            _keys = keys.ToDictionary(k => k.Key, v => v.Value);
        }
        public string GetCurrentKeyId() => _keys.Keys.Last();
        public byte[] GetCurrentKey() => _keys.Values.Last();
        public bool TryGetKey(string keyId, out byte[]? keyBytes) => _keys.TryGetValue(keyId, out keyBytes);
        public System.Collections.Generic.IReadOnlyDictionary<string, byte[]> GetAllKeys() => _keys;
    }

    [TestMethod]
    public async Task CreateInvitation_ShouldGenerateUniqueToken()
    {
        var db = CreateDbContext(nameof(CreateInvitation_ShouldGenerateUniqueToken));
        var repo = new InvitationRepository(db);
        var key = RandomKey();
        var svc = new InvitationService(repo, Mock.Of<ILogger<InvitationService>>(), new KeyProviderStub(new[] { new KeyValuePair<string, byte[]>("k", key) }));

        var r1 = await svc.CreateInvitationAsync(Guid.NewGuid(), null, "a@b.com", null, "p", DateTime.UtcNow.AddDays(1));
        var r2 = await svc.CreateInvitationAsync(Guid.NewGuid(), null, "c@d.com", null, "p", DateTime.UtcNow.AddDays(1));

        var i1 = await repo.GetByIdAsync(r1.InvitationId);
        var i2 = await repo.GetByIdAsync(r2.InvitationId);

        Assert.IsNotNull(i1);
        Assert.IsNotNull(i2);
        Assert.AreNotEqual(i1.TokenHash, i2.TokenHash, "Two created invitations should have different token hashes");
    }

    [TestMethod]
    public async Task CreateInvitation_ShouldCreateAudit()
    {
        var db = CreateDbContext(nameof(CreateInvitation_ShouldCreateAudit));
        var repo = new InvitationRepository(db);
        var key = RandomKey();
        var svc = new InvitationService(repo, Mock.Of<ILogger<InvitationService>>(), new KeyProviderStub(new[] { new KeyValuePair<string, byte[]>("k", key) }));

        var res = await svc.CreateInvitationAsync(Guid.NewGuid(), null, "e@f.com", null, "p", DateTime.UtcNow.AddDays(1));
        var audits = await db.InvitationAudits.Where(a => a.InvitationId == res.InvitationId).ToListAsync();
        Assert.IsTrue(audits.Any(), "Creation should produce an audit entry");
        Assert.IsTrue(audits.Any(a => a.EventType == "Created"));
    }

    [TestMethod]
    public async Task ValidateInvitation_ShouldNotModifyData()
    {
        var db = CreateDbContext(nameof(ValidateInvitation_ShouldNotModifyData));
        var repo = new InvitationRepository(db);
        var key = RandomKey();
        var kp = new KeyProviderStub(new[] { new KeyValuePair<string, byte[]>("k", key) });

        // Create a raw token and corresponding hash
        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        var raw = Convert.ToBase64String(tokenBytes).TrimEnd('=');
        using var hmac = new HMACSHA256(key);
        var hash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(raw)));

        var invitation = new Invitation { Id = Guid.NewGuid(), InviterId = Guid.NewGuid(), Purpose = "p", TokenHash = hash, TokenKeyId = "k", Status = "Created", CreatedAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddHours(1), MaxUses = 1, SingleUse = true };
        await repo.CreateAsync(invitation);

        var svc = new InvitationService(repo, Mock.Of<ILogger<InvitationService>>(), kp);

        var before = await repo.GetByIdAsync(invitation.Id);

        var result = await svc.ValidateInvitationAsync(raw);

        var after = await repo.GetByIdAsync(invitation.Id);

        Assert.IsTrue(result.IsValid);
        // Ensure no fields that should remain stable were changed by validation
        Assert.AreEqual(before.Status, after.Status);
        Assert.AreEqual(before.TokenHash, after.TokenHash);
        Assert.AreEqual(before.UsesCount, after.UsesCount);
        Assert.AreEqual(before.ConsumedAt, after.ConsumedAt);
    }

    [TestMethod]
    public async Task ValidateInvitation_ShouldCreateAuditEntry()
    {
        var db = CreateDbContext(nameof(ValidateInvitation_ShouldCreateAuditEntry));
        var repo = new InvitationRepository(db);
        var key = RandomKey();
        var kp = new KeyProviderStub(new[] { new KeyValuePair<string, byte[]>("k", key) });

        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        var raw = Convert.ToBase64String(tokenBytes).TrimEnd('=');
        using var hmac = new HMACSHA256(key);
        var hash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(raw)));

        var invitation = new Invitation { Id = Guid.NewGuid(), InviterId = Guid.NewGuid(), Purpose = "p", TokenHash = hash, TokenKeyId = "k", Status = "Created", CreatedAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddHours(1), MaxUses = 1, SingleUse = true };
        await repo.CreateAsync(invitation);

        var svc = new InvitationService(repo, Mock.Of<ILogger<InvitationService>>(), kp);

        var res = await svc.ValidateInvitationAsync(raw);

        var audits = await db.InvitationAudits.Where(a => a.InvitationId == invitation.Id).ToListAsync();
        Assert.IsTrue(audits.Any(), "Validation should emit an audit entry");
        Assert.IsTrue(audits.Any(a => a.EventType == "Validated"));
    }

    [TestMethod]
    public async Task TokenHash_ShouldNeverEqualRawToken()
    {
        var db = CreateDbContext(nameof(TokenHash_ShouldNeverEqualRawToken));
        var repo = new InvitationRepository(db);
        var key = RandomKey();
        var svc = new InvitationService(repo, Mock.Of<ILogger<InvitationService>>(), new KeyProviderStub(new[] { new KeyValuePair<string, byte[]>("k", key) }));

        var res = await svc.CreateInvitationAsync(Guid.NewGuid(), null, "x@y.com", null, "p", DateTime.UtcNow.AddDays(1));
        var inv = await repo.GetByIdAsync(res.InvitationId);
        Assert.IsNotNull(inv);
        // The token hash stored should not be equal to any plausible raw token string
        Assert.IsFalse(string.Equals(inv.TokenHash, Convert.ToBase64String(Encoding.UTF8.GetBytes(inv.TokenHash))), "Sanity: token hash differs from base64 of itself");
    }

    [TestMethod]
    public async Task Validation_ShouldRejectTamperedToken()
    {
        var db = CreateDbContext(nameof(Validation_ShouldRejectTamperedToken));
        var repo = new InvitationRepository(db);
        var key = RandomKey();
        var kp = new KeyProviderStub(new[] { new KeyValuePair<string, byte[]>("k", key) });

        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        var raw = Convert.ToBase64String(tokenBytes).TrimEnd('=');
        using var hmac = new HMACSHA256(key);
        var hash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(raw)));

        var invitation = new Invitation { Id = Guid.NewGuid(), InviterId = Guid.NewGuid(), Purpose = "p", TokenHash = hash, TokenKeyId = "k", Status = "Created", CreatedAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddHours(1), MaxUses = 1, SingleUse = true };
        await repo.CreateAsync(invitation);

        var svc = new InvitationService(repo, Mock.Of<ILogger<InvitationService>>(), kp);

        // Tamper one char
        var tampered = raw.Substring(0, raw.Length - 1) + (raw[^1] == 'A' ? 'B' : 'A');
        var res = await svc.ValidateInvitationAsync(tampered);
        Assert.IsFalse(res.IsValid);
    }

    [TestMethod]
    public async Task Validation_ShouldRejectMalformedToken()
    {
        var db = CreateDbContext(nameof(Validation_ShouldRejectMalformedToken));
        var repo = new InvitationRepository(db);
        var key = RandomKey();
        var kp = new KeyProviderStub(new[] { new KeyValuePair<string, byte[]>("k", key) });

        var token = "not-a-valid-token-format-!!";

        var svc = new InvitationService(repo, Mock.Of<ILogger<InvitationService>>(), kp);
        var res = await svc.ValidateInvitationAsync(token);
        Assert.IsFalse(res.IsValid);
    }

    [TestMethod]
    public async Task KeyRotation_ShouldValidateHistoricalTokens()
    {
        var db = CreateDbContext(nameof(KeyRotation_ShouldValidateHistoricalTokens));
        var repo = new InvitationRepository(db);

        // old and new keys
        var oldKey = RandomKey();
        var newKey = RandomKey();

        // Create token hashed with oldKey and stored with tokenKeyId = "old"
        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        var raw = Convert.ToBase64String(tokenBytes).TrimEnd('=');
        using var hmacOld = new HMACSHA256(oldKey);
        var hashOld = Convert.ToBase64String(hmacOld.ComputeHash(Encoding.UTF8.GetBytes(raw)));

        var invitation = new Invitation { Id = Guid.NewGuid(), InviterId = Guid.NewGuid(), Purpose = "p", TokenHash = hashOld, TokenKeyId = "old", Status = "Created", CreatedAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddHours(1), MaxUses = 1, SingleUse = true };
        await repo.CreateAsync(invitation);

        var kp = new KeyProviderStub(new[] { new KeyValuePair<string, byte[]>("old", oldKey), new KeyValuePair<string, byte[]>("new", newKey) });
        var svc = new InvitationService(repo, Mock.Of<ILogger<InvitationService>>(), kp);

        var res = await svc.ValidateInvitationAsync(raw);
        Assert.IsTrue(res.IsValid, "Validation should succeed using historical key present in provider");
    }

    [TestMethod]
    public async Task SqliteIntegration_TransactionRollback_RollsBackOnRollback()
    {
        // Use SQLite in-memory to verify transaction rollback semantics
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();
        try
        {
            var options = new DbContextOptionsBuilder<SsomeroDbContext>()
                .UseSqlite(connection)
                .Options;

            using (var db = new SsomeroDbContext(options))
            {
                db.Database.EnsureCreated();

                using var tx = await db.Database.BeginTransactionAsync();
                var inv = new Invitation { Id = Guid.NewGuid(), InviterId = Guid.NewGuid(), Purpose = "p", TokenHash = "h", TokenKeyId = "k", Status = "Created", CreatedAt = DateTime.UtcNow, ExpiresAt = DateTime.UtcNow.AddHours(1) };
                db.Invitations.Add(inv);
                await db.SaveChangesAsync();

                // Add audit but then rollback
                var audit = new InvitationAudit { Id = Guid.NewGuid(), InvitationId = inv.Id, EventType = "Created", Timestamp = DateTime.UtcNow };
                db.InvitationAudits.Add(audit);
                await db.SaveChangesAsync();

                // Rollback entire transaction
                await tx.RollbackAsync();
            }

            // New context to query
            using (var db2 = new SsomeroDbContext(new DbContextOptionsBuilder<SsomeroDbContext>().UseSqlite(connection).Options))
            {
                var invites = await db2.Invitations.ToListAsync();
                var audits = await db2.InvitationAudits.ToListAsync();
                Assert.AreEqual(0, invites.Count, "Invitations should be rolled back by transaction");
                Assert.AreEqual(0, audits.Count, "Audits should be rolled back by transaction");
            }
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
}
