using Microsoft.Maui.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ssomero.Views.Chat;

namespace Ssomero.Views.Chat.UnitTests;



/// <summary>
/// Unit tests for the <see cref="ChatPage"/> class.
/// </summary>
[TestClass]
public partial class ChatPageTests
{
    /// <summary>
    /// Tests that the ChatPage constructor successfully creates an instance without throwing exceptions.
    /// Note: This test requires MAUI infrastructure to be properly initialized. If the test fails,
    /// it may be due to missing MAUI application context or platform initialization.
    /// </summary>
    [TestMethod]
    public void ChatPage_Constructor_CreatesInstanceSuccessfully()
    {
        // Arrange & Act
        ChatPage? chatPage = null;
        var exception = Record.Exception(() => chatPage = new ChatPage());

        // Assert
        Assert.IsNull(exception, "Constructor should not throw any exceptions.");
        Assert.IsNotNull(chatPage, "ChatPage instance should not be null.");
    }

    /// <summary>
    /// Tests that the ChatPage instance is of the correct type and inherits from ContentPage.
    /// </summary>
    [TestMethod]
    public void ChatPage_Constructor_CreatesCorrectType()
    {
        // Arrange & Act
        var chatPage = new ChatPage();

        // Assert
        Assert.IsInstanceOfType<ChatPage>(chatPage, "Instance should be of type ChatPage.");
        Assert.IsInstanceOfType<ContentPage>(chatPage, "Instance should inherit from ContentPage.");
    }

    /// <summary>
    /// Helper method to record exceptions during action execution.
    /// </summary>
    private static class Record
    {
        public static Exception? Exception(Action action)
        {
            try
            {
                action();
                return null;
            }
            catch (Exception ex)
            {
                return ex;
            }
        }
    }
}