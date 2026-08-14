using MediatR;
using ZakirPro.Common.Abstractions;

namespace ZakirPro.Features.Auth.Logout;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/logout", async (ISender sender) =>
        {
            var result = await sender.Send(new Command());
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithName("Logout")
        .WithTags("Auth")
        .RequireAuthorization("AuthenticatedUser");
    }
}
