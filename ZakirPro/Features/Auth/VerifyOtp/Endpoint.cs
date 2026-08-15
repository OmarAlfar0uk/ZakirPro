using MediatR;
using ZakirPro.Common.Abstractions;

namespace ZakirPro.Features.Auth.VerifyOtp;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/auth/verify-otp",
            async (Command command, ISender sender) =>
            {
                var result = await sender.Send(command);
                return result.Success ? Results.Ok(result) : Results.BadRequest(result);
            })
            .WithName("VerifyOtp")
            .WithTags("Auth")
            .AllowAnonymous();
    }
}
