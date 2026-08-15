using MediatR;
using ZakirPro.Common.Abstractions;

namespace ZakirPro.Features.Teachers.Exams.GetExams;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/teachers/{teacherId:guid}/exams",
            async (
                Guid    teacherId,
                Guid?   teacherSubjectStageId,
                string? status,
                ISender sender) =>
            {
                var result = await sender.Send(new Query(teacherId, teacherSubjectStageId, status));

                if (!result.Success)
                {
                    if (result.Message.Contains("not authorized"))
                        return Results.Forbid();
                    return Results.BadRequest(result);
                }

                return Results.Ok(result);
            })
            .WithName("GetTeacherExams")
            .WithTags("Teachers - Exams")
            .RequireAuthorization("TeacherOrAbove");
    }
}
