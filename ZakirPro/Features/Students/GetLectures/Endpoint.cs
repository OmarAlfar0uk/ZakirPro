using MediatR;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Students.GetLectures;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/students/{studentId:guid}/lectures",
            async (
                Guid studentId,
                Guid teacherId,
                Guid subjectId,
                Guid stageId,
                ISender sender) =>
            {
                var result = await sender.Send(new Query(studentId, teacherId, subjectId, stageId));

                if (!result.Success)
                {
                    if (result.Message.Contains("not authorized") || result.Message.Contains("Access denied")
                        || result.Message.Contains("not enrolled"))
                        return Results.Forbid();

                    if (result.Message.Contains("not found"))
                        return Results.NotFound(result);

                    return Results.BadRequest(result);
                }

                return Results.Ok(result);
            })
            .WithName("GetStudentLectures")
            .WithTags("Students")
            .RequireAuthorization("StudentOnly");
    }
}
