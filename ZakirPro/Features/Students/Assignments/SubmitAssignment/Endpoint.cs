using System;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ZakirPro.Common.Abstractions;

namespace ZakirPro.Features.Students.Assignments.SubmitAssignment;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/students/{studentId:guid}/assignments/{assignmentId:guid}/submit", async (
            Guid studentId,
            Guid assignmentId,
            IFormFile file,
            ISender sender) =>
        {
            var result = await sender.Send(new Command(studentId, assignmentId, file));
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .RequireAuthorization("StudentOnly")
        .WithTags("Student Assignments")
        .DisableAntiforgery();
    }
}
