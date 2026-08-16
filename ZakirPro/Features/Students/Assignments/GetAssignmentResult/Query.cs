using System;
using MediatR;
using ZakirPro.Common.Models;
using ZakirPro.Features.Students.Assignments.SubmitAssignment;

namespace ZakirPro.Features.Students.Assignments.GetAssignmentResult;

public record Query(
    Guid StudentId,
    Guid AssignmentId
) : IRequest<EndpointResponse<StudentSubmissionDto>>;
