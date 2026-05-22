using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero.Interfaces;
using Ssomero.Models;
using Ssomero.Services;
using Ssomero.ViewModels;
using Ssomero.Views.Admin;

namespace Ssomero.Views.Admin.UnitTests;

/// <summary>
/// Unit tests for the <see cref = "UsersPage"/> class.
/// </summary>
[TestClass]
public partial class UsersPageTests
{
    /// <summary>
    /// Testable wrapper for UsersPage that exposes the protected OnAppearing method
    /// for unit testing purposes.
    /// </summary>
    private class TestableUsersPage : UsersPage
    {
        public TestableUsersPage(UsersViewModel vm) : base(vm)
        {
        }

        /// <summary>
        /// Exposes the protected OnAppearing method for testing.
        /// </summary>
        public void CallOnAppearing()
        {
            OnAppearing();
        }
    }

    /// <summary>
    /// Tests that the constructor handles a null UsersViewModel parameter.
    /// Since the parameter is non-nullable, this tests runtime behavior when null is passed.
    /// The BindingContext should be set to null without throwing an exception.
    /// </summary>
    [TestMethod]
    public void UsersPage_NullUsersViewModel_SetsBindingContextToNull()
    {
        // Arrange
        UsersViewModel? viewModel = null;
        // Act
        var page = new UsersPage(viewModel!);
        // Assert
        Assert.IsNotNull(page);
        Assert.IsNull(page.BindingContext);
    }

    /// <summary>
    /// Tests that the constructor with a valid UsersViewModel successfully creates a UsersPage instance.
    /// Verifies that the constructor executes without throwing exceptions and that
    /// the created instance is properly initialized.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidViewModel_CreatesInstanceSuccessfully()
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        // Act
        var page = new UsersPage(viewModel);
        // Assert
        Assert.IsNotNull(page);
        Assert.IsInstanceOfType(page, typeof(UsersPage));
    }

    /// <summary>
    /// Tests that the constructor with a valid UsersViewModel sets the BindingContext property correctly.
    /// Verifies that the BindingContext is assigned to the provided UsersViewModel instance.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidViewModel_SetsBindingContextCorrectly()
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        // Act
        var page = new UsersPage(viewModel);
        // Assert
        Assert.IsNotNull(page.BindingContext);
        Assert.AreSame(viewModel, page.BindingContext);
    }

    /// <summary>
    /// Tests that the UsersPage inherits from ContentPage as expected.
    /// Verifies that the page instance is of type ContentPage, confirming proper inheritance.
    /// </summary>
    [TestMethod]
    public void Constructor_ValidViewModel_InheritsFromContentPage()
    {
        // Arrange
        var mockAdminService = new Mock<IAdminService>();
        var mockLogger = new Mock<ILogger<UsersViewModel>>();
        var viewModel = new UsersViewModel(mockAdminService.Object, new Mock<IRefreshCoordinator>().Object, mockLogger.Object);
        // Act
        var page = new UsersPage(viewModel);
        // Assert
        Assert.IsInstanceOfType(page, typeof(ContentPage));
    }

}