using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Teachers.Exams.GetAttempts;

public record Query(Guid TeacherId, Guid ExamId)
    : IRequest<EndpointResponse<List<AttemptSummaryDto>>>;

public record AttemptSummaryDto(
    Guid     AttemptId,
    Guid     StudentId,
    string   StudentFullName,
    string   AttemptStatus,
    int?     FinalScore,
    int      MaxScore,
    bool?    IsPassed,
    DateTime? SubmittedAt);
