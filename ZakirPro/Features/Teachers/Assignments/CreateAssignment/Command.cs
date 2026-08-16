using System;
using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Teachers.Assignments.CreateAssignment;

public record Command(
    string   Title,
    string?  Description,
    DateTime DueDate,
    decimal  MaxScore,
    Guid     TeacherSubjectStageId
) : IRequest<EndpointResponse<AssignmentDto>>;

public record AssignmentDto(
    Guid     Id,
    string   Title,
    string?  Description,
    DateTime DueDate,
    decimal  MaxScore,
    Guid     TeacherSubjectStageId,
    DateTime CreatedAt
);
