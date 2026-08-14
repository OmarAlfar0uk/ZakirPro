using MediatR;
using ZakirPro.Common.Abstractions;

namespace ZakirPro.Features.Auth.UpdateProfile;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/v1/auth/update-profile", async (Command command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithName("UpdateProfile")
        .WithTags("Auth")
        .RequireAuthorization("AuthenticatedUser");
    }
}
