using MediatR;
using ZakirPro.Common.Abstractions;

namespace ZakirPro.Features.SuperAdmin.GetDashboardSummary;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/superadmin/dashboard/summary",
            async (ISender sender) =>
            {
                var result = await sender.Send(new Query());
                return result.Success ? Results.Ok(result) : Results.BadRequest(result);
            })
            .WithName("GetDashboardSummary")
            .WithTags("SuperAdmin - Dashboard")
            .RequireAuthorization("SuperAdminOnly");
    }
}
