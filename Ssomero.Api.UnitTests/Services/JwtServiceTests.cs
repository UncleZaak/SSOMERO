using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero.Api.Configuration;
using Ssomero.Api.Services;

namespace Ssomero.Api.Services.UnitTests;


/// <summary>
/// Contains unit tests for the <see cref="JwtService"/> class.
/// </summary>
[TestClass]
public class JwtServiceTests
{
    /// <summary>
    /// Tests that the constructor properly initializes the service when provided with valid settings.
    /// Input: Valid IOptions with non-null JwtSettings.
    /// Expected: Service is created without throwing exceptions and settings are properly stored.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidSettings_InitializesSuccessfully()
    {
        // Arrange
        var jwtSettings = new JwtSettings
        {
            Secret = "test-secret-key-that-is-long-enough",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpiryMinutes = 30,
            RefreshTokenExpiryDays = 14
        };
        var mockOptions = new Mock<IOptions<JwtSettings>>();
        mockOptions.Setup(o => o.Value).Returns(jwtSettings);

        // Act
        var service = new JwtService(mockOptions.Object);

        // Assert
        Assert.IsNotNull(service);
        mockOptions.Verify(o => o.Value, Times.Once);
    }

    /// <summary>
    /// Tests that the constructor handles the case when settings.Value returns null.
    /// Input: Valid IOptions instance but Value property returns null.
    /// Expected: Service is created but internal settings field will be null (potential issue).
    /// </summary>
    [TestMethod]
    public void Constructor_SettingsValueIsNull_DoesNotThrow()
    {
        // Arrange
        var mockOptions = new Mock<IOptions<JwtSettings>>();
        mockOptions.Setup(o => o.Value).Returns((JwtSettings)null!);

        // Act
        var service = new JwtService(mockOptions.Object);

        // Assert
        Assert.IsNotNull(service);
        mockOptions.Verify(o => o.Value, Times.Once);
    }

    /// <summary>
    /// Tests that the constructor works with minimal JwtSettings configuration.
    /// Input: JwtSettings with default/empty values.
    /// Expected: Service is created successfully.
    /// </summary>
    [TestMethod]
    public void Constructor_MinimalSettings_InitializesSuccessfully()
    {
        // Arrange
        var jwtSettings = new JwtSettings();
        var mockOptions = new Mock<IOptions<JwtSettings>>();
        mockOptions.Setup(o => o.Value).Returns(jwtSettings);

        // Act
        var service = new JwtService(mockOptions.Object);

        // Assert
        Assert.IsNotNull(service);
    }

    /// <summary>
    /// Tests that the constructor works with extreme values in JwtSettings.
    /// Input: JwtSettings with maximum integer values for expiry times.
    /// Expected: Service is created successfully without overflow.
    /// </summary>
    [TestMethod]
    public void Constructor_ExtremeExpiryValues_InitializesSuccessfully()
    {
        // Arrange
        var jwtSettings = new JwtSettings
        {
            Secret = "test-secret",
            AccessTokenExpiryMinutes = int.MaxValue,
            RefreshTokenExpiryDays = int.MaxValue
        };
        var mockOptions = new Mock<IOptions<JwtSettings>>();
        mockOptions.Setup(o => o.Value).Returns(jwtSettings);

        // Act
        var service = new JwtService(mockOptions.Object);

        // Assert
        Assert.IsNotNull(service);
    }

    /// <summary>
    /// Tests that the constructor works with negative expiry values in JwtSettings.
    /// Input: JwtSettings with negative integer values for expiry times.
    /// Expected: Service is created successfully (validation not enforced in constructor).
    /// </summary>
    [TestMethod]
    public void Constructor_NegativeExpiryValues_InitializesSuccessfully()
    {
        // Arrange
        var jwtSettings = new JwtSettings
        {
            Secret = "test-secret",
            AccessTokenExpiryMinutes = -1,
            RefreshTokenExpiryDays = -100
        };
        var mockOptions = new Mock<IOptions<JwtSettings>>();
        mockOptions.Setup(o => o.Value).Returns(jwtSettings);

        // Act
        var service = new JwtService(mockOptions.Object);

        // Assert
        Assert.IsNotNull(service);
    }

    /// <summary>
    /// Tests that the constructor works with empty string values in JwtSettings.
    /// Input: JwtSettings with empty strings for Secret, Issuer, and Audience.
    /// Expected: Service is created successfully.
    /// </summary>
    [TestMethod]
    public void Constructor_EmptyStringSettings_InitializesSuccessfully()
    {
        // Arrange
        var jwtSettings = new JwtSettings
        {
            Secret = string.Empty,
            Issuer = string.Empty,
            Audience = string.Empty
        };
        var mockOptions = new Mock<IOptions<JwtSettings>>();
        mockOptions.Setup(o => o.Value).Returns(jwtSettings);

        // Act
        var service = new JwtService(mockOptions.Object);

        // Assert
        Assert.IsNotNull(service);
    }

    /// <summary>
    /// Tests that the constructor works with very long string values in JwtSettings.
    /// Input: JwtSettings with extremely long strings for Secret, Issuer, and Audience.
    /// Expected: Service is created successfully.
    /// </summary>
    [TestMethod]
    public void Constructor_VeryLongStringSettings_InitializesSuccessfully()
    {
        // Arrange
        var longString = new string('a', 10000);
        var jwtSettings = new JwtSettings
        {
            Secret = longString,
            Issuer = longString,
            Audience = longString
        };
        var mockOptions = new Mock<IOptions<JwtSettings>>();
        mockOptions.Setup(o => o.Value).Returns(jwtSettings);

        // Act
        var service = new JwtService(mockOptions.Object);

        // Assert
        Assert.IsNotNull(service);
    }

    /// <summary>
    /// Tests that the constructor works with special characters in string settings.
    /// Input: JwtSettings with special characters, unicode, and control characters.
    /// Expected: Service is created successfully.
    /// </summary>
    [TestMethod]
    public void Constructor_SpecialCharactersInSettings_InitializesSuccessfully()
    {
        // Arrange
        var jwtSettings = new JwtSettings
        {
            Secret = "!@#$%^&*()_+-=[]{}|;':\",./<>?\n\t\r",
            Issuer = "测试发行者🎉",
            Audience = "Test\u0000Audience"
        };
        var mockOptions = new Mock<IOptions<JwtSettings>>();
        mockOptions.Setup(o => o.Value).Returns(jwtSettings);

        // Act
        var service = new JwtService(mockOptions.Object);

        // Assert
        Assert.IsNotNull(service);
    }

    /// <summary>
    /// Tests that GenerateRefreshToken returns a valid Base64 string with the expected length of 88 characters.
    /// </summary>
    [TestMethod]
    public void GenerateRefreshToken_ReturnsValidBase64StringWithExpectedLength()
    {
        // Arrange
        var mockSettings = new Mock<IOptions<JwtSettings>>();
        mockSettings.Setup(s => s.Value).Returns(new JwtSettings());
        var jwtService = new global::Ssomero.Api.Services.JwtService(mockSettings.Object);

        // Act
        string refreshToken = jwtService.GenerateRefreshToken();

        // Assert
        Assert.IsNotNull(refreshToken);
        Assert.IsFalse(string.IsNullOrEmpty(refreshToken));
        Assert.AreEqual(88, refreshToken.Length, "Base64 encoded 64-byte array should be 88 characters long");

        // Verify it's valid Base64 by attempting to decode
        byte[] decodedBytes = Convert.FromBase64String(refreshToken);
        Assert.IsNotNull(decodedBytes);
    }

