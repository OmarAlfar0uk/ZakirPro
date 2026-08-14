using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Auth.GetMe;

public record Query : IRequest<EndpointResponse<MeResponse>>;
public record MeResponse(Guid Id, string FullName, string Email, string Role, bool IsActive, DateTime CreatedAt);
