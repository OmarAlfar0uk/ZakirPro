using MediatR;
using Microsoft.AspNetCore.Mvc;
using ZakirPro.Common.Abstractions;

namespace ZakirPro.Features.SuperAdmin.GetRecentActivity;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/superadmin/dashboard/recent-activity",
            async ([FromServices] ISender sender, int limit = 20) =>
            {
                var result = await sender.Send(new Query(limit));
                return result.Success ? Results.Ok(result) : Results.BadRequest(result);
            })
            .WithName("GetRecentActivity")
            .WithTags("SuperAdmin - Dashboard")
            .RequireAuthorization("SuperAdminOnly");
    }
}
