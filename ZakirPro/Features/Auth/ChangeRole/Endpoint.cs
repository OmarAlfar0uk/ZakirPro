using MediatR;
using ZakirPro.Common.Abstractions;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.Auth.ChangeRole;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/v1/auth/admin/change-role", async (Command command, ISender sender) =>
        {
            var result = await sender.Send(command);
            if (!result.Success)
            {
                if (result.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
                    return Results.NotFound(result);

                if (result.Message.Contains("Cannot change", StringComparison.OrdinalIgnoreCase) ||
                    result.Message.Contains("Access denied", StringComparison.OrdinalIgnoreCase))
                    return Results.Forbid();

                return Results.BadRequest(result);
            }
            return Results.Ok(result);
        })
        .WithName("ChangeRole")
        .WithTags("Auth")
        .RequireAuthorization("SuperAdminOnly");
    }
}
