using System;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ZakirPro.Common.Abstractions;

namespace ZakirPro.Features.Students.Assignments.GetAssignmentResult;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/students/{studentId:guid}/assignments/{assignmentId:guid}/result", async (
            Guid studentId,
            Guid assignmentId,
            ISender sender) =>
        {
            var result = await sender.Send(new Query(studentId, assignmentId));
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .RequireAuthorization("StudentOnly")
        .WithTags("Student Assignments");
    }
}
