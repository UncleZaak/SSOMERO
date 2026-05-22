using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ssomero.Views;
using Ssomero.Views.Grades;

namespace Ssomero.Views.Grades.UnitTests;



/// <summary>
/// Unit tests for the GradesPage class.
/// </summary>
[TestClass]
public partial class GradesPageTests
{
    /// <summary>
    /// Tests that the GradesPage constructor successfully creates an instance.
    /// Verifies that the constructor executes without throwing exceptions and that
    /// the created instance is properly initialized as a ContentPage.
    /// </summary>
    [TestMethod]
    public void Constructor_WhenCalled_CreatesInstanceSuccessfully()
    {
        // Arrange & Act
        var gradesPage = new GradesPage();

        // Assert
        Assert.IsNotNull(gradesPage);
        Assert.IsInstanceOfType(gradesPage, typeof(GradesPage));
        Assert.IsInstanceOfType(gradesPage, typeof(ContentPage));
    }
}