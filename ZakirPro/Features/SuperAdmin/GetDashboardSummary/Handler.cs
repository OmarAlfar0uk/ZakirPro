using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Data;
using ZakirPro.Domain.Entities;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.SuperAdmin.GetDashboardSummary;

public class Handler : IRequestHandler<Query, EndpointResponse<DashboardSummaryDto>>
{
    private readonly AppDbContext _db;

    public Handler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<EndpointResponse<DashboardSummaryDto>> Handle(
        Query request, CancellationToken cancellationToken)
    {
        var now       = DateTime.UtcNow;
        var weekAgo   = now.AddDays(-7);
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        // ── User counts ────────────────────────────────────────────────────────
        // Single pass over the Users table; EF will translate each to COUNT(*) FILTER(WHERE ...)
        // via GroupBy or parallel single-column aggregates. We prefer one trip per concern for clarity.
        var totalTeachers   = await _db.Users.CountAsync(u => u.Role == UserRole.Teacher,   cancellationToken);
        var totalStudents   = await _db.Users.CountAsync(u => u.Role == UserRole.Student,   cancellationToken);
        var totalAdmins     = await _db.Users.CountAsync(u => u.Role == UserRole.Admin,     cancellationToken);
        var totalAssistants = await _db.Users.CountAsync(u => u.Role == UserRole.Assistant, cancellationToken);

        // "New" = Students + Teachers only (roles shown in the growth chart)
        var newThisWeek = await _db.Users.CountAsync(u =>
            (u.Role == UserRole.Student || u.Role == UserRole.Teacher) &&
            u.CreatedAt >= weekAgo,
            cancellationToken);

        var newThisMonth = await _db.Users.CountAsync(u =>
            (u.Role == UserRole.Student || u.Role == UserRole.Teacher) &&
            u.CreatedAt >= monthStart,
            cancellationToken);

        // ── Content counts ──────────────────────────────────────────────────────
        // Global query filters (IsDeleted == false) apply automatically on all navigable DbSets.
        var totalTss      = await _db.TeacherSubjectStages.CountAsync(cancellationToken);
        var totalLectures = await _db.Lectures.CountAsync(cancellationToken);

        // Exam status breakdown — one GROUP BY STATUS aggregation
        var examByStatus = await _db.Exams
            .GroupBy(e => e.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        int examDraft     = examByStatus.FirstOrDefault(x => x.Status == ExamStatus.Draft)?.Count     ?? 0;
        int examPublished = examByStatus.FirstOrDefault(x => x.Status == ExamStatus.Published)?.Count ?? 0;
        int examArchived  = examByStatus.FirstOrDefault(x => x.Status == ExamStatus.Archived)?.Count  ?? 0;

        return EndpointResponse<DashboardSummaryDto>.SuccessResponse(
            new DashboardSummaryDto(
                Users: new UserCountsDto(
                    TotalTeachers:   totalTeachers,
                    TotalStudents:   totalStudents,
                    TotalAdmins:     totalAdmins,
                    TotalAssistants: totalAssistants,
                    NewThisWeek:     newThisWeek,
                    NewThisMonth:    newThisMonth),
                Content: new ContentCountsDto(
                    TotalTeacherSubjectStageAssignments: totalTss,
                    TotalLectures: totalLectures,
                    Exams: new ExamCountsDto(
                        Draft:     examDraft,
                        Published: examPublished,
                        Archived:  examArchived)),
                NotYetAvailable: ["attendance", "assignments"]),
            "Dashboard summary retrieved successfully.");
    }
}
