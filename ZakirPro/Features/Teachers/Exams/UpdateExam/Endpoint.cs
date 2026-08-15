using MediatR;
using ZakirPro.Common.Abstractions;

namespace ZakirPro.Features.Teachers.Exams.UpdateExam;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPatch("/api/v1/teachers/{teacherId:guid}/exams/{examId:guid}",
            async (Guid teacherId, Guid examId, UpdateExamRequest body, ISender sender) =>
            {
                var result = await sender.Send(new Command(
                    teacherId,
                    examId,
                    body.Title,
                    body.Description,
                    body.ScheduledEnd));

                if (!result.Success)
                {
                    if (result.Message.Contains("not authorized") || result.Message.Contains("not belong"))
                        return Results.Forbid();
                    if (result.Message.Contains("not found"))
                        return Results.NotFound(result);
                    return Results.BadRequest(result);
                }

                return Results.Ok(result);
            })
            .WithName("UpdateExam")
            .WithTags("Teachers - Exams")
            .RequireAuthorization("TeacherOrAbove");
    }
}

public record UpdateExamRequest(
    string?  Title,
    string?  Description,
    DateTime? ScheduledEnd);
