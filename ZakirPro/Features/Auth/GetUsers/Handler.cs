using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Domain.Entities;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.Auth.GetUsers;

public class Handler : IRequestHandler<Query, EndpointResponse<PaginatedResult<UserListDto>>>
{
    private readonly IUnitOfWork _uow;

    public Handler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<EndpointResponse<PaginatedResult<UserListDto>>> Handle(Query request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : request.PageSize;

        var repo = _uow.GetRepository<User>();

        // 1. Base query: always exclude SuperAdmin from this list
        var query = repo.Query()
            .Where(u => u.Role != UserRole.SuperAdmin);

        // 2. Apply optional role filter
        if (request.RoleFilter.HasValue)
            query = query.Where(u => u.Role == request.RoleFilter.Value);

        // 3. Apply optional search (FullName or Email, case-insensitive)
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(u =>
                u.FullName.ToLower().Contains(search) ||
                u.Email.ToLower().Contains(search));
        }

        // 4. Total count for pagination metadata
        var totalCount = await query.CountAsync(cancellationToken);

        // 5. Paginate and project to DTO
        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserListDto(
                u.Id,
                u.FullName,
                u.Email,
                u.Role.ToString(),
                u.IsActive,
                u.CreatedAt))
            .ToListAsync(cancellationToken);

        var result = new PaginatedResult<UserListDto>(items, totalCount, page, pageSize);

        return EndpointResponse<PaginatedResult<UserListDto>>.SuccessResponse(result, "Users retrieved successfully.");
    }
}
