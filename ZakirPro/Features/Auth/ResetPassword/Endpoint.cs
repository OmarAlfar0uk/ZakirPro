using MediatR;
using ZakirPro.Common.Abstractions;

namespace ZakirPro.Features.Auth.ResetPassword;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/reset-password",
            async (Command command, ISender sender) =>
            {
                var result = await sender.Send(command);
                return result.Success ? Results.Ok(result) : Results.BadRequest(result);
            })
            .WithName("ResetPassword")
            .WithTags("Auth")
            .AllowAnonymous();
    }
}
