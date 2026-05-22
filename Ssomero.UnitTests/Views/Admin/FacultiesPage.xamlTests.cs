using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ssomero.Interfaces;
using Ssomero.ViewModels;
using Ssomero.Views.Admin;

namespace Ssomero.Views.Admin.UnitTests;


/// <summary>
/// Unit tests for the <see cref = "FacultiesPage"/> class.
/// </summary>
[TestClass]
public partial class FacultiesPageTests
{
    /// <summary>
    /// Tests that the constructor properly initializes the page when provided with a valid ViewModel.
    /// Verifies that the BindingContext is correctly set to the provided ViewModel instance.
    /// </summary>
    [TestMethod]
    public void FacultiesPage_WithValidViewModel_SetsBindingContextCorrectly()
    {
        // Arrange
        var mockViewModel = new Mock<FacultiesViewModel>(Mock.Of<IAcademicService>(), Mock.Of<ILogger<FacultiesViewModel>>());
        // Act
        var page = new FacultiesPage(mockViewModel.Object);
        // Assert
        Assert.IsNotNull(page);
        Assert.AreSame(mockViewModel.Object, page.BindingContext);
    }

    /// <summary>
    /// Tests that the constructor handles a null ViewModel parameter.
    /// Even though the parameter is marked as non-nullable, this test verifies runtime behavior
    /// when null is passed, which sets BindingContext to null without throwing an exception.
    /// </summary>
    [TestMethod]
    public void FacultiesPage_WithNullViewModel_SetsBindingContextToNull()
    {
        // Arrange
        FacultiesViewModel nullViewModel = null!;
        // Act
        var page = new FacultiesPage(nullViewModel);
        // Assert
        Assert.IsNotNull(page);
        Assert.IsNull(page.BindingContext);
    }

    /// <summary>
    /// Helper class to expose protected OnAppearing method for testing.
    /// </summary>
    private class TestableFacultiesPage : FacultiesPage
    {
        public TestableFacultiesPage(FacultiesViewModel vm) : base(vm)
        {
        }

        public void InvokeOnAppearing()
        {
            OnAppearing();
        }
    }


}