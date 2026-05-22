using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero.Api.Data;
using Ssomero.Api.Entities;
using Ssomero.Api.Services;

namespace Ssomero.Api.Services.UnitTests;


/// <summary>
/// Unit tests for the ClassService class.
/// </summary>
[TestClass]
public partial class ClassServiceTests
{
    /// <summary>
    /// Tests the constructor behavior when the db parameter is null.
    /// Verifies that the constructor does not throw an exception and creates an instance.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullDb_CreatesInstance()
    {
        // Arrange
        SsomeroDbContext? nullDb = null;
        var mockLogger = new Mock<ILogger<ClassService>>();

        // Act
        var service = new ClassService(nullDb!, mockLogger.Object);

        // Assert
        Assert.IsNotNull(service);
    }

    /// <summary>
    /// Tests the constructor behavior when both parameters are null.
    /// Verifies that the constructor does not throw an exception and creates an instance.
    /// </summary>
    [TestMethod]
    public void Constructor_WithBothParametersNull_CreatesInstance()
    {
        // Arrange
        SsomeroDbContext? nullDb = null;
        ILogger<ClassService>? nullLogger = null;

        // Act
        var service = new ClassService(nullDb!, nullLogger!);

        // Assert
        Assert.IsNotNull(service);
    }
}