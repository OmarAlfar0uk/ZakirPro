using System;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ZakirPro.Common.Abstractions;

namespace ZakirPro.Features.Students.Attendance.GetStudentAttendance;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/students/{studentId:guid}/attendance", async (Guid studentId, Guid? teacherSubjectStageId, ISender sender) =>
        {
            var result = await sender.Send(new Query(studentId, teacherSubjectStageId));
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .RequireAuthorization("StudentOnly")
        .WithTags("Student Attendance");
    }
}
