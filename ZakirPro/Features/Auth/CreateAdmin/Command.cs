using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Auth.CreateAdmin;

public record Command(string FullName, string Email, string Password)
    : IRequest<EndpointResponse<CreateAdminResponse>>;

public record CreateAdminResponse(Guid Id, string FullName, string Email);