    /// <summary>
    /// Tests that GenerateRefreshToken returns decoded byte array of exactly 64 bytes.
    /// </summary>
    [TestMethod]
    public void GenerateRefreshToken_DecodedToken_Has64Bytes()
    {
        // Arrange
        var mockSettings = new Mock<IOptions<JwtSettings>>();
        mockSettings.Setup(s => s.Value).Returns(new JwtSettings());
        var jwtService = new global::Ssomero.Api.Services.JwtService(mockSettings.Object);

        // Act
        string refreshToken = jwtService.GenerateRefreshToken();
        byte[] decodedBytes = Convert.FromBase64String(refreshToken);

        // Assert
        Assert.AreEqual(64, decodedBytes.Length, "Decoded refresh token should contain exactly 64 bytes");
    }

    /// <summary>
    /// Tests that multiple calls to GenerateRefreshToken produce different tokens, verifying randomness.
    /// </summary>
    [TestMethod]
    public void GenerateRefreshToken_MultipleCalls_GeneratesDifferentTokens()
    {
        // Arrange
        var mockSettings = new Mock<IOptions<JwtSettings>>();
        mockSettings.Setup(s => s.Value).Returns(new JwtSettings());
        var jwtService = new global::Ssomero.Api.Services.JwtService(mockSettings.Object);

        // Act
        string token1 = jwtService.GenerateRefreshToken();
        string token2 = jwtService.GenerateRefreshToken();
        string token3 = jwtService.GenerateRefreshToken();

        // Assert
        Assert.AreNotEqual(token1, token2, "First and second tokens should be different");
        Assert.AreNotEqual(token1, token3, "First and third tokens should be different");
        Assert.AreNotEqual(token2, token3, "Second and third tokens should be different");
    }

    /// <summary>
    /// Tests that GenerateRefreshToken returns tokens with non-zero content when decoded.
    /// </summary>
    [TestMethod]
    public void GenerateRefreshToken_DecodedToken_ContainsNonZeroBytes()
    {
        // Arrange
        var mockSettings = new Mock<IOptions<JwtSettings>>();
        mockSettings.Setup(s => s.Value).Returns(new JwtSettings());
        var jwtService = new global::Ssomero.Api.Services.JwtService(mockSettings.Object);

        // Act
        string refreshToken = jwtService.GenerateRefreshToken();
        byte[] decodedBytes = Convert.FromBase64String(refreshToken);

        // Assert
        bool hasNonZeroByte = false;
        foreach (byte b in decodedBytes)
        {
            if (b != 0)
            {
                hasNonZeroByte = true;
                break;
            }
        }
        Assert.IsTrue(hasNonZeroByte, "Decoded token should contain at least one non-zero byte (extremely high probability with random generation)");
    }

    /// <summary>
    /// Tests that GenerateAccessToken successfully creates a valid JWT token with standard valid inputs.
    /// </summary>
    [TestMethod]
    public void GenerateAccessToken_ValidInputs_ReturnsValidJwtToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var role = "Admin";
        var settings = new JwtSettings
        {
            Secret = "ThisIsAVerySecureSecretKeyForTestingPurposesOnly1234567890",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpiryMinutes = 30
        };
        var mockOptions = new Mock<IOptions<JwtSettings>>();
        mockOptions.Setup(o => o.Value).Returns(settings);
        var service = new JwtService(mockOptions.Object);

        // Act
        var token = service.GenerateAccessToken(userId, email, role);

        // Assert
        Assert.IsNotNull(token);
        Assert.IsFalse(string.IsNullOrWhiteSpace(token));

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        Assert.AreEqual(settings.Issuer, jwtToken.Issuer);
        Assert.AreEqual(settings.Audience, jwtToken.Audiences.First());

        var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
        Assert.IsNotNull(subClaim);
        Assert.AreEqual(userId.ToString(), subClaim.Value);

