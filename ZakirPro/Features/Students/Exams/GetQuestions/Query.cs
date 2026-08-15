using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Students.Exams.GetQuestions;

public record Query(Guid StudentId, Guid AttemptId)
    : IRequest<EndpointResponse<ActiveExamDto>>;

public record ActiveExamDto(
    Guid     AttemptId,
    DateTime DeadlineAt,
    int      QuestionCount,
    List<QuestionDto> Questions);

public record QuestionDto(
    Guid   QuestionId,
    int    OrderIndex,
    string Text,
    string Type,
    int    Points,
    List<ChoiceDto> Choices,
    Guid?  SavedSelectedChoiceId,
    string? SavedEssayResponse,
    bool   IsMarkedForReview);

/// <summary>
/// Safe choice DTO — deliberately omits IsCorrect.
/// IsCorrect is only ever included in the review endpoint (post-submission).
/// </summary>
public record ChoiceDto(Guid ChoiceId, string Text, int OrderIndex);
