using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero.Api.Entities;
using Ssomero.Api.Repositories.Interfaces;
using Ssomero.Api.Security;
using Ssomero.Api.Services.Implementations;

namespace Ssomero.Api.UnitTests.Services;

[TestClass]
public class InvitationServiceTests
{
    private Mock<IInvitationRepository> _repoMock = null!;
    private Mock<IKeyProvider> _keyProviderMock = null!;
    private Mock<ILogger<InvitationService>> _loggerMock = null!;
    private InvitationService _service = null!;

    [TestInitialize]
    public void Init()
    {
        _repoMock = new Mock<IInvitationRepository>();
        _keyProviderMock = new Mock<IKeyProvider>();
        _loggerMock = new Mock<ILogger<InvitationService>>();
        _service = new InvitationService(_repoMock.Object, _loggerMock.Object, _keyProviderMock.Object);
    }

    private static string ComputeHashBase64(byte[] key, string token)
    {
        using var hmac = new HMACSHA256(key);
        var computed = hmac.ComputeHash(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(computed);
    }

    [TestMethod]
    public async Task ValidateInvitation_ShouldSucceed_WhenTokenValid()
    {
        var keyId = "k1";
        var key = RandomNumberGenerator.GetBytes(32);
        var token = "valid-token-123";
        var tokenHash = ComputeHashBase64(key, token);

        var inv = new Invitation
        {
            Id = Guid.NewGuid(),
            TokenHash = tokenHash,
            TokenKeyId = keyId,
            Status = "Created",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            ConsumedAt = null
        };

        _keyProviderMock.Setup(k => k.GetAllKeys()).Returns(new Dictionary<string, byte[]> { { keyId, key } });
        _repoMock.Setup(r => r.GetByTokenHashAsync(tokenHash, default)).ReturnsAsync(inv);
        _repoMock.Setup(r => r.AddAuditAsync(It.IsAny<InvitationAudit>(), default)).Returns(Task.CompletedTask);

        var res = await _service.ValidateInvitationAsync(token);

        Assert.IsTrue(res.IsValid);
        Assert.AreEqual(inv.Id, res.InvitationId);
        _repoMock.Verify(r => r.AddAuditAsync(It.Is<InvitationAudit>(a => a.InvitationId == inv.Id && a.Details == "Valid"), default), Times.Once);
    }

    [TestMethod]
    public async Task ValidateInvitation_ShouldFail_WhenTokenExpired()
    {
        var keyId = "k2";
        var key = RandomNumberGenerator.GetBytes(32);
        var token = "expired-token";
        var tokenHash = ComputeHashBase64(key, token);

        var inv = new Invitation
        {
            Id = Guid.NewGuid(),
            TokenHash = tokenHash,
            TokenKeyId = keyId,
            Status = "Created",
            ExpiresAt = DateTime.UtcNow.AddHours(-1),
            ConsumedAt = null
        };

        _keyProviderMock.Setup(k => k.GetAllKeys()).Returns(new Dictionary<string, byte[]> { { keyId, key } });
        _repoMock.Setup(r => r.GetByTokenHashAsync(tokenHash, default)).ReturnsAsync(inv);
        _repoMock.Setup(r => r.AddAuditAsync(It.IsAny<InvitationAudit>(), default)).Returns(Task.CompletedTask);

        var res = await _service.ValidateInvitationAsync(token);

        Assert.IsFalse(res.IsValid);
        Assert.AreEqual("Expired", res.Reason);
        _repoMock.Verify(r => r.AddAuditAsync(It.Is<InvitationAudit>(a => a.Details == "Expired"), default), Times.Once);
    }

    [TestMethod]
    public async Task ValidateInvitation_ShouldFail_WhenTokenRevoked()
    {
        var keyId = "k3";
        var key = RandomNumberGenerator.GetBytes(32);
        var token = "revoked-token";
        var tokenHash = ComputeHashBase64(key, token);

        var inv = new Invitation
        {
            Id = Guid.NewGuid(),
            TokenHash = tokenHash,
            TokenKeyId = keyId,
            Status = "Revoked",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            ConsumedAt = null
        };

        _keyProviderMock.Setup(k => k.GetAllKeys()).Returns(new Dictionary<string, byte[]> { { keyId, key } });
        _repoMock.Setup(r => r.GetByTokenHashAsync(tokenHash, default)).ReturnsAsync(inv);
        _repoMock.Setup(r => r.AddAuditAsync(It.IsAny<InvitationAudit>(), default)).Returns(Task.CompletedTask);

        var res = await _service.ValidateInvitationAsync(token);

        Assert.IsFalse(res.IsValid);
        Assert.AreEqual("Revoked", res.Reason);
        _repoMock.Verify(r => r.AddAuditAsync(It.Is<InvitationAudit>(a => a.Details == "Revoked"), default), Times.Once);
    }

    [TestMethod]
    public async Task ValidateInvitation_ShouldFail_WhenTokenConsumed()
    {
        var keyId = "k4";
        var key = RandomNumberGenerator.GetBytes(32);
        var token = "consumed-token";
        var tokenHash = ComputeHashBase64(key, token);

        var inv = new Invitation
        {
            Id = Guid.NewGuid(),
            TokenHash = tokenHash,
            TokenKeyId = keyId,
            Status = "Created",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            ConsumedAt = DateTime.UtcNow.AddMinutes(-1)
        };

        _keyProviderMock.Setup(k => k.GetAllKeys()).Returns(new Dictionary<string, byte[]> { { keyId, key } });
        _repoMock.Setup(r => r.GetByTokenHashAsync(tokenHash, default)).ReturnsAsync(inv);
        _repoMock.Setup(r => r.AddAuditAsync(It.IsAny<InvitationAudit>(), default)).Returns(Task.CompletedTask);

        var res = await _service.ValidateInvitationAsync(token);

        Assert.IsFalse(res.IsValid);
        Assert.AreEqual("AlreadyConsumed", res.Reason);
        _repoMock.Verify(r => r.AddAuditAsync(It.Is<InvitationAudit>(a => a.Details == "AlreadyConsumed"), default), Times.Once);
    }

    [TestMethod]
    public async Task ValidateInvitation_ShouldFail_WhenTokenUnknown()
    {
        var keyId = "k5";
        var key = RandomNumberGenerator.GetBytes(32);
        var token = "unknown-token";
        var tokenHash = ComputeHashBase64(key, token);

        _keyProviderMock.Setup(k => k.GetAllKeys()).Returns(new Dictionary<string, byte[]> { { keyId, key } });
        _repoMock.Setup(r => r.GetByTokenHashAsync(It.IsAny<string>(), default)).ReturnsAsync((Invitation?)null);
        _repoMock.Setup(r => r.AddAuditAsync(It.IsAny<InvitationAudit>(), default)).Returns(Task.CompletedTask);

        var res = await _service.ValidateInvitationAsync(token);

        Assert.IsFalse(res.IsValid);
        Assert.AreEqual("InvalidToken", res.Reason);
        _repoMock.Verify(r => r.AddAuditAsync(It.Is<InvitationAudit>(a => a.InvitationId == Guid.Empty && a.Details == "TokenNotFound"), default), Times.Once);
    }

    [TestMethod]
    public async Task ValidateInvitation_ShouldSupportKeyRotation()
    {
        // Provide two keys: first is old (doesn't match), second is current (matches)
        var oldKeyId = "old";
        var newKeyId = "new";
        var oldKey = RandomNumberGenerator.GetBytes(32);
        var newKey = RandomNumberGenerator.GetBytes(32);
        var token = "rotated-token";
        var correctHash = ComputeHashBase64(newKey, token);

        var inv = new Invitation
        {
            Id = Guid.NewGuid(),
            TokenHash = correctHash,
            TokenKeyId = newKeyId,
            Status = "Created",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            ConsumedAt = null
        };

        // Return both keys; algorithm should try oldKey then newKey and find a match
        _keyProviderMock.Setup(k => k.GetAllKeys()).Returns(new Dictionary<string, byte[]> { { oldKeyId, oldKey }, { newKeyId, newKey } });
        _repoMock.Setup(r => r.GetByTokenHashAsync(correctHash, default)).ReturnsAsync(inv);
        _repoMock.Setup(r => r.AddAuditAsync(It.IsAny<InvitationAudit>(), default)).Returns(Task.CompletedTask);

        var res = await _service.ValidateInvitationAsync(token);

        Assert.IsTrue(res.IsValid);
        Assert.AreEqual(inv.Id, res.InvitationId);
        _repoMock.Verify(r => r.AddAuditAsync(It.Is<InvitationAudit>(a => a.Details == "Valid"), default), Times.Once);
    }
}
