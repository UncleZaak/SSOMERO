using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Ssomero.Models;

namespace Ssomero.Interfaces;

public interface IAdminService
{
    Task<List<UserItem>> GetStudentsAsync(CancellationToken ct = default);
    Task<List<UserItem>> GetLecturersAsync(CancellationToken ct = default);
    Task<bool> SuspendStudentAsync(Guid id, CancellationToken ct = default);
    Task<bool> ActivateStudentAsync(Guid id, CancellationToken ct = default);
    Task<bool> DeleteStudentAsync(Guid id, CancellationToken ct = default);
    Task<bool> SuspendLecturerAsync(Guid id, CancellationToken ct = default);
    Task<bool> ActivateLecturerAsync(Guid id, CancellationToken ct = default);
    Task<bool> DeleteLecturerAsync(Guid id, CancellationToken ct = default);
    Task<bool> ApproveLecturerAsync(Guid id, CancellationToken ct = default);

    // Dashboard stats
    Task<AdminStatsDto?> GetAdminStatsAsync(CancellationToken ct = default);

    // Classes
    Task<List<ClassDto>> GetAllClassesAsync(string? search = null, CancellationToken ct = default);

    // Attendance
    Task<List<AdminAttendanceSummaryDto>> GetAttendanceSummaryAsync(CancellationToken ct = default);

    // Notifications
    Task<bool> SendNotificationAsync(AdminNotificationRequestDto request, CancellationToken ct = default);

    // Audit Logs
    Task<AuditLogPagedResult?> GetAuditLogsAsync(
        int page = 1,
        int pageSize = 20,
        string? action = null,
        string? entity = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null,
        CancellationToken ct = default);

    // Analytics Trends
    Task<AdminTrendsDto?> GetTrendsAsync(
        DateTime from,
        DateTime to,
        string granularity,
        CancellationToken ct = default);
}
