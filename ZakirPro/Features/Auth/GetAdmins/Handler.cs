using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Domain.Entities;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.Auth.GetAdmins;

public class Handler : IRequestHandler<Query, EndpointResponse<PaginatedResult<AdminDto>>>
{
    private readonly IUnitOfWork _uow;

    public Handler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<EndpointResponse<PaginatedResult<AdminDto>>> Handle(Query request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : request.PageSize;

        // 1. Query only Admin-role users (not SuperAdmin)
        var repo = _uow.GetRepository<User>();
        var query = repo.Query()
            .Where(u => u.Role == UserRole.Admin);

        // 2. Total count for pagination
        var totalCount = await query.CountAsync(cancellationToken);

        // 3. Apply pagination and project to DTO
        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new AdminDto(
                u.Id,
                u.FullName,
                u.Email,
                u.IsActive,
                u.CreatedAt))
            .ToListAsync(cancellationToken);

        var result = new PaginatedResult<AdminDto>(items, totalCount, page, pageSize);

        return EndpointResponse<PaginatedResult<AdminDto>>.SuccessResponse(result, "Admins retrieved successfully.");
    }
}
