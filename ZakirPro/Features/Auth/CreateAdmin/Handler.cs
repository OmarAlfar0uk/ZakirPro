using MediatR;
using Microsoft.AspNetCore.Identity;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Domain.Entities;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.Auth.CreateAdmin;

public class Handler : IRequestHandler<Command, EndpointResponse<CreateAdminResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuditLogger _auditLogger;

    public Handler(IUnitOfWork uow, ICurrentUserService currentUser, IAuditLogger auditLogger)
    {
        _uow = uow;
        _currentUser = currentUser;
        _auditLogger = auditLogger;
    }

    public async Task<EndpointResponse<CreateAdminResponse>> Handle(Command request, CancellationToken cancellationToken)
    {
        var userRepo = _uow.GetRepository<User>();

        // Check email uniqueness
        var emailExists = await userRepo.ExistsAsync(u => u.Email == request.Email);
        if (emailExists)
            return EndpointResponse<CreateAdminResponse>.ErrorResponse("A user with this email already exists.");

        // Create Admin entity
        var admin = new Admin
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName,
            Email = request.Email,
            Role = UserRole.Admin,
            IsActive = true,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        // Hash password
        var hasher = new PasswordHasher<User>();
        admin.PasswordHash = hasher.HashPassword(admin, request.Password);

        await userRepo.AddAsync(admin);
        await _uow.SaveChangesAsync();

        // Audit log (fire-and-forget)
        _ = _auditLogger.LogAsync(
            action: "CreateAdmin",
            userId: _currentUser.UserId,
            targetId: admin.Id,
            description: $"SuperAdmin created a new Admin account for {admin.Email}",
            ipAddress: _currentUser.IpAddress);

        return EndpointResponse<CreateAdminResponse>.SuccessResponse(
            new CreateAdminResponse(admin.Id, admin.FullName, admin.Email),
            "Admin account created successfully.");
    }
}
