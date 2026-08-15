using MediatR;
using ZakirPro.Common.Abstractions;

namespace ZakirPro.Features.Teachers.Exams.PublishExam;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPatch("/api/v1/teachers/{teacherId:guid}/exams/{examId:guid}/publish",
            async (Guid teacherId, Guid examId, ISender sender) =>
            {
                var result = await sender.Send(new Command(teacherId, examId));

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
            .WithName("PublishExam")
            .WithTags("Teachers - Exams")
            .RequireAuthorization("TeacherOrAbove");
    }
}
