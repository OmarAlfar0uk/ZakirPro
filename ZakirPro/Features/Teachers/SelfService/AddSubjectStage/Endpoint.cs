using System;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ZakirPro.Common.Abstractions;

namespace ZakirPro.Features.Teachers.SelfService.AddSubjectStage;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/teachers/{teacherId:guid}/subject-stages", async (Guid teacherId, AddSubjectStageRequest req, ISender sender) =>
        {
            var result = await sender.Send(new Command(teacherId, req.SubjectId, req.StageId));
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .RequireAuthorization("TeacherOrAbove")
        .WithTags("Teacher Self Service");
    }
}

public record AddSubjectStageRequest(Guid SubjectId, Guid StageId);
