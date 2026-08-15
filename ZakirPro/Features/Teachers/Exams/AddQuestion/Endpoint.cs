using MediatR;
using ZakirPro.Common.Abstractions;

namespace ZakirPro.Features.Teachers.Exams.AddQuestion;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/teachers/{teacherId:guid}/exams/{examId:guid}/questions",
            async (Guid teacherId, Guid examId, AddQuestionRequest body, ISender sender) =>
            {
                var result = await sender.Send(new Command(
                    teacherId,
                    examId,
                    body.Text,
                    body.Type,
                    body.Points,
                    body.OrderIndex,
                    body.Choices.Select(c => new ChoiceRequest(c.Text, c.IsCorrect, c.OrderIndex)).ToList()));

                if (!result.Success)
                {
                    if (result.Message.Contains("not authorized") || result.Message.Contains("not belong"))
                        return Results.Forbid();
                    if (result.Message.Contains("not found"))
                        return Results.NotFound(result);
                    return Results.BadRequest(result);
                }

                return Results.Created(
                    $"/api/v1/teachers/{teacherId}/exams/{examId}/questions/{result.Data!.Id}",
                    result);
            })
            .WithName("AddQuestion")
            .WithTags("Teachers - Exams")
            .RequireAuthorization("TeacherOrAbove");
    }
}

public record AddQuestionRequest(
    string  Text,
    string  Type,
    int     Points,
    int?    OrderIndex,
    List<AddChoiceRequest> Choices);

public record AddChoiceRequest(string Text, bool IsCorrect, int OrderIndex);
