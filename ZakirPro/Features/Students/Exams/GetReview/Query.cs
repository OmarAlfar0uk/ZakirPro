using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Students.Exams.GetReview;

public record Query(Guid StudentId, Guid AttemptId)
    : IRequest<EndpointResponse<ReviewDto>>;

public record ReviewDto(
    Guid   AttemptId,
    string AttemptStatus,
    List<ReviewQuestionDto> Questions);

public record ReviewQuestionDto(
    Guid   QuestionId,
    int    OrderIndex,
    string Text,
    string Type,
    int    Points,
    List<ReviewChoiceDto> Choices,      // Includes IsCorrect — exam is over
    Guid?  SelectedChoiceId,
    string? EssayResponse,
    bool?  IsCorrect,
    int?   PointsAwarded,
    /// <summary>
    /// For Essay answers — indicates the color-coding to apply:
    /// "full"    = PointsAwarded == Question.Points  → green
    /// "none"    = PointsAwarded == 0                → red
    /// "partial" = 0 < PointsAwarded < Question.Points → amber
    /// "pending" = PointsAwarded is null (not yet graded)
    /// null      = not applicable (choice question — use IsCorrect for color)
    /// </summary>
    string? EssayGradeLabel);

public record ReviewChoiceDto(
    Guid   ChoiceId,
    string Text,
    int    OrderIndex,
    bool   IsCorrect);   // IsCorrect revealed post-submission
