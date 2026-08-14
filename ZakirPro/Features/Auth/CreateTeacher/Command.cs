using MediatR;
using ZakirPro.Common.Models;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.Auth.CreateTeacher;

public record AssignmentDto(Guid SubjectId, Guid StageId);

public record Command(
    string FullName,
    string Email,
    string Password,
    Gender Gender,
    List<AssignmentDto> Assignments)
    : IRequest<EndpointResponse<CreateTeacherResponse>>;

public record CreateTeacherResponse(
    Guid Id,
    string FullName,
    string Email,
    string Gender,
    List<AssignmentDto> Assignments);
