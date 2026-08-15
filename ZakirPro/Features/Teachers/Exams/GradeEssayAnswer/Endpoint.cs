using MediatR;
using ZakirPro.Common.Abstractions;

namespace ZakirPro.Features.Teachers.Exams.GradeEssayAnswer;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPut(
            "/api/v1/teachers/{teacherId:guid}/exams/{examId:guid}/attempts/{attemptId:guid}/essays/{answerId:guid}",
            async (Guid teacherId, Guid examId, Guid attemptId, Guid answerId,
                   GradeEssayRequest body, ISender sender) =>
            {
                var result = await sender.Send(new Command(
                    teacherId,
                    examId,
                    attemptId,
                    answerId,
                    body.PointsAwarded,
                    body.TeacherFeedback));

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
            .WithName("GradeEssayAnswer")
            .WithTags("Teachers - Exams")
            .RequireAuthorization("TeacherOrAbove");
    }
}

public record GradeEssayRequest(int PointsAwarded, string? TeacherFeedback);
