using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ssomero;
using Ssomero.Components;

namespace Ssomero.Components.UnitTests;



/// <summary>
/// Unit tests for the <see cref="CardView"/> class.
/// </summary>
[TestClass]
public partial class CardViewTests
{
    /// <summary>
    /// Tests that the CardView constructor successfully creates an instance.
    /// Note: This test requires MAUI application infrastructure to be initialized
    /// for InitializeComponent() to execute properly. If the test fails with XAML
    /// parsing errors, ensure your test project is properly configured as a MAUI
    /// test project with appropriate application initialization.
    /// </summary>
    [TestMethod]
    public void Constructor_WhenCalled_CreatesInstanceSuccessfully()
    {
        // Arrange & Act
        CardView? cardView = null;
        Exception? exception = null;

        try
        {
            cardView = new CardView();
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        // Assert
        Assert.IsNull(exception, $"Constructor should not throw an exception. Exception: {exception?.Message}");
        Assert.IsNotNull(cardView, "CardView instance should be created.");
        Assert.IsInstanceOfType(cardView, typeof(ContentView), "CardView should be an instance of ContentView.");
    }
}