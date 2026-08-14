using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Domain.Entities;

namespace ZakirPro.Features.Auth.GetMe;

public class Handler : IRequestHandler<Query, EndpointResponse<MeResponse>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public Handler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<EndpointResponse<MeResponse>> Handle(Query request, CancellationToken cancellationToken)
    {
        // 1. Get UserId from ICurrentUserService
        var userId = _currentUser.UserId;
        if (userId is null)
            return EndpointResponse<MeResponse>.ErrorResponse("Unauthenticated.");

        // 2. Find user by ID using the default (soft-delete-aware) filter
        var repo = _uow.GetRepository<User>();
        var user = await repo.Query()
            .FirstOrDefaultAsync(u => u.Id == userId.Value, cancellationToken);

        // 3. Not found
        if (user is null)
            return EndpointResponse<MeResponse>.NotFoundResponse("User not found.");

        // 4. Map to MeResponse
        var response = new MeResponse(
            Id: user.Id,
            FullName: user.FullName,
            Email: user.Email,
            Role: user.Role.ToString(),
            IsActive: user.IsActive,
            CreatedAt: user.CreatedAt);

        // 5. Return success
        return EndpointResponse<MeResponse>.SuccessResponse(response, "Profile retrieved successfully.");
    }
}
