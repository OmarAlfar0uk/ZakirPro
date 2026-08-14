using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Auth.ToggleUserStatus;

public record Command(Guid UserId) : IRequest<EndpointResponse<string>>;
