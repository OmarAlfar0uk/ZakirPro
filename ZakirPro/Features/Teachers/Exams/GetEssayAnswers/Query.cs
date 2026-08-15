using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Teachers.Exams.GetEssayAnswers;

public record Query(Guid TeacherId, Guid ExamId, Guid AttemptId)
    : IRequest<EndpointResponse<List<EssayAnswerDto>>>;

public record EssayAnswerDto(
    Guid    AnswerId,
    Guid    QuestionId,
    string  QuestionText,
    int     QuestionPoints,
    string? EssayResponse,
    int?    PointsAwarded);
