using System;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ZakirPro.Common.Abstractions;

namespace ZakirPro.Features.Teachers.Assignments.DeleteAssignment;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/v1/teachers/assignments/{assignmentId:guid}", async (
            Guid assignmentId,
            ISender sender) =>
        {
            var result = await sender.Send(new Command(assignmentId));
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .RequireAuthorization("AssistantOrAbove")
        .WithTags("Teacher Assignments");
    }
}
