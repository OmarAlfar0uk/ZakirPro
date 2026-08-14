using ZakirPro.Common.Abstractions;
using ZakirPro.Data;
using ZakirPro.Domain.Entities;

namespace ZakirPro.Common.Infrastructure;

public class AuditLogger : IAuditLogger
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuditLogger> _logger;

    public AuditLogger(IServiceScopeFactory scopeFactory, ILogger<AuditLogger> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public Task LogAsync(string action, Guid? userId, Guid? targetId, string? description, string? ipAddress)
        => LogAsync(action, userId?.ToString(), targetId?.ToString(), description, ipAddress);

    public Task LogAsync(string action, Guid? userId, string? targetId, string? description, string? ipAddress)
        => LogAsync(action, userId?.ToString(), targetId, description, ipAddress);

    /// <summary>
    /// Writes an audit log entry. This method NEVER throws — any failure is caught,
    /// logged via ILogger, and silently discarded so callers are not affected.
    /// </summary>
    public async Task LogAsync(
        string action,
        string? userId,
        string? targetId,
        string? description,
        string? ipAddress)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.AuditLogs.Add(new AuditLog
            {
                Action      = action,
                UserId      = userId,
                TargetId    = targetId,
                Description = description,
                IPAddress   = ipAddress,
                CreatedAt   = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "AuditLogger failed to persist. Action={Action} UserId={UserId} TargetId={TargetId}",
                action, userId, targetId);
        }
    }
}
