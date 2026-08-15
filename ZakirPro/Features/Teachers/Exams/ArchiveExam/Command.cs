using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Teachers.Exams.ArchiveExam;

public record Command(Guid TeacherId, Guid ExamId) : IRequest<EndpointResponse<object>>;
