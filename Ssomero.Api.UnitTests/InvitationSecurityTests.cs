using System;
using System.Threading.Tasks;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ssomero.Api.Security;
using System.Collections.Generic;

namespace Ssomero.Api.UnitTests;

[TestClass]
public class InvitationSecurityTests
{
    [TestMethod]
    public void ConfigurationKeyProvider_LoadsKeys()
    {
        var mainSection = new Mock<IConfigurationSection>();
        var prevSection = new Mock<IConfigurationSection>();
        prevSection.Setup(p => p.GetChildren()).Returns(Array.Empty<IConfigurationSection>());
        mainSection.Setup(s => s["CurrentKeyId"]).Returns("k1");
        mainSection.Setup(s => s["CurrentKey"]).Returns(Convert.ToBase64String(Guid.NewGuid().ToByteArray()));
        mainSection.Setup(s => s.GetSection("PreviousKeys")).Returns(prevSection.Object);

        var config = new Mock<IConfiguration>();
        config.Setup(c => c.GetSection("InvitationSecurity")).Returns(mainSection.Object);

        var p = new ConfigurationKeyProvider(config.Object);
        var id = p.GetCurrentKeyId();
        Assert.IsFalse(string.IsNullOrWhiteSpace(id));
        var key = p.GetCurrentKey();
        Assert.IsNotNull(key);
        Assert.IsTrue(key.Length > 0);
    }
}
