using Ssomero.Models;

namespace Ssomero.Interfaces;

public interface IInsightsService
{
    /// <summary>Generates smart insight messages for the dashboard.</summary>
    IReadOnlyList<string> GenerateDashboardInsights(int attendancePct, int missedToday, int upcomingToday, int announcementCount);

    /// <summary>Generates per-report insights for the analytics page.</summary>
    IReadOnlyList<string> GenerateReportInsights(StudentAttendanceReportDto report, int threshold = 75);

    /// <summary>
    /// Predicts how many more absences a student can afford before
    /// falling below <paramref name="threshold"/>%.
    /// Returns null when there is no risk.
    /// </summary>
    string? PredictRisk(AttendanceStatsDto stat, int threshold = 75);
}
