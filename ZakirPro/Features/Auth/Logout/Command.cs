using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Auth.Logout;

public record Command : IRequest<EndpointResponse<string>>;
