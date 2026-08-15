using System;
using System.Collections.Generic;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ZakirPro.Common.Abstractions;

namespace ZakirPro.Features.Teachers.Attendance.MarkAttendance;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/teachers/lectures/{lectureId:guid}/attendance", async (Guid lectureId, MarkAttendanceRequest req, ISender sender) =>
        {
            var result = await sender.Send(new Command(lectureId, req.Entries));
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .RequireAuthorization("AssistantOrAbove")
        .WithTags("Teacher Attendance");
    }
}

public record MarkAttendanceRequest(List<AttendanceEntry> Entries);
