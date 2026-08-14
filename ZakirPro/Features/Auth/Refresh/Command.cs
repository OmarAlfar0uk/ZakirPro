using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Auth.Refresh;

public record Command(string AccessToken, string RefreshToken) : IRequest<EndpointResponse<RefreshResponse>>;
public record RefreshResponse(string AccessToken, string RefreshToken);
