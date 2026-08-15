using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Data;

namespace ZakirPro.Features.SuperAdmin.GetRecentActivity;

public class Handler : IRequestHandler<Query, EndpointResponse<List<ActivityFeedItemDto>>>
{
    private readonly AppDbContext _db;

    // Hard cap — regardless of what the client requests, never return more than this
    private const int MaxLimit = 100;

    public Handler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<EndpointResponse<List<ActivityFeedItemDto>>> Handle(
        Query request, CancellationToken cancellationToken)
    {
        int limit = Math.Clamp(request.Limit, 1, MaxLimit);

        // 1. Fetch the most recent N audit log entries (no global soft-delete filter on AuditLog)
        var logs = await _db.AuditLogs
            .OrderByDescending(a => a.CreatedAt)
            .Take(limit)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        // 2. Safely resolve actor Guids.
        //    AuditLog.UserId is stored as string? — it may be:
        //      - null              (system event, e.g. ExamAutoSubmitService)
        //      - a valid Guid string
        //      - any other string  (malformed / legacy)
        //    We parse defensively and collect only the valid Guid set to avoid
        //    unnecessary joins or exceptions.
        var actorGuidMap = logs
            .Select(a => a.UserId)
            .Where(uid => uid is not null && Guid.TryParse(uid, out _))
            .Select(uid => Guid.Parse(uid!))
            .Distinct()
            .ToHashSet();

        // 3. Single JOIN to Users for all resolvable actor Guids
        Dictionary<Guid, string> actorNames = [];
        if (actorGuidMap.Count > 0)
        {
            actorNames = await _db.Users
                .Where(u => actorGuidMap.Contains(u.Id))
                .AsNoTracking()
                .ToDictionaryAsync(u => u.Id, u => $"{u.FullName} ({u.Role})", cancellationToken);
        }

        // 4. Project to DTO — for each log entry, resolve actor name or return null
        var result = logs.Select(a =>
        {
            string? actorFullName = null;
            if (a.UserId is not null && Guid.TryParse(a.UserId, out var actorGuid))
                actorNames.TryGetValue(actorGuid, out actorFullName); // null if user was deleted

            return new ActivityFeedItemDto(
                Id:           a.Id,
                Action:       a.Action,
                ActorId:      a.UserId,
                ActorFullName: actorFullName,   // null for system events, non-Guid entries, or deleted actors
                TargetId:     a.TargetId,
                Description:  a.Description,
                IpAddress:    a.IPAddress,
                OccurredAt:   a.CreatedAt);
        }).ToList();

        return EndpointResponse<List<ActivityFeedItemDto>>.SuccessResponse(
            result, "Recent activity retrieved successfully.");
    }
}
