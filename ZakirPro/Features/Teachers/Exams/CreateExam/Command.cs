using MediatR;
using ZakirPro.Common.Models;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.Teachers.Exams.CreateExam;

public record Command(
    Guid   TeacherId,
    Guid   TeacherSubjectStageId,
    string Title,
    string? Description,
    DateTime ScheduledStart,
    DateTime ScheduledEnd,
    int    DurationMinutes,
    decimal PassThresholdPercent
) : IRequest<EndpointResponse<ExamCreatedDto>>;

public record ExamCreatedDto(
    Guid     Id,
    string   Title,
    string?  Description,
    Guid     TeacherSubjectStageId,
    DateTime ScheduledStart,
    DateTime ScheduledEnd,
    int      DurationMinutes,
    decimal  PassThresholdPercent,
    int      TotalPoints,
    string   Status,
    DateTime CreatedAt);
