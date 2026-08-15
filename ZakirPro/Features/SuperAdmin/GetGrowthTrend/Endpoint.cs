using MediatR;
using ZakirPro.Common.Abstractions;

namespace ZakirPro.Features.SuperAdmin.GetGrowthTrend;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/superadmin/dashboard/growth",
            async (string? period, ISender sender) =>
            {
                var result = await sender.Send(new Query(period ?? "week"));
                return result.Success ? Results.Ok(result) : Results.BadRequest(result);
            })
            .WithName("GetGrowthTrend")
            .WithTags("SuperAdmin - Dashboard")
            .RequireAuthorization("SuperAdminOnly");
    }
}
