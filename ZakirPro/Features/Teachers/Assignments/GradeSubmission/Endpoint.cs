using System;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ZakirPro.Common.Abstractions;

namespace ZakirPro.Features.Teachers.Assignments.GradeSubmission;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/v1/teachers/assignments/{assignmentId:guid}/submissions/{submissionId:guid}/grade", async (
            Guid assignmentId,
            Guid submissionId,
            GradeSubmissionRequest req,
            ISender sender) =>
        {
            var result = await sender.Send(new Command(
                assignmentId,
                submissionId,
                req.Score,
                req.Feedback
            ));
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .RequireAuthorization("AssistantOrAbove")
        .WithTags("Teacher Assignments");
    }
}

public record GradeSubmissionRequest(
    decimal Score,
    string? Feedback
);
