using MediatR;
using ZakirPro.Common.Abstractions;

namespace ZakirPro.Features.Students.Exams.SubmitAttempt;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/students/{studentId:guid}/attempts/{attemptId:guid}/submit",
            async (Guid studentId, Guid attemptId, SubmitRequest body, ISender sender) =>
            {
                var result = await sender.Send(new Command(studentId, attemptId, body.IsAutoSubmit));

                if (!result.Success)
                {
                    if (result.Message.Contains("not authorized"))
                        return Results.Forbid();
                    if (result.Message.Contains("not found"))
                        return Results.NotFound(result);
                    return Results.BadRequest(result);
                }

                return Results.Ok(result);
            })
            .WithName("SubmitAttempt")
            .WithTags("Students - Exams")
            .RequireAuthorization("StudentOnly");
    }
}

public record SubmitRequest(bool IsAutoSubmit = false);
