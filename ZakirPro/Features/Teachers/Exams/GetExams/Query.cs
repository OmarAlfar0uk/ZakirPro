using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Teachers.Exams.GetExams;

public record Query(
    Guid  TeacherId,
    Guid? TeacherSubjectStageId,
    string? StatusFilter
) : IRequest<EndpointResponse<List<TeacherExamDto>>>;

public record TeacherExamDto(
    Guid     Id,
    string   Title,
    string?  Description,
    Guid     TeacherSubjectStageId,
    string   SubjectName,
    string   StageName,
    DateTime ScheduledStart,
    DateTime ScheduledEnd,
    int      DurationMinutes,
    decimal  PassThresholdPercent,
    int      TotalPoints,
    string   Status,
    int      AttemptCount,
    DateTime CreatedAt);
