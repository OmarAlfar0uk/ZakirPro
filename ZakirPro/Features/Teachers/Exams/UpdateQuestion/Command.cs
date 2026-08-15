using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Teachers.Exams.UpdateQuestion;

public record Command(
    Guid   TeacherId,
    Guid   ExamId,
    Guid   QuestionId,
    string Text,
    int    Points,
    List<ChoiceRequest> Choices
) : IRequest<EndpointResponse<object>>;

public record ChoiceRequest(string Text, bool IsCorrect, int OrderIndex);
