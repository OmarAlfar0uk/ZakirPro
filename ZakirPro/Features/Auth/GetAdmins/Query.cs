using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Auth.GetAdmins;

public record Query(int Page = 1, int PageSize = 20) : IRequest<EndpointResponse<PaginatedResult<AdminDto>>>;
public record AdminDto(Guid Id, string FullName, string Email, bool IsActive, DateTime CreatedAt);
