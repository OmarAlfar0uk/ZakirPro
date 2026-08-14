using MediatR;
using ZakirPro.Common.Models;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.Auth.GetUsers;

public record Query(int Page = 1, int PageSize = 20, UserRole? RoleFilter = null, string? Search = null)
    : IRequest<EndpointResponse<PaginatedResult<UserListDto>>>;

public record UserListDto(Guid Id, string FullName, string Email, string Role, bool IsActive, DateTime CreatedAt);
