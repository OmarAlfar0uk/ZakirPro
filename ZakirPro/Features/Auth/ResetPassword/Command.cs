using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Auth.ResetPassword;

public record Command(string ResetToken, string NewPassword, string ConfirmPassword)
    : IRequest<EndpointResponse<object>>;