        var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email);
        Assert.IsNotNull(emailClaim);
        Assert.AreEqual(email, emailClaim.Value);

        var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
        Assert.IsNotNull(roleClaim);
        Assert.AreEqual(role, roleClaim.Value);

        var jtiClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti);
        Assert.IsNotNull(jtiClaim);
        Assert.IsFalse(string.IsNullOrWhiteSpace(jtiClaim.Value));
    }

    /// <summary>
    /// Tests that GenerateAccessToken sets the correct expiry time based on AccessTokenExpiryMinutes setting.
    /// </summary>
    [TestMethod]
    [DataRow(15)]
    [DataRow(30)]
    [DataRow(60)]
    [DataRow(120)]
    public void GenerateAccessToken_VariousExpiryMinutes_SetsCorrectExpiry(int expiryMinutes)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var role = "User";
        var settings = new JwtSettings
        {
            Secret = "ThisIsAVerySecureSecretKeyForTestingPurposesOnly1234567890",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpiryMinutes = expiryMinutes
        };
        var mockOptions = new Mock<IOptions<JwtSettings>>();
        mockOptions.Setup(o => o.Value).Returns(settings);
        var service = new JwtService(mockOptions.Object);
        var beforeGeneration = DateTime.UtcNow;

        // Act
        var token = service.GenerateAccessToken(userId, email, role);
        var afterGeneration = DateTime.UtcNow;

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // JWT tokens store expiry as Unix timestamps in seconds, so we need to account for precision loss
        var expectedMinExpiry = beforeGeneration.AddMinutes(expiryMinutes).AddSeconds(-1);
        var expectedMaxExpiry = afterGeneration.AddMinutes(expiryMinutes).AddSeconds(1);

        Assert.IsTrue(jwtToken.ValidTo >= expectedMinExpiry);
        Assert.IsTrue(jwtToken.ValidTo <= expectedMaxExpiry);
    }

    /// <summary>
    /// Tests that GenerateAccessToken works with Guid.Empty as userId.
    /// </summary>
    [TestMethod]
    public void GenerateAccessToken_EmptyGuidUserId_ReturnsValidToken()
    {
        // Arrange
        var userId = Guid.Empty;
        var email = "test@example.com";
        var role = "User";
        var settings = new JwtSettings
        {
            Secret = "ThisIsAVerySecureSecretKeyForTestingPurposesOnly1234567890",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpiryMinutes = 30
        };
        var mockOptions = new Mock<IOptions<JwtSettings>>();
        mockOptions.Setup(o => o.Value).Returns(settings);
        var service = new JwtService(mockOptions.Object);

        // Act
        var token = service.GenerateAccessToken(userId, email, role);

        // Assert
        Assert.IsNotNull(token);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);
        Assert.IsNotNull(subClaim);
        Assert.AreEqual(Guid.Empty.ToString(), subClaim.Value);
    }

    /// <summary>
    /// Tests that GenerateAccessToken handles various email formats including empty and whitespace.
    /// </summary>
    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("   ")]
    [DataRow("test@example.com")]
    [DataRow("very.long.email.address.with.many.parts@subdomain.example.com")]
    [DataRow("user+tag@example.com")]
    [DataRow("user@localhost")]
    public void GenerateAccessToken_VariousEmailFormats_ReturnsValidToken(string email)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var role = "User";
        var settings = new JwtSettings
        {
            Secret = "ThisIsAVerySecureSecretKeyForTestingPurposesOnly1234567890",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpiryMinutes = 30
        };
        var mockOptions = new Mock<IOptions<JwtSettings>>();
        mockOptions.Setup(o => o.Value).Returns(settings);
        var service = new JwtService(mockOptions.Object);

        // Act
        var token = service.GenerateAccessToken(userId, email, role);

        // Assert
        Assert.IsNotNull(token);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email);
        Assert.IsNotNull(emailClaim);
        Assert.AreEqual(email, emailClaim.Value);
    }

    /// <summary>
    /// Tests that GenerateAccessToken handles various role values including empty and whitespace.
    /// </summary>
    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow("Admin")]
    [DataRow("User")]
    [DataRow("SuperAdministrator")]
    [DataRow("Role-With-Special-Characters!@#$%")]
    public void GenerateAccessToken_VariousRoleValues_ReturnsValidToken(string role)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var settings = new JwtSettings
        {
            Secret = "ThisIsAVerySecureSecretKeyForTestingPurposesOnly1234567890",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpiryMinutes = 30
        };
        var mockOptions = new Mock<IOptions<JwtSettings>>();
        mockOptions.Setup(o => o.Value).Returns(settings);
        var service = new JwtService(mockOptions.Object);

        // Act
        var token = service.GenerateAccessToken(userId, email, role);

        // Assert
        Assert.IsNotNull(token);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
        Assert.IsNotNull(roleClaim);
        Assert.AreEqual(role, roleClaim.Value);
    }

    /// <summary>
    /// Tests that GenerateAccessToken generates unique Jti (JWT ID) claims for each token.
    /// </summary>
    [TestMethod]
    public void GenerateAccessToken_MultipleCalls_GeneratesUniqueJtiClaims()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var role = "User";
        var settings = new JwtSettings
        {
            Secret = "ThisIsAVerySecureSecretKeyForTestingPurposesOnly1234567890",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpiryMinutes = 30
        };
        var mockOptions = new Mock<IOptions<JwtSettings>>();
        mockOptions.Setup(o => o.Value).Returns(settings);
        var service = new JwtService(mockOptions.Object);

        // Act
        var token1 = service.GenerateAccessToken(userId, email, role);
        var token2 = service.GenerateAccessToken(userId, email, role);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken1 = handler.ReadJwtToken(token1);
        var jwtToken2 = handler.ReadJwtToken(token2);

        var jti1 = jwtToken1.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
        var jti2 = jwtToken2.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;

        Assert.IsNotNull(jti1);
        Assert.IsNotNull(jti2);
        Assert.AreNotEqual(jti1, jti2);
    }

    /// <summary>
    /// Tests that GenerateAccessToken handles very long string inputs for email and role.
    /// </summary>
    [TestMethod]
    public void GenerateAccessToken_VeryLongStrings_ReturnsValidToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = new string('a', 500) + "@example.com";
        var role = new string('b', 500);
        var settings = new JwtSettings
        {
            Secret = "ThisIsAVerySecureSecretKeyForTestingPurposesOnly1234567890",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpiryMinutes = 30
        };
        var mockOptions = new Mock<IOptions<JwtSettings>>();
        mockOptions.Setup(o => o.Value).Returns(settings);
        var service = new JwtService(mockOptions.Object);

        // Act
        var token = service.GenerateAccessToken(userId, email, role);

        // Assert
        Assert.IsNotNull(token);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email);
        var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
        Assert.IsNotNull(emailClaim);
        Assert.IsNotNull(roleClaim);
        Assert.AreEqual(email, emailClaim.Value);
        Assert.AreEqual(role, roleClaim.Value);
    }

    /// <summary>
    /// Tests that GenerateAccessToken handles special characters in email and role.
    /// </summary>
    [TestMethod]
    public void GenerateAccessToken_SpecialCharactersInInputs_ReturnsValidToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test+tag@example.com";
        var role = "Admin-Role!@#$%^&*()";
        var settings = new JwtSettings
        {
            Secret = "ThisIsAVerySecureSecretKeyForTestingPurposesOnly1234567890",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpiryMinutes = 30
        };
        var mockOptions = new Mock<IOptions<JwtSettings>>();
        mockOptions.Setup(o => o.Value).Returns(settings);
        var service = new JwtService(mockOptions.Object);

        // Act
        var token = service.GenerateAccessToken(userId, email, role);

        // Assert
        Assert.IsNotNull(token);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email);
        var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
        Assert.AreEqual(email, emailClaim.Value);
        Assert.AreEqual(role, roleClaim.Value);
    }

    /// <summary>
    /// Tests that GenerateAccessToken handles edge case expiry minutes values.
    /// </summary>
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(int.MaxValue)]
    public void GenerateAccessToken_EdgeCaseExpiryMinutes_ReturnsValidToken(int expiryMinutes)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var role = "User";
        var settings = new JwtSettings
        {
            Secret = "ThisIsAVerySecureSecretKeyForTestingPurposesOnly1234567890",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpiryMinutes = expiryMinutes
        };
        var mockOptions = new Mock<IOptions<JwtSettings>>();
        mockOptions.Setup(o => o.Value).Returns(settings);
        var service = new JwtService(mockOptions.Object);

        // Act
        var token = service.GenerateAccessToken(userId, email, role);

        // Assert
        Assert.IsNotNull(token);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        Assert.IsNotNull(jwtToken);
    }

    /// <summary>
    /// Tests that GenerateAccessToken handles empty issuer and audience settings.
    /// </summary>
    [TestMethod]
    public void GenerateAccessToken_EmptyIssuerAndAudience_ReturnsValidToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var role = "User";
        var settings = new JwtSettings
        {
            Secret = "ThisIsAVerySecureSecretKeyForTestingPurposesOnly1234567890",
            Issuer = "",
            Audience = "",
            AccessTokenExpiryMinutes = 30
        };
        var mockOptions = new Mock<IOptions<JwtSettings>>();
        mockOptions.Setup(o => o.Value).Returns(settings);
        var service = new JwtService(mockOptions.Object);

        // Act
        var token = service.GenerateAccessToken(userId, email, role);

        // Assert
        Assert.IsNotNull(token);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        Assert.IsNotNull(jwtToken);
    }

    /// <summary>
    /// Tests that GenerateAccessToken handles Unicode characters in email and role.
    /// </summary>
    [TestMethod]
    public void GenerateAccessToken_UnicodeCharacters_ReturnsValidToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "用户@example.com";
        var role = "管理员";
        var settings = new JwtSettings
        {
            Secret = "ThisIsAVerySecureSecretKeyForTestingPurposesOnly1234567890",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpiryMinutes = 30
        };
        var mockOptions = new Mock<IOptions<JwtSettings>>();
        mockOptions.Setup(o => o.Value).Returns(settings);
        var service = new JwtService(mockOptions.Object);

        // Act
        var token = service.GenerateAccessToken(userId, email, role);

        // Assert
        Assert.IsNotNull(token);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email);
        var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
        Assert.AreEqual(email, emailClaim.Value);
        Assert.AreEqual(role, roleClaim.Value);
    }

    /// <summary>
    /// Tests that GenerateAccessToken handles negative expiry minutes value.
    /// </summary>
    [TestMethod]
    public void GenerateAccessToken_NegativeExpiryMinutes_ReturnsTokenWithPastExpiry()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "test@example.com";
        var role = "User";
        var settings = new JwtSettings
        {
            Secret = "ThisIsAVerySecureSecretKeyForTestingPurposesOnly1234567890",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpiryMinutes = -30
        };
        var mockOptions = new Mock<IOptions<JwtSettings>>();
        mockOptions.Setup(o => o.Value).Returns(settings);
        var service = new JwtService(mockOptions.Object);

        // Act
        var token = service.GenerateAccessToken(userId, email, role);

        // Assert
        Assert.IsNotNull(token);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        Assert.IsTrue(jwtToken.ValidTo < DateTime.UtcNow);
    }

    /// <summary>
    /// Tests that GetAccessTokenExpiry returns the correct expiry time
    /// when AccessTokenExpiryMinutes is set to various positive, zero, and negative values.
    /// Expected result: The returned DateTime should be approximately equal to UtcNow plus the configured minutes.
    /// </summary>
    /// <param name="accessTokenExpiryMinutes">The number of minutes to add to the current time.</param>
    [TestMethod]
    [DataRow(15)]
    [DataRow(60)]
    [DataRow(1440)]
    [DataRow(0)]
    [DataRow(-15)]
    [DataRow(-60)]
    public void GetAccessTokenExpiry_WithVariousMinuteValues_ReturnsCorrectExpiryTime(int accessTokenExpiryMinutes)
    {
        // Arrange
        var mockSettings = new Mock<IOptions<JwtSettings>>();
        mockSettings.Setup(s => s.Value).Returns(new JwtSettings
        {
            AccessTokenExpiryMinutes = accessTokenExpiryMinutes
        });
        var jwtService = new JwtService(mockSettings.Object);
        var beforeCall = DateTime.UtcNow;

        // Act
        var result = jwtService.GetAccessTokenExpiry();

        // Assert
        var afterCall = DateTime.UtcNow;
        var expectedMin = beforeCall.AddMinutes(accessTokenExpiryMinutes);
        var expectedMax = afterCall.AddMinutes(accessTokenExpiryMinutes);
        Assert.IsTrue(result >= expectedMin && result <= expectedMax,
            $"Expected expiry time between {expectedMin} and {expectedMax}, but got {result}");
    }

    /// <summary>
    /// Tests that GetAccessTokenExpiry handles extreme boundary value int.MaxValue
    /// without throwing an exception.
    /// Expected result: The method returns a DateTime far in the future without error.
    /// </summary>
    [TestMethod]
    public void GetAccessTokenExpiry_WithMaxIntValue_ReturnsExtremelyFarFutureDate()
    {
        // Arrange
        var mockSettings = new Mock<IOptions<JwtSettings>>();
        mockSettings.Setup(s => s.Value).Returns(new JwtSettings
        {
            AccessTokenExpiryMinutes = int.MaxValue
        });
        var jwtService = new JwtService(mockSettings.Object);
        var beforeCall = DateTime.UtcNow;

        // Act
        var result = jwtService.GetAccessTokenExpiry();

        // Assert
        var afterCall = DateTime.UtcNow;
        var expectedMin = beforeCall.AddMinutes(int.MaxValue);
        var expectedMax = afterCall.AddMinutes(int.MaxValue);
        Assert.IsTrue(result >= expectedMin && result <= expectedMax,
            $"Expected expiry time between {expectedMin} and {expectedMax}, but got {result}");
        Assert.IsTrue(result > DateTime.UtcNow, "Result should be far in the future");
    }

    /// <summary>
    /// Tests that GetAccessTokenExpiry returns a time in UTC (not local time).
    /// Expected result: The returned DateTime's Kind should be Utc.
    /// </summary>
    [TestMethod]
    public void GetAccessTokenExpiry_ReturnsUtcDateTime()
    {
        // Arrange
        var mockSettings = new Mock<IOptions<JwtSettings>>();
        mockSettings.Setup(s => s.Value).Returns(new JwtSettings
        {
            AccessTokenExpiryMinutes = 60
        });
        var jwtService = new JwtService(mockSettings.Object);

        // Act
        var result = jwtService.GetAccessTokenExpiry();

        // Assert
        Assert.AreEqual(DateTimeKind.Utc, result.Kind, "The returned DateTime should be in UTC");
    }

    /// <summary>
    /// Tests that GetAccessTokenExpiry correctly uses the AccessTokenExpiryMinutes setting.
    /// Expected result: The setting property is accessed during the method execution.
    /// </summary>
    [TestMethod]
    public void GetAccessTokenExpiry_AccessesAccessTokenExpiryMinutesSetting()
    {
        // Arrange
        var jwtSettings = new JwtSettings { AccessTokenExpiryMinutes = 30 };
        var mockSettings = new Mock<IOptions<JwtSettings>>();
        mockSettings.Setup(s => s.Value).Returns(jwtSettings);
        var jwtService = new JwtService(mockSettings.Object);

        // Act
        var result = jwtService.GetAccessTokenExpiry();

        // Assert
        mockSettings.Verify(s => s.Value, Times.AtLeastOnce);
        var expected = DateTime.UtcNow.AddMinutes(30);
        Assert.IsTrue(Math.Abs((result - expected).TotalSeconds) < 1,
            "The result should be approximately 30 minutes from now");
    }
}