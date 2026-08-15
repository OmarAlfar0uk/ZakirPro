using MediatR;
using ZakirPro.Common.Abstractions;

namespace ZakirPro.Features.Students.Exams.GetQuestions;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/v1/students/{studentId:guid}/attempts/{attemptId:guid}/questions",
            async (Guid studentId, Guid attemptId, ISender sender) =>
            {
                var result = await sender.Send(new Query(studentId, attemptId));

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
            .WithName("GetAttemptQuestions")
            .WithTags("Students - Exams")
            .RequireAuthorization("StudentOnly");
    }
}
