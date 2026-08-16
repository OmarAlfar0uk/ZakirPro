using System;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ZakirPro.Common.Abstractions;

namespace ZakirPro.Features.Teachers.Assignments.CreateAssignment;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/teachers/assignments", async (CreateAssignmentRequest req, ISender sender) =>
        {
            var result = await sender.Send(new Command(
                req.Title,
                req.Description,
                req.DueDate,
                req.MaxScore ?? 100m,
                req.TeacherSubjectStageId
            ));
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .RequireAuthorization("AssistantOrAbove")
        .WithTags("Teacher Assignments");
    }
}

public record CreateAssignmentRequest(
    string   Title,
    string?  Description,
    DateTime DueDate,
    Guid     TeacherSubjectStageId,
    decimal? MaxScore = 100m
);
