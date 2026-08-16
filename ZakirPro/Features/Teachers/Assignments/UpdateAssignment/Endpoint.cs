using System;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ZakirPro.Common.Abstractions;

namespace ZakirPro.Features.Teachers.Assignments.UpdateAssignment;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/v1/teachers/assignments/{assignmentId:guid}", async (
            Guid assignmentId,
            UpdateAssignmentRequest req,
            ISender sender) =>
        {
            var result = await sender.Send(new Command(
                assignmentId,
                req.Title,
                req.Description,
                req.DueDate,
                req.MaxScore
            ));
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .RequireAuthorization("AssistantOrAbove")
        .WithTags("Teacher Assignments");
    }
}

public record UpdateAssignmentRequest(
    string   Title,
    string?  Description,
    DateTime DueDate,
    decimal? MaxScore
);
