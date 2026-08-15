using System;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ZakirPro.Common.Abstractions;

namespace ZakirPro.Features.Teachers.SelfService.GetMySubjectStages;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/teachers/{teacherId:guid}/subject-stages", async (Guid teacherId, ISender sender) =>
        {
            var result = await sender.Send(new Query(teacherId));
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .RequireAuthorization("TeacherOrAbove")
        .WithTags("Teacher Self Service");
    }
}
