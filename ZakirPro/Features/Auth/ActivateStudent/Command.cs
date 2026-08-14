using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Auth.ActivateStudent;

public record Command(string ActivationCode, string NewPassword, string ConfirmPassword)
    : IRequest<EndpointResponse<string>>;
