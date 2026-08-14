using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Domain.Entities;

namespace ZakirPro.Features.Auth.ChangePassword;

public class Handler : IRequestHandler<Command, EndpointResponse<string>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public Handler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<EndpointResponse<string>> Handle(Command request, CancellationToken cancellationToken)
    {
        // 1. Get current user from ICurrentUserService
        var userId = _currentUser.UserId;
        if (userId is null)
            return EndpointResponse<string>.ErrorResponse("Unauthenticated.");

        // 2. Find user by ID (ignore filters, check IsDeleted manually)
        var repo = _uow.GetRepository<User>();
        var user = await repo.QueryIgnoreFilters()
            .FirstOrDefaultAsync(u => u.Id == userId.Value && !u.IsDeleted, cancellationToken);

        if (user is null)
            return EndpointResponse<string>.NotFoundResponse("User not found.");

        // 3. Verify current password
        var hasher = new PasswordHasher<User>();
        var verificationResult = hasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword);
        if (verificationResult == PasswordVerificationResult.Failed)
            return EndpointResponse<string>.ErrorResponse("Current password is incorrect.");

        // 4. Hash and set the new password
        user.PasswordHash = hasher.HashPassword(user, request.NewPassword);

        // 5. Persist
        repo.Update(user);
        await _uow.SaveChangesAsync();

        return EndpointResponse<string>.SuccessResponse("Password changed successfully.", "Password changed successfully.");
    }
}
