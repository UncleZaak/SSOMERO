using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ssomero.Models;

namespace Ssomero.Models.UnitTests;


/// <summary>
/// Unit tests for the <see cref="AttendanceStatsDto"/> class.
/// </summary>
[TestClass]
public class AttendanceStatsDtoTests
{
    /// <summary>
    /// Tests that AttendancePercent returns 0 when TotalSessions is 0,
    /// preventing division by zero.
    /// </summary>
    /// <param name="attendedSessions">The attended sessions count.</param>
    [TestMethod]
    [DataRow(0)]
    [DataRow(5)]
    [DataRow(100)]
    public void AttendancePercent_TotalSessionsIsZero_ReturnsZero(int attendedSessions)
    {
        // Arrange
        var dto = new AttendanceStatsDto
        {
            TotalSessions = 0,
            AttendedSessions = attendedSessions
        };

        // Act
        double result = dto.AttendancePercent;

        // Assert
        Assert.AreEqual(0.0, result);
    }

    /// <summary>
    /// Tests that AttendancePercent returns 0 when TotalSessions is negative.
    /// </summary>
    /// <param name="totalSessions">The total sessions count (negative).</param>
    /// <param name="attendedSessions">The attended sessions count.</param>
    [TestMethod]
    [DataRow(-1, 0)]
    [DataRow(-5, 5)]
    [DataRow(-100, 50)]
    public void AttendancePercent_TotalSessionsIsNegative_ReturnsZero(int totalSessions, int attendedSessions)
    {
        // Arrange
        var dto = new AttendanceStatsDto
        {
            TotalSessions = totalSessions,
            AttendedSessions = attendedSessions
        };

        // Act
        double result = dto.AttendancePercent;

        // Assert
        Assert.AreEqual(0.0, result);
    }

    /// <summary>
    /// Tests that AttendancePercent returns 0 when AttendedSessions is 0
    /// and TotalSessions is positive.
    /// </summary>
    /// <param name="totalSessions">The total sessions count.</param>
    [TestMethod]
    [DataRow(1)]
    [DataRow(10)]
    [DataRow(100)]
    public void AttendancePercent_AttendedSessionsIsZero_ReturnsZero(int totalSessions)
    {
        // Arrange
        var dto = new AttendanceStatsDto
        {
            TotalSessions = totalSessions,
            AttendedSessions = 0
        };

        // Act
        double result = dto.AttendancePercent;

        // Assert
        Assert.AreEqual(0.0, result);
    }

    /// <summary>
    /// Tests that AttendancePercent returns 100 when AttendedSessions equals TotalSessions.
    /// </summary>
    /// <param name="sessions">The number of sessions (both total and attended).</param>
    [TestMethod]
    [DataRow(1)]
    [DataRow(10)]
    [DataRow(100)]
    [DataRow(1000)]
    public void AttendancePercent_PerfectAttendance_ReturnsOneHundred(int sessions)
    {
        // Arrange
        var dto = new AttendanceStatsDto
        {
            TotalSessions = sessions,
            AttendedSessions = sessions
        };

        // Act
        double result = dto.AttendancePercent;

        // Assert
        Assert.AreEqual(100.0, result);
    }

    /// <summary>
    /// Tests that AttendancePercent correctly calculates percentage for various valid inputs.
    /// </summary>
    /// <param name="totalSessions">The total sessions count.</param>
    /// <param name="attendedSessions">The attended sessions count.</param>
    /// <param name="expectedPercent">The expected attendance percentage.</param>
    [TestMethod]
    [DataRow(10, 5, 50.0)]
    [DataRow(4, 2, 50.0)]
    [DataRow(10, 7, 70.0)]
    [DataRow(10, 3, 30.0)]
    [DataRow(5, 1, 20.0)]
    [DataRow(20, 15, 75.0)]
    [DataRow(100, 25, 25.0)]
    public void AttendancePercent_ValidInputs_ReturnsCorrectPercentage(int totalSessions, int attendedSessions, double expectedPercent)
    {
        // Arrange
        var dto = new AttendanceStatsDto
        {
            TotalSessions = totalSessions,
            AttendedSessions = attendedSessions
        };

        // Act
        double result = dto.AttendancePercent;

        // Assert
        Assert.AreEqual(expectedPercent, result, 0.0001);
    }

    /// <summary>
    /// Tests that AttendancePercent returns a fractional percentage when division
    /// does not result in a whole number.
    /// </summary>
    [TestMethod]
    public void AttendancePercent_FractionalResult_ReturnsCorrectValue()
    {
        // Arrange
        var dto = new AttendanceStatsDto
        {
            TotalSessions = 3,
            AttendedSessions = 2
        };

        // Act
        double result = dto.AttendancePercent;

        // Assert
        Assert.AreEqual(66.66666666666667, result, 0.0001);
    }

