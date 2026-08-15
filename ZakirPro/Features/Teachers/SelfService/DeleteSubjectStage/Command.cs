using System;
using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Teachers.SelfService.DeleteSubjectStage;

public record Command(Guid TeacherId, Guid TssId) : IRequest<EndpointResponse<bool>>;
