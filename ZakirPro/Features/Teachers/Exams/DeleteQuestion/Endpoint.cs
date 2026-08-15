using MediatR;
using ZakirPro.Common.Abstractions;

namespace ZakirPro.Features.Teachers.Exams.DeleteQuestion;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/v1/teachers/{teacherId:guid}/exams/{examId:guid}/questions/{questionId:guid}",
            async (Guid teacherId, Guid examId, Guid questionId, ISender sender) =>
            {
                var result = await sender.Send(new Command(teacherId, examId, questionId));

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
            .WithName("DeleteQuestion")
            .WithTags("Teachers - Exams")
            .RequireAuthorization("TeacherOrAbove");
    }
}
