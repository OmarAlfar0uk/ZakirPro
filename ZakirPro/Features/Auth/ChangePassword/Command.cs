using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Auth.ChangePassword;

public record Command(string CurrentPassword, string NewPassword, string ConfirmNewPassword)
    : IRequest<EndpointResponse<string>>;
