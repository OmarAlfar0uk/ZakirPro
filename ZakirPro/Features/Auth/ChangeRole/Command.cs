using MediatR;
using ZakirPro.Common.Models;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.Auth.ChangeRole;

public record Command(Guid UserId, UserRole NewRole) : IRequest<EndpointResponse<string>>;
