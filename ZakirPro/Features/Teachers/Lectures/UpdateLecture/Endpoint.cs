using System;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ZakirPro.Common.Abstractions;

namespace ZakirPro.Features.Teachers.Lectures.UpdateLecture;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/v1/teachers/lectures/{lectureId:guid}", async (Guid lectureId, UpdateLectureRequest req, ISender sender) =>
        {
            var result = await sender.Send(new Command(lectureId, req.Title, req.Description, req.GoogleDriveLink));
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .RequireAuthorization("AssistantOrAbove")
        .WithTags("Teacher Lectures");
    }
}

public record UpdateLectureRequest(string Title, string? Description, string GoogleDriveLink);
