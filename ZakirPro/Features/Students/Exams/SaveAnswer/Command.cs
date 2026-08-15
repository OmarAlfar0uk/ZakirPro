using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Students.Exams.SaveAnswer;

public record Command(
    Guid   StudentId,
    Guid   AttemptId,
    Guid   QuestionId,
    Guid?  SelectedChoiceId,
    string? EssayResponse,
    bool   IsMarkedForReview
) : IRequest<EndpointResponse<SavedAnswerDto>>;

public record SavedAnswerDto(
    Guid   QuestionId,
    Guid?  SelectedChoiceId,
    string? EssayResponse,
    bool   IsMarkedForReview,
    DateTime SavedAt);
