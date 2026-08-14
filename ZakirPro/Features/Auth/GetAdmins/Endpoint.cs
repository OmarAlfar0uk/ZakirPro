using MediatR;
using ZakirPro.Common.Abstractions;

namespace ZakirPro.Features.Auth.GetAdmins;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/auth/admin/admins", async (
            ISender sender,
            int page = 1,
            int pageSize = 20) =>
        {
            var result = await sender.Send(new Query(page, pageSize));
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithName("GetAdmins")
        .WithTags("Auth")
        .RequireAuthorization("SuperAdminOnly");
    }
}