    /// <summary>
    /// Tests that AttendancePercent handles over-attendance scenario
    /// (AttendedSessions > TotalSessions), which may indicate data integrity issues.
    /// </summary>
    /// <param name="totalSessions">The total sessions count.</param>
    /// <param name="attendedSessions">The attended sessions count (greater than total).</param>
    [TestMethod]
    [DataRow(10, 15)]
    [DataRow(5, 10)]
    [DataRow(1, 5)]
    public void AttendancePercent_OverAttendance_ReturnsPercentageGreaterThanOneHundred(int totalSessions, int attendedSessions)
    {
        // Arrange
        var dto = new AttendanceStatsDto
        {
            TotalSessions = totalSessions,
            AttendedSessions = attendedSessions
        };

        // Act
        double result = dto.AttendancePercent;

        // Assert
        Assert.IsTrue(result > 100.0);
    }

    /// <summary>
    /// Tests that AttendancePercent returns a negative percentage when AttendedSessions
    /// is negative and TotalSessions is positive.
    /// </summary>
    /// <param name="totalSessions">The total sessions count (positive).</param>
    /// <param name="attendedSessions">The attended sessions count (negative).</param>
    [TestMethod]
    [DataRow(10, -5)]
    [DataRow(5, -1)]
    [DataRow(100, -50)]
    public void AttendancePercent_NegativeAttendedSessions_ReturnsNegativePercentage(int totalSessions, int attendedSessions)
    {
        // Arrange
        var dto = new AttendanceStatsDto
        {
            TotalSessions = totalSessions,
            AttendedSessions = attendedSessions
        };

        // Act
        double result = dto.AttendancePercent;

        // Assert
        Assert.IsTrue(result < 0.0);
    }

    /// <summary>
    /// Tests that AttendancePercent handles large values without overflow.
    /// </summary>
    [TestMethod]
    public void AttendancePercent_LargeValues_ReturnsCorrectPercentage()
    {
        // Arrange
        var dto = new AttendanceStatsDto
        {
            TotalSessions = 1000000,
            AttendedSessions = 500000
        };

        // Act
        double result = dto.AttendancePercent;

        // Assert
        Assert.AreEqual(50.0, result, 0.0001);
    }

    /// <summary>
    /// Tests that AttendancePercent handles extreme boundary value int.MaxValue
    /// for TotalSessions without overflow.
    /// </summary>
    [TestMethod]
    public void AttendancePercent_MaxIntTotalSessions_ReturnsCorrectPercentage()
    {
        // Arrange
        var dto = new AttendanceStatsDto
        {
            TotalSessions = int.MaxValue,
            AttendedSessions = int.MaxValue / 2
        };

        // Act
        double result = dto.AttendancePercent;

        // Assert
        Assert.IsTrue(result >= 49.9 && result <= 50.1);
    }

    /// <summary>
    /// Tests that AttendancePercent returns 0 when TotalSessions is int.MinValue
    /// (negative boundary).
    /// </summary>
    [TestMethod]
    public void AttendancePercent_MinIntTotalSessions_ReturnsZero()
    {
        // Arrange
        var dto = new AttendanceStatsDto
        {
            TotalSessions = int.MinValue,
            AttendedSessions = 0
        };

        // Act
        double result = dto.AttendancePercent;

        // Assert
        Assert.AreEqual(0.0, result);
    }

    /// <summary>
    /// Tests that AttendancePercent handles both properties set to int.MinValue.
    /// </summary>
    [TestMethod]
    public void AttendancePercent_BothMinInt_ReturnsZero()
    {
        // Arrange
        var dto = new AttendanceStatsDto
        {
            TotalSessions = int.MinValue,
            AttendedSessions = int.MinValue
        };

        // Act
        double result = dto.AttendancePercent;

        // Assert
        Assert.AreEqual(0.0, result);
    }

    /// <summary>
    /// Tests that AttendancePercent handles both properties set to int.MaxValue.
    /// </summary>
    [TestMethod]
    public void AttendancePercent_BothMaxInt_ReturnsOneHundred()
    {
        // Arrange
        var dto = new AttendanceStatsDto
        {
            TotalSessions = int.MaxValue,
            AttendedSessions = int.MaxValue
        };

        // Act
        double result = dto.AttendancePercent;

        // Assert
        Assert.AreEqual(100.0, result, 0.0001);
    }
}