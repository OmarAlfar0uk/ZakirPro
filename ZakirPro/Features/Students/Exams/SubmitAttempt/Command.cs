using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Students.Exams.SubmitAttempt;

public record Command(
    Guid StudentId,
    Guid AttemptId,
    bool IsAutoSubmit
) : IRequest<EndpointResponse<SubmissionResultDto>>;

public record SubmissionResultDto(
    Guid   AttemptId,
    string AttemptStatus,
    int?   AutoGradedScore,
    int    MaxScore,
    string GradingStatus,  // "Complete" | "PendingEssayGrading"
    int?   FinalScore,
    bool?  IsPassed,
    int?   TimeTakenSeconds);
