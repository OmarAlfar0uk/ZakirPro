using MediatR;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Teachers.Exams.CreateExam;

public class Endpoint : IEndpointDefinition
{
    public void DefineEndpoints(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/teachers/{teacherId:guid}/exams",
            async (Guid teacherId, CreateExamRequest body, ISender sender) =>
            {
                var result = await sender.Send(new Command(
                    teacherId,
                    body.TeacherSubjectStageId,
                    body.Title,
                    body.Description,
                    body.ScheduledStart,
                    body.ScheduledEnd,
                    body.DurationMinutes,
                    body.PassThresholdPercent));

                if (!result.Success)
                {
                    if (result.Message.Contains("not authorized") || result.Message.Contains("Access denied"))
                        return Results.Forbid();
                    if (result.Message.Contains("not found"))
                        return Results.NotFound(result);
                    return Results.BadRequest(result);
                }

                return Results.Created($"/api/v1/teachers/{teacherId}/exams/{result.Data!.Id}", result);
            })
            .WithName("CreateExam")
            .WithTags("Teachers - Exams")
            .RequireAuthorization("TeacherOrAbove");
    }
}

public record CreateExamRequest(
    Guid    TeacherSubjectStageId,
    string  Title,
    string? Description,
    DateTime ScheduledStart,
    DateTime ScheduledEnd,
    int     DurationMinutes,
    decimal PassThresholdPercent);
