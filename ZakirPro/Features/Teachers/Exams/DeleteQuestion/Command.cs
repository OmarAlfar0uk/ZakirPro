using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Teachers.Exams.DeleteQuestion;

public record Command(Guid TeacherId, Guid ExamId, Guid QuestionId)
    : IRequest<EndpointResponse<object>>;
