using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ssomero.Views;
using Ssomero.Views.Assignments;

namespace Ssomero.Views.Assignments.UnitTests;



/// <summary>
/// Unit tests for the <see cref="AssignmentsPage"/> class.
/// </summary>
[TestClass]
public partial class AssignmentsPageTests
{
    /// <summary>
    /// Tests that the AssignmentsPage constructor successfully creates an instance.
    /// This test verifies that the parameterless constructor can be called and that
    /// the resulting object is properly initialized as a ContentPage.
    /// Note: This test may require MAUI framework initialization in some environments.
    /// If InitializeComponent() requires XAML resources to be available, consider running
    /// this as an integration test with proper MAUI test host setup.
    /// </summary>
    [TestMethod]
    public void Constructor_WhenCalled_CreatesInstanceSuccessfully()
    {
        // Arrange & Act
        AssignmentsPage? page = null;
        Exception? exception = null;

        try
        {
            page = new AssignmentsPage();
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNull(exception, $"Constructor should not throw an exception. Exception: {exception?.Message}");
        Assert.IsNotNull(page, "Constructor should create a non-null instance.");
        Assert.IsInstanceOfType(page, typeof(ContentPage), "Instance should be of type ContentPage.");
        Assert.IsInstanceOfType(page, typeof(AssignmentsPage), "Instance should be of type AssignmentsPage.");
    }
}