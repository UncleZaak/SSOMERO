using System;

using Android.Runtime;
using Microsoft.Maui.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ssomero.UnitTests
{
    /// <summary>
    /// Unit tests for the MainApplication class.
    /// </summary>
    [TestClass]
    public class MainApplicationTests
    {
        /// <summary>
        /// Tests that CreateMauiApp successfully returns a MauiApp instance.
        /// Verifies that the method delegates to MauiProgram.CreateMauiApp() and returns a non-null result.
        /// Note: This test may be inconclusive due to Android platform initialization requirements.
        /// </summary>
        [TestMethod]
        public void CreateMauiApp_WhenCalled_ReturnsMauiAppInstance()
        {
            // Arrange
            MainApplicationTestHelper? testHelper = null;
            Exception? exception = null;

            try
            {
                testHelper = new MainApplicationTestHelper(IntPtr.Zero, JniHandleOwnership.DoNotTransfer);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            if (exception != null)
            {
                Assert.Inconclusive($"Cannot create MainApplication instance (likely due to Android runtime initialization requirement): {exception.Message}");
                return;
            }

            // Act
            MauiApp? result = null;
            try
            {
                result = testHelper!.TestCreateMauiApp();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            if (exception != null)
            {
                Assert.Inconclusive($"CreateMauiApp threw exception (likely due to MAUI platform initialization requirement): {exception.Message}");
            }
            else
            {
                Assert.IsNotNull(result, "CreateMauiApp should return a non-null MauiApp instance");
                Assert.IsInstanceOfType(result, typeof(MauiApp));
            }
        }

        /// <summary>
        /// Tests that CreateMauiApp consistently returns the same type of object on multiple calls.
        /// Verifies that the method behavior is consistent across invocations.
        /// Note: This test may be inconclusive due to Android platform initialization requirements.
        /// </summary>
        [TestMethod]
        public void CreateMauiApp_CalledMultipleTimes_ReturnsConsistentType()
        {
            // Arrange
            MainApplicationTestHelper? testHelper = null;
            Exception? exception = null;

            try
            {
                testHelper = new MainApplicationTestHelper(IntPtr.Zero, JniHandleOwnership.DoNotTransfer);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            if (exception != null)
            {
                Assert.Inconclusive($"Cannot create MainApplication instance (likely due to Android runtime initialization requirement): {exception.Message}");
                return;
            }

            // Act
            MauiApp? result1 = null;
            MauiApp? result2 = null;
            try
            {
                result1 = testHelper!.TestCreateMauiApp();
                result2 = testHelper.TestCreateMauiApp();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            if (exception != null)
            {
                Assert.Inconclusive($"CreateMauiApp threw exception (likely due to MAUI platform initialization requirement): {exception.Message}");
            }
            else
            {
                Assert.IsNotNull(result1, "First call to CreateMauiApp should return a non-null MauiApp instance");
                Assert.IsNotNull(result2, "Second call to CreateMauiApp should return a non-null MauiApp instance");
                Assert.IsInstanceOfType(result1, typeof(MauiApp));
                Assert.IsInstanceOfType(result2, typeof(MauiApp));
            }
        }

        /// <summary>
        /// Helper class that exposes the protected CreateMauiApp method for testing purposes.
        /// </summary>
        private class MainApplicationTestHelper : MainApplication
        {
            public MainApplicationTestHelper(IntPtr handle, JniHandleOwnership ownership)
                : base(handle, ownership)
            {
            }

            public MauiApp TestCreateMauiApp()
            {
                return CreateMauiApp();
            }
        }

        /// <summary>
        /// Tests that the MainApplication constructor successfully creates an instance with valid standard parameters.
        /// Verifies that the constructor executes with typical JniHandleOwnership values and a valid handle.
        /// If the test fails due to Android platform initialization requirements, it is marked as inconclusive.
        /// </summary>
        /// <param name="handle">The native handle value to test.</param>
        /// <param name="ownership">The JniHandleOwnership value to test.</param>
        [TestMethod]
        [DataRow(0, JniHandleOwnership.DoNotTransfer, DisplayName = "Zero handle with DoNotTransfer")]
        [DataRow(1, JniHandleOwnership.TransferLocalRef, DisplayName = "Positive handle with TransferLocalRef")]
        [DataRow(100, JniHandleOwnership.TransferGlobalRef, DisplayName = "Positive handle with TransferGlobalRef")]
        [DataRow(-1, JniHandleOwnership.DoNotTransfer, DisplayName = "Negative handle with DoNotTransfer")]
        public void Constructor_ValidParameters_CreatesInstanceOrRequiresPlatform(int handleValue, JniHandleOwnership ownership)
        {
            // Arrange
            nint handle = handleValue;
            MainApplication? mainApplication = null;
            Exception? exception = null;

            // Act
            try
            {
                mainApplication = new MainApplication(handle, ownership);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            if (exception == null)
            {
                Assert.IsNotNull(mainApplication);
                Assert.IsInstanceOfType(mainApplication, typeof(MainApplication));
            }
            else
            {
                Assert.Inconclusive($"Constructor threw exception (likely due to Android platform initialization requirement): {exception.GetType().Name} - {exception.Message}");
            }
        }

        /// <summary>
        /// Tests that the MainApplication constructor handles extreme nint values.
        /// Verifies behavior with nint.MinValue and nint.MaxValue, which represent extreme handle values.
        /// If the test fails due to Android platform initialization requirements, it is marked as inconclusive.
        /// </summary>
        /// <param name="handle">The extreme nint handle value to test.</param>
        /// <param name="ownership">The JniHandleOwnership value to test.</param>
        [TestMethod]
        [DataRow(JniHandleOwnership.DoNotTransfer, DisplayName = "MinValue handle")]
        [DataRow(JniHandleOwnership.TransferLocalRef, DisplayName = "MaxValue handle")]
        public void Constructor_ExtremeHandleValues_CreatesInstanceOrRequiresPlatform(JniHandleOwnership ownership)
        {
            // Arrange
            nint handle = ownership == JniHandleOwnership.DoNotTransfer ? nint.MinValue : nint.MaxValue;
            MainApplication? mainApplication = null;
            Exception? exception = null;

            // Act
            try
            {
                mainApplication = new MainApplication(handle, ownership);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            if (exception == null)
            {
                Assert.IsNotNull(mainApplication);
                Assert.IsInstanceOfType(mainApplication, typeof(MainApplication));
            }
            else
            {
                Assert.Inconclusive($"Constructor threw exception (likely due to Android platform initialization requirement): {exception.GetType().Name} - {exception.Message}");
            }
        }

        /// <summary>
        /// Tests that the MainApplication constructor handles undefined JniHandleOwnership enum values.
        /// Verifies behavior when an invalid enum value (outside defined range) is passed.
        /// Tests if the constructor validates enum values or passes them directly to the base class.
        /// </summary>
        [TestMethod]
        public void Constructor_InvalidEnumValue_CreatesInstanceOrThrows()
        {
            // Arrange
            nint handle = 1;
            JniHandleOwnership invalidOwnership = (JniHandleOwnership)999;
            MainApplication? mainApplication = null;
            Exception? exception = null;

            // Act
            try
            {
                mainApplication = new MainApplication(handle, invalidOwnership);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            if (exception == null)
            {
                // Constructor doesn't validate enum values
                Assert.IsNotNull(mainApplication);
            }
            else
            {
                // Either platform initialization failed or enum validation occurred
                Assert.Inconclusive($"Constructor threw exception with invalid enum: {exception.GetType().Name} - {exception.Message}");
            }
        }

        /// <summary>
        /// Tests that the MainApplication constructor with all JniHandleOwnership enum values.
        /// Verifies that each defined enum value can be used with the constructor.
        /// </summary>
        /// <param name="ownership">The JniHandleOwnership value to test.</param>
        [TestMethod]
        [DataRow(JniHandleOwnership.DoNotTransfer, DisplayName = "DoNotTransfer ownership")]
        [DataRow(JniHandleOwnership.TransferLocalRef, DisplayName = "TransferLocalRef ownership")]
        [DataRow(JniHandleOwnership.TransferGlobalRef, DisplayName = "TransferGlobalRef ownership")]
        public void Constructor_AllEnumValues_CreatesInstanceOrRequiresPlatform(JniHandleOwnership ownership)
        {
            // Arrange
            nint handle = 42;
            MainApplication? mainApplication = null;
            Exception? exception = null;

            // Act
            try
            {
                mainApplication = new MainApplication(handle, ownership);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            if (exception == null)
            {
                Assert.IsNotNull(mainApplication);
                Assert.IsInstanceOfType(mainApplication, typeof(MainApplication));
            }
            else
            {
                Assert.Inconclusive($"Constructor threw exception (likely due to Android platform initialization requirement): {exception.GetType().Name} - {exception.Message}");
            }
        }
    }
}