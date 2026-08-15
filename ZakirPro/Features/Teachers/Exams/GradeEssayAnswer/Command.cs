using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Teachers.Exams.GradeEssayAnswer;

public record Command(
    Guid    TeacherId,
    Guid    ExamId,
    Guid    AttemptId,
    Guid    AnswerId,
    int     PointsAwarded,
    string? TeacherFeedback
) : IRequest<EndpointResponse<GradeResultDto>>;

public record GradeResultDto(
    Guid   AttemptId,
    string AttemptStatus,
    int    PendingEssayCount,
    int?   FinalScore,
    bool?  IsPassed);
