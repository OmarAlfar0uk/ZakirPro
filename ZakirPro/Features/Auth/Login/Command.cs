using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Auth.Login;

public record Command(string Email, string Password, bool RememberMe = false)
    : IRequest<EndpointResponse<LoginResponse>>;

public record LoginResponse(string AccessToken, string RefreshToken, UserDto User);

public record UserDto(Guid Id, string FullName, string Email, string Role);
