using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Domain.Entities;

namespace ZakirPro.Features.Auth.UpdateProfile;

public class Handler : IRequestHandler<Command, EndpointResponse<UpdateProfileResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public Handler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<EndpointResponse<UpdateProfileResponse>> Handle(Command request, CancellationToken cancellationToken)
    {
        // 1. Get current user from ICurrentUserService
        var userId = _currentUser.UserId;
        if (userId is null)
            return EndpointResponse<UpdateProfileResponse>.ErrorResponse("Unauthenticated.");

        // 2. Find user by ID
        var repo = _uow.GetRepository<User>();
        var user = await repo.Query()
            .FirstOrDefaultAsync(u => u.Id == userId.Value, cancellationToken);

        if (user is null)
            return EndpointResponse<UpdateProfileResponse>.NotFoundResponse("User not found.");

        // 3. Update FullName (UpdatedAt is stamped by repo.Update)
        user.FullName = request.FullName.Trim();

        // 4. Persist
        repo.Update(user);
        await _uow.SaveChangesAsync();

        // 5. Return updated profile
        var response = new UpdateProfileResponse(
            Id: user.Id,
            FullName: user.FullName,
            Email: user.Email);

        return EndpointResponse<UpdateProfileResponse>.SuccessResponse(response, "Profile updated successfully.");
    }
}
