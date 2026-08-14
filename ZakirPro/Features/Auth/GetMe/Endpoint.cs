using MediatR;
using ZakirPro.Common.Abstractions;

namespace ZakirPro.Features.Auth.GetMe;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/auth/me", async (ISender sender) =>
        {
            var result = await sender.Send(new Query());
            return result.Success ? Results.Ok(result) : Results.NotFound(result);
        })
        .WithName("GetMe")
        .WithTags("Auth")
        .RequireAuthorization("AuthenticatedUser");
    }
}
