using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.SuperAdmin.GetRecentActivity;

public record Query(int Limit) : IRequest<EndpointResponse<List<ActivityFeedItemDto>>>;

public record ActivityFeedItemDto(
    Guid    Id,
    string  Action,
    string? ActorId,
    string? ActorFullName,   // null for system events (background service, null UserId, or non-Guid UserId)
    string? TargetId,
    string? Description,
    string? IpAddress,
    DateTime OccurredAt);
