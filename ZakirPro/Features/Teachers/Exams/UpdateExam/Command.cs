using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Teachers.Exams.UpdateExam;

/// <summary>
/// Metadata-only patch — allowed even after publish.
/// Questions and choices cannot be edited once the exam is Published.
/// </summary>
public record Command(
    Guid     TeacherId,
    Guid     ExamId,
    string?  Title,
    string?  Description,
    DateTime? ScheduledEnd
) : IRequest<EndpointResponse<object>>;
