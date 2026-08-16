using System;
using MediatR;
using ZakirPro.Common.Models;
using ZakirPro.Features.Teachers.Assignments.CreateAssignment;

namespace ZakirPro.Features.Teachers.Assignments.UpdateAssignment;

public record Command(
    Guid     AssignmentId,
    string   Title,
    string?  Description,
    DateTime DueDate,
    decimal? MaxScore
) : IRequest<EndpointResponse<AssignmentDto>>;
