using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Auth.DeleteUser;

public record Command(Guid UserId) : IRequest<EndpointResponse<string>>;
