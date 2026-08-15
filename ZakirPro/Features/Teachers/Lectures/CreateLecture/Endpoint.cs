using System;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ZakirPro.Common.Abstractions;

namespace ZakirPro.Features.Teachers.Lectures.CreateLecture;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/teachers/lectures", async (CreateLectureRequest req, ISender sender) =>
        {
            var result = await sender.Send(new Command(req.Title, req.Description, req.GoogleDriveLink, req.TeacherSubjectStageId));
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .RequireAuthorization("AssistantOrAbove")
        .WithTags("Teacher Lectures");
    }
}

public record CreateLectureRequest(string Title, string? Description, string GoogleDriveLink, Guid TeacherSubjectStageId);
