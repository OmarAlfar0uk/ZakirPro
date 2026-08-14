using MediatR;
using ZakirPro.Common.Abstractions;

namespace ZakirPro.Features.Auth.DeleteUser;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/v1/auth/admin/users/{userId:guid}", async (Guid userId, ISender sender) =>
        {
            var result = await sender.Send(new Command(userId));
            if (!result.Success)
            {
                if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return Results.NotFound(result);

                if (result.Message.Contains("Cannot", StringComparison.OrdinalIgnoreCase) ||
                    result.Message.Contains("cannot", StringComparison.OrdinalIgnoreCase) ||
                    result.Message.Contains("Access denied", StringComparison.OrdinalIgnoreCase) ||
                    result.Message.Contains("Only SuperAdmin", StringComparison.OrdinalIgnoreCase))
                    return Results.Forbid();

                return Results.BadRequest(result);
            }
            return Results.Ok(result);
        })
        .WithName("DeleteUser")
        .WithTags("Auth")
        .RequireAuthorization("AdminOrAbove");
    }
}
