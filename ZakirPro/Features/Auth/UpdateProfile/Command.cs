using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Auth.UpdateProfile;

public record Command(string FullName) : IRequest<EndpointResponse<UpdateProfileResponse>>;
public record UpdateProfileResponse(Guid Id, string FullName, string Email);
