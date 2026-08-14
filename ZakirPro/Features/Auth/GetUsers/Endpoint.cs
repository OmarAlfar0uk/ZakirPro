using MediatR;
using ZakirPro.Common.Abstractions;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.Auth.GetUsers;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/auth/admin/users", async (
            ISender sender,
            int page = 1,
            int pageSize = 20,
            int? roleFilter = null,
            string? search = null) =>
        {
            UserRole? parsedRole = roleFilter.HasValue && Enum.IsDefined(typeof(UserRole), roleFilter.Value)
                ? (UserRole)roleFilter.Value
                : null;

            var result = await sender.Send(new Query(page, pageSize, parsedRole, search));
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithName("GetUsers")
        .WithTags("Auth")
        .RequireAuthorization("AdminOrAbove");
    }
}
