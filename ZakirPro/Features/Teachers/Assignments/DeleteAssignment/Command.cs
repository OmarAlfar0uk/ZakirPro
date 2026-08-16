using System;
using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Teachers.Assignments.DeleteAssignment;

public record Command(Guid AssignmentId) : IRequest<EndpointResponse<bool>>;
