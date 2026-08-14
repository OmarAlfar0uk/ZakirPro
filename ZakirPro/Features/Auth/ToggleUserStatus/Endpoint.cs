using MediatR;
using ZakirPro.Common.Abstractions;

namespace ZakirPro.Features.Auth.ToggleUserStatus;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/v1/auth/admin/users/{userId:guid}/toggle-status", async (Guid userId, ISender sender) =>
        {
            var result = await sender.Send(new Command(userId));
            if (!result.Success)
            {
                if (result.Message?.Contains("Forbidden", StringComparison.OrdinalIgnoreCase) == true ||
                    result.Message?.Contains("Cannot modify", StringComparison.OrdinalIgnoreCase) == true)
                    return Results.Forbid();

                return Results.BadRequest(result);
            }
            return Results.Ok(result);
        })
        .WithName("ToggleUserStatus")
        .WithTags("Auth")
        .RequireAuthorization("AdminOrAbove");
    }
}
