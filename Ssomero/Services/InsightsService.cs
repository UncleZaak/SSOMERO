using Ssomero.Interfaces;
using Ssomero.Models;

namespace Ssomero.Services;

public class InsightsService : IInsightsService
{
    public IReadOnlyList<string> GenerateDashboardInsights(
        int attendancePct, int missedToday, int upcomingToday, int announcementCount)
    {
        var list = new List<string>();

        if (attendancePct > 0 && attendancePct < 75)
            list.Add($"⚠️ Overall attendance {attendancePct}% — below the 75% requirement.");
        else if (attendancePct >= 90)
            list.Add("🌟 Excellent attendance! You're in the top tier.");

        if (missedToday > 0)
            list.Add($"📋 {missedToday} class{(missedToday > 1 ? "es" : "")} already passed today.");

        if (upcomingToday > 0)
            list.Add($"📅 {upcomingToday} more class{(upcomingToday > 1 ? "es" : "")} remaining today.");

        if (announcementCount > 0)
            list.Add($"📢 {announcementCount} new announcement{(announcementCount > 1 ? "s" : "")} from your lecturers.");

        if (list.Count == 0)
            list.Add("✅ You're on track! Keep attending all your classes.");

        return list;
    }

    public IReadOnlyList<string> GenerateReportInsights(StudentAttendanceReportDto report, int threshold = 75)
    {
        var list = new List<string>();

        if (report.OverallPercent < threshold)
            list.Add($"⚠️ Your overall attendance is {report.OverallPercent:F0}% — below the {threshold}% requirement.");

        foreach (var s in report.CourseStats)
        {
            // Comparison vs class average
            if (s.ClassAvgPercent > 0)
            {
                var diff = s.AttendancePercent - s.ClassAvgPercent;
                if (diff >= 10)
                    list.Add($"🌟 {s.CourseName}: you're {diff:F0}% above the class average ({s.ClassAvgPercent:F0}%).");
                else if (diff <= -10)
                    list.Add($"📉 {s.CourseName}: you're {Math.Abs(diff):F0}% below the class average ({s.ClassAvgPercent:F0}%).");
            }

            // Risk prediction
            if (s.AttendancePercent < threshold)
            {
                var prediction = PredictRisk(s, threshold);
                if (prediction is not null)
                    list.Add(prediction);
                else
                    list.Add($"📉 Attendance dropping in {s.CourseName} ({s.AttendancePercent:F0}%).");
            }
            else
            {
                var risk = PredictRisk(s, threshold);
                if (risk is not null) list.Add(risk);
            }
        }

        if (!list.Any())
            list.Add("✅ You're on track! Keep attending all your classes.");

        return list;
    }

    public string? PredictRisk(AttendanceStatsDto stat, int threshold = 75)
    {
        if (stat.TotalSessions == 0) return null;

        // How many more absences can the student afford?
        // attended / (total + future) >= threshold/100
        // Future sessions to add: solve for n where:
        //   attended / (total + n) = threshold / 100
        //   → n = attended * 100 / threshold - total
        var maxTotal   = (int)Math.Floor(stat.AttendedSessions * 100.0 / threshold);
        var canAfford  = maxTotal - stat.TotalSessions;

        if (canAfford <= 0)
            return $"🔴 You cannot miss any more classes in {stat.CourseName} to stay above {threshold}%.";

        if (canAfford <= 2)
            return $"⏰ You can only afford {canAfford} more absence{(canAfford > 1 ? "s" : "")} in {stat.CourseName}.";

        return null;
    }
}
