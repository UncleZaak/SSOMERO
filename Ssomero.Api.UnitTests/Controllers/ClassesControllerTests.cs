using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero.Api.Controllers;
using Ssomero.Api.Data;
using Ssomero.Api.Dtos;
using Ssomero.Api.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Ssomero.Api.Controllers.UnitTests;
/// <summary>
/// Unit tests for the <see cref = "ClassesController"/> class.
/// </summary>
[TestClass]
public class ClassesControllerTests
{
    /// <summary>
    /// Tests that the constructor accepts null DbContext parameter.
    /// Note: The parameter is marked as non-nullable but no explicit validation is performed in the constructor.
    /// Input: Null DbContext.
    /// Expected: Instance is created without throwing an exception (current behavior).
    /// </summary>
    [TestMethod]
    public void Constructor_NullDbContext_CreatesInstanceWithoutThrowingException()
    {
        // Arrange
        SsomeroDbContext? nullDbContext = null;
        // Act
        var controller = new ClassesController(nullDbContext!);
        // Assert
        Assert.IsNotNull(controller);
    }

    /// <summary>
    /// Tests that GetCourses returns Forbid when user has an unauthorized role.
    /// Validates that GetCourses properly delegates to GetMyClasses and returns Forbid for Admin role.
    /// </summary>
    [TestMethod]
    public async Task GetCourses_AdminRole_ReturnsForbid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var mockDbContext = new Mock<SsomeroDbContext>(new DbContextOptions<SsomeroDbContext>());
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var controller = new ClassesController(mockDbContext.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            }
        };
        // Act
        var result = await controller.GetCourses();
        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType<ForbidResult>(result);
    }

    private static SsomeroDbContext CreateInMemoryContext()
    {
        DbContextOptions<SsomeroDbContext> options = new DbContextOptionsBuilder<SsomeroDbContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
        return new SsomeroDbContext(options);
    }

    private static ClassesController CreateController(SsomeroDbContext context, Guid userId, string? role)
    {
        ClassesController controller = new ClassesController(context);
        List<Claim> claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };
        if (role != null)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuthType");
        ClaimsPrincipal principal = new ClaimsPrincipal(identity);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };
        return controller;
    }

    private static Class CreateClass(string name = "Test Class")
    {
        return new Class
        {
            Id = Guid.NewGuid(),
            Name = name,
            ProgramId = Guid.NewGuid(),
            SemesterId = Guid.NewGuid(),
            AcademicYearId = Guid.NewGuid(),
            YearOfStudy = 1
        };
    }
}