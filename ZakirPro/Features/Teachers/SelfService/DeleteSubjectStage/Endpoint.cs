using System;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ZakirPro.Common.Abstractions;

namespace ZakirPro.Features.Teachers.SelfService.DeleteSubjectStage;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/v1/teachers/{teacherId:guid}/subject-stages/{tssId:guid}", async (Guid teacherId, Guid tssId, ISender sender) =>
        {
            var result = await sender.Send(new Command(teacherId, tssId));
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .RequireAuthorization("TeacherOrAbove")
        .WithTags("Teacher Self Service");
    }
}
