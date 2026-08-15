using MediatR;
using ZakirPro.Common.Abstractions;

namespace ZakirPro.Features.Students.Exams.StartAttempt;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/students/{studentId:guid}/exams/{examId:guid}/attempts",
            async (Guid studentId, Guid examId, ISender sender) =>
            {
                var result = await sender.Send(new Command(studentId, examId));

                if (!result.Success)
                {
                    if (result.Message.Contains("not authorized") || result.Message.Contains("not enrolled"))
                        return Results.Forbid();
                    if (result.Message.Contains("window has closed") ||
                        result.Message.Contains("not started yet"))
                        return Results.Forbid();
                    if (result.Message.Contains("not found"))
                        return Results.NotFound(result);
                    if (result.Message.Contains("already attempted"))
                        return Results.Conflict(result);
                    return Results.BadRequest(result);
                }

                return Results.Created(
                    $"/api/v1/students/{studentId}/attempts/{result.Data!.AttemptId}",
                    result);
            })
            .WithName("StartExamAttempt")
            .WithTags("Students - Exams")
            .RequireAuthorization("StudentOnly");
    }
}
