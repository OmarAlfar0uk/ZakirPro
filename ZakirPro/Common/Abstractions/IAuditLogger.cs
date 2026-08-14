namespace ZakirPro.Common.Abstractions;

public interface IAuditLogger
{
    /// <summary>
    /// Logs an action to the audit_logs table.
    /// This method NEVER throws — failures are swallowed and logged via ILogger.
    /// </summary>
    Task LogAsync(string action, string? userId, string? targetId, string? description, string? ipAddress);
    Task LogAsync(string action, Guid? userId, Guid? targetId, string? description, string? ipAddress);
    Task LogAsync(string action, Guid? userId, string? targetId, string? description, string? ipAddress);
}
