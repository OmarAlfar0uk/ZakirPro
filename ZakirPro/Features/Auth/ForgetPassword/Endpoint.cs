using MediatR;
using ZakirPro.Common.Abstractions;

namespace ZakirPro.Features.Auth.ForgetPassword;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/forget-password",
            async (Command command, ISender sender) =>
            {
                var result = await sender.Send(command);
                return Results.Ok(result);   // Always 200 — enumeration-safe
            })
            .WithName("ForgetPassword")
            .WithTags("Auth")
            .AllowAnonymous();
    }
}
