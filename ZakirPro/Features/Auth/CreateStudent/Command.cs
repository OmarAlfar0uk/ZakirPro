using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Auth.CreateStudent;

public record Command(
    string FullName,
    string Email,
    Guid TeacherId,
    Guid SubjectId,
    Guid StageId)
    : IRequest<EndpointResponse<CreateStudentResponse>>;

public record CreateStudentResponse(Guid Id, string FullName, string Email, string ActivationCode);
