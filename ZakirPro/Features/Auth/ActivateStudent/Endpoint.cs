using MediatR;
using ZakirPro.Common.Abstractions;

namespace ZakirPro.Features.Auth.ActivateStudent;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/activate", async (Command command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .WithName("ActivateStudent")
        .WithTags("Auth")
        .AllowAnonymous();
    }
}
