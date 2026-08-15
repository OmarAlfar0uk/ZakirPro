using MediatR;
using ZakirPro.Common.Abstractions;

namespace ZakirPro.Features.Teachers.Exams.UpdateQuestion;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/v1/teachers/{teacherId:guid}/exams/{examId:guid}/questions/{questionId:guid}",
            async (Guid teacherId, Guid examId, Guid questionId, UpdateQuestionRequest body, ISender sender) =>
            {
                var result = await sender.Send(new Command(
                    teacherId,
                    examId,
                    questionId,
                    body.Text,
                    body.Points,
                    body.Choices.Select(c => new ChoiceRequest(c.Text, c.IsCorrect, c.OrderIndex)).ToList()));

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
            .WithName("UpdateQuestion")
            .WithTags("Teachers - Exams")
            .RequireAuthorization("TeacherOrAbove");
    }
}

public record UpdateQuestionRequest(
    string  Text,
    int     Points,
    List<UpdateChoiceRequest> Choices);

public record UpdateChoiceRequest(string Text, bool IsCorrect, int OrderIndex);
