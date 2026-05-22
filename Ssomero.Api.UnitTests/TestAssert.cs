global using Assert = Ssomero.Api.UnitTests.TestAssert;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MST = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Ssomero.Api.UnitTests
{
    public static class TestAssert
    {
        // Helper to format messages with optional args to match MSTest signatures
        private static string? F(string? message, params object[]? args)
            => message is null ? null : (args is null || args.Length == 0 ? message : string.Format(message, args));

        // Forward common MSTest Assert methods used across tests
        public static void IsTrue(bool condition) => MST.IsTrue(condition);
        public static void IsTrue(bool condition, string? message) => MST.IsTrue(condition, message);
        public static void IsTrue(bool condition, string? message, params object[] messageArgs) => MST.IsTrue(condition, F(message, messageArgs));
        public static void IsFalse(bool condition) => MST.IsFalse(condition);
        public static void IsFalse(bool condition, string? message) => MST.IsFalse(condition, message);
        public static void IsFalse(bool condition, string? message, params object[] messageArgs) => MST.IsFalse(condition, F(message, messageArgs));
        public static void IsNull(object? value) => MST.IsNull(value);
        public static void IsNull(object? value, string? message) => MST.IsNull(value, message);
        public static void IsNull(object? value, string? message, params object[] messageArgs) => MST.IsNull(value, F(message, messageArgs));
        public static void IsNotNull(object? value) => MST.IsNotNull(value);
        public static void IsNotNull(object? value, string? message) => MST.IsNotNull(value, message);
        public static void IsNotNull(object? value, string? message, params object[] messageArgs) => MST.IsNotNull(value, F(message, messageArgs));
        public static void AreEqual<T>(T? expected, T? actual) => MST.AreEqual(expected, actual);
        public static void AreEqual<T>(T? expected, T? actual, string? message) => MST.AreEqual(expected, actual, message);
        public static void AreEqual<T>(T? expected, T? actual, string? message, params object[] messageArgs) => MST.AreEqual(expected, actual, F(message, messageArgs));
        public static void AreEqual(object? expected, object? actual) => MST.AreEqual(expected, actual);
        public static void AreEqual(object? expected, object? actual, string? message) => MST.AreEqual(expected, actual, message);
        public static void AreEqual(object? expected, object? actual, string? message, params object[] messageArgs) => MST.AreEqual(expected, actual, F(message, messageArgs));
        public static void AreEqual(int expected, int actual) => MST.AreEqual(expected, actual);
        public static void AreEqual(int expected, int actual, string? message) => MST.AreEqual(expected, actual, message);
        public static void AreEqual(int expected, int actual, string? message, params object[] messageArgs) => MST.AreEqual(expected, actual, F(message, messageArgs));
        public static void AreEqual(string? expected, string? actual) => MST.AreEqual(expected, actual);
        public static void AreEqual(string? expected, string? actual, string? message) => MST.AreEqual(expected, actual, message);
        public static void AreEqual(string? expected, string? actual, string? message, params object[] messageArgs) => MST.AreEqual(expected, actual, F(message, messageArgs));
        public static void AreNotEqual(object? expected, object? actual) => MST.AreNotEqual(expected, actual);
        public static void AreNotEqual(object? expected, object? actual, string? message) => MST.AreNotEqual(expected, actual, message);
        public static void AreNotEqual(object? expected, object? actual, string? message, params object[] messageArgs) => MST.AreNotEqual(expected, actual, F(message, messageArgs));
        public static void Fail(string? message) => MST.Fail(message ?? string.Empty);
        public static void Fail(string? message, params object[] messageArgs) => MST.Fail(F(message, messageArgs) ?? string.Empty);
        public static void IsInstanceOfType(object? value, Type expectedType) => MST.IsInstanceOfType(value, expectedType);
        public static void IsInstanceOfType(object? value, Type expectedType, string? message) => MST.IsInstanceOfType(value, expectedType, message);
        public static void IsInstanceOfType(object? value, Type expectedType, string? message, params object[] messageArgs) => MST.IsInstanceOfType(value, expectedType, F(message, messageArgs));
        public static void IsInstanceOfType<T>(object? value) => MST.IsInstanceOfType(value, typeof(T));
        public static void IsInstanceOfType<T>(object? value, string? message) => MST.IsInstanceOfType(value, typeof(T), message);
        public static void IsInstanceOfType<T>(object? value, string? message, params object[] messageArgs) => MST.IsInstanceOfType(value, typeof(T), F(message, messageArgs));

        public static void Inconclusive(string? message) => MST.Inconclusive(message ?? string.Empty);
        public static void Inconclusive(string? message, params object[] messageArgs) => MST.Inconclusive(F(message, messageArgs) ?? string.Empty);

        // Custom helpers used by tests
        public static void IsEmpty<T>(IEnumerable<T>? collection)
        {
            var count = collection?.Count() ?? 0;
            MST.AreEqual(0, count);
        }

        public static void HasCount<T>(int expected, IEnumerable<T>? collection)
        {
            var count = collection?.Count() ?? 0;
            MST.AreEqual(expected, count);
        }

        public static void IsEmpty(string? s)
        {
            MST.IsTrue(string.IsNullOrEmpty(s));
        }

        public static async Task ThrowsExactlyAsync<TException>(Func<Task> action)
            where TException : Exception
        {
            try
            {
                await action();
                MST.Fail($"Expected exception of type {typeof(TException)}, but no exception was thrown.");
            }
            catch (TException)
            {
                // expected
            }
            catch (Exception ex)
            {
                MST.Fail($"Expected exception of type {typeof(TException)}, but caught {ex.GetType()}.");
            }
        }
    }
}
