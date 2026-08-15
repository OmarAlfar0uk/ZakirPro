using MediatR;
using ZakirPro.Common.Abstractions;

namespace ZakirPro.Features.Students.Exams.SaveAnswer;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPut(
            "/api/v1/students/{studentId:guid}/attempts/{attemptId:guid}/answers/{questionId:guid}",
            async (Guid studentId, Guid attemptId, Guid questionId,
                   SaveAnswerRequest body, ISender sender) =>
            {
                var result = await sender.Send(new Command(
                    studentId,
                    attemptId,
                    questionId,
                    body.SelectedChoiceId,
                    body.EssayResponse,
                    body.IsMarkedForReview));

                if (!result.Success)
                {
                    if (result.Message.Contains("not authorized"))
                        return Results.Forbid();
                    if (result.Message.Contains("not found"))
                        return Results.NotFound(result);
                    // 409 when time expired — client should trigger submit
                    if (result.Message.Contains("time has expired"))
                        return Results.Conflict(result);
                    return Results.BadRequest(result);
                }

                return Results.Ok(result);
            })
            .WithName("SaveAnswer")
            .WithTags("Students - Exams")
            .RequireAuthorization("StudentOnly");
    }
}

public record SaveAnswerRequest(
    Guid?   SelectedChoiceId,
    string? EssayResponse,
    bool    IsMarkedForReview);
