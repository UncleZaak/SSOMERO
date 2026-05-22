using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ssomero.Views.Notifications;

namespace Ssomero.Views.Notifications.UnitTests;



/// <summary>
/// Unit tests for the <see cref="NotificationsPage"/> class.
/// </summary>
[TestClass]
public partial class NotificationsPageTests
{
    /// <summary>
    /// Tests that the NotificationsPage constructor successfully creates an instance
    /// without throwing exceptions.
    /// NOTE: This test requires MAUI infrastructure to be properly initialized.
    /// The constructor calls InitializeComponent() which is auto-generated and loads XAML resources.
    /// In environments where XAML infrastructure is not available, this test may fail or need to be marked as [Ignore].
    /// </summary>
    [TestMethod]
    public void Constructor_WhenCalled_CreatesInstanceSuccessfully()
    {
        // Arrange & Act
        NotificationsPage? page = null;
        Exception? exception = null;

        try
        {
            page = new NotificationsPage();
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        // Assert
        // Note: If XAML infrastructure is not available in the test environment,
        // InitializeComponent() may throw. Consider marking this test with [Ignore]
        // or configuring the test project with MAUI support.
        Assert.IsNull(exception, $"Constructor should not throw an exception. Exception: {exception?.Message}");
        Assert.IsNotNull(page, "Constructor should create a valid NotificationsPage instance.");
        Assert.IsInstanceOfType<NotificationsPage>(page, "Created instance should be of type NotificationsPage.");
        Assert.IsInstanceOfType<ContentPage>(page, "Created instance should inherit from ContentPage.");
    }
}