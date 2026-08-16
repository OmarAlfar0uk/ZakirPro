using System;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ZakirPro.Common.Abstractions;

namespace ZakirPro.Features.Teachers.Assignments.GetAssignmentSubmissions;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/teachers/assignments/{assignmentId:guid}/submissions", async (
            Guid assignmentId,
            ISender sender) =>
        {
            var result = await sender.Send(new Query(assignmentId));
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .RequireAuthorization("AssistantOrAbove")
        .WithTags("Teacher Assignments");
    }
}
