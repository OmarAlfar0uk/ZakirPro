using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ZakirPro.Common.Abstractions;

namespace ZakirPro.Features.Admins.Reports.GetTeachersReport;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/admin/reports/teachers", async ([AsParameters] Query query, ISender sender) =>
        {
            var result = await sender.Send(query);
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .RequireAuthorization("AdminOrAbove")
        .WithTags("Admin Reports");
    }
}
