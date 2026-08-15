using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Data;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.SuperAdmin.GetGrowthTrend;

public class Handler : IRequestHandler<Query, EndpointResponse<GrowthTrendDto>>
{
    private readonly AppDbContext _db;

    public Handler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<EndpointResponse<GrowthTrendDto>> Handle(
        Query request, CancellationToken cancellationToken)
    {
        // Normalise period parameter — default to week for any unrecognised value
        var period = request.Period?.Trim().ToLowerInvariant() switch
        {
            "month" => "month",
            _       => "week"
        };

        var utcNow  = DateTime.UtcNow;
        int days    = period == "month" ? 30 : 7;
        var cutoff  = utcNow.AddDays(-days + 1).Date; // inclusive: today - (days-1)

        // Pull raw date-level aggregates for the window.
        // We separate students and teachers so the chart can show two lines.
        // Postgres: DATE("CreatedAt") truncates to the UTC date.
        // EF translates DateOnly via Npgsql; using DateTime.Date is safe on EF Core 8+.
        var rawStudents = await _db.Users
            .Where(u =>
                u.Role == UserRole.Student &&
                u.CreatedAt >= cutoff.ToUniversalTime())
            .GroupBy(u => u.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var rawTeachers = await _db.Users
            .Where(u =>
                u.Role == UserRole.Teacher &&
                u.CreatedAt >= cutoff.ToUniversalTime())
            .GroupBy(u => u.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        // Build lookup dictionaries for O(1) date resolution
        var studentsByDate = rawStudents.ToDictionary(x => DateOnly.FromDateTime(x.Date), x => x.Count);
        var teachersByDate = rawTeachers.ToDictionary(x => DateOnly.FromDateTime(x.Date), x => x.Count);

        // Generate a complete, gap-free date series — dates with zero signups are filled with 0
        var startDate = DateOnly.FromDateTime(cutoff);
        var endDate   = DateOnly.FromDateTime(utcNow.Date);

        var series = new List<GrowthDataPoint>(days);
        for (var d = startDate; d <= endDate; d = d.AddDays(1))
        {
            series.Add(new GrowthDataPoint(
                Date:        d,
                NewStudents: studentsByDate.GetValueOrDefault(d, 0),
                NewTeachers: teachersByDate.GetValueOrDefault(d, 0)));
        }

        return EndpointResponse<GrowthTrendDto>.SuccessResponse(
            new GrowthTrendDto(
                Period:          period,
                SeriesStartDate: startDate,
                SeriesEndDate:   endDate,
                Series:          series),
            "Growth trend retrieved successfully.");
    }
}
