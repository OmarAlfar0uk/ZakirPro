using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Auth.ForgetPassword;

public record Command(string Email) : IRequest<EndpointResponse<object>>;
