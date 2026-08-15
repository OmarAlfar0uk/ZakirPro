using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Students.Exams.StartAttempt;

public record Command(Guid StudentId, Guid ExamId)
    : IRequest<EndpointResponse<AttemptStartedDto>>;

public record AttemptStartedDto(
    Guid     AttemptId,
    DateTime StartedAt,
    DateTime DeadlineAt,
    int      QuestionCount);
