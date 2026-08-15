using System;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ZakirPro.Common.Abstractions;

namespace ZakirPro.Features.Teachers.SelfService.GetMyStudents;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/teachers/{teacherId:guid}/students", async (
            Guid teacherId,
            Guid? teacherSubjectStageId,
            string? search,
            ISender sender,
            int page = 1,
            int pageSize = 20) =>
        {
            var result = await sender.Send(new Query(teacherId, teacherSubjectStageId, search, page, pageSize));
            return result.Success ? Results.Ok(result) : Results.BadRequest(result);
        })
        .RequireAuthorization("TeacherOrAbove")
        .WithTags("Teacher Self Service");
    }
}
