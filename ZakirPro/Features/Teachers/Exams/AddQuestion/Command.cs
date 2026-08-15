using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Teachers.Exams.AddQuestion;

public record Command(
    Guid   TeacherId,
    Guid   ExamId,
    string Text,
    string Type,
    int    Points,
    int?   OrderIndex,
    List<ChoiceRequest> Choices
) : IRequest<EndpointResponse<QuestionCreatedDto>>;

public record ChoiceRequest(string Text, bool IsCorrect, int OrderIndex);

public record QuestionCreatedDto(
    Guid   Id,
    string Text,
    string Type,
    int    Points,
    int    OrderIndex,
    List<ChoiceDto> Choices);

public record ChoiceDto(Guid Id, string Text, int OrderIndex);
