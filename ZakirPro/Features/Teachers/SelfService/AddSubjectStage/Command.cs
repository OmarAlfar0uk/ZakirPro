using System;
using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Teachers.SelfService.AddSubjectStage;

public record Command(Guid TeacherId, Guid SubjectId, Guid StageId) : IRequest<EndpointResponse<TssDto>>;

public record TssDto(Guid Id, Guid SubjectId, string SubjectName, Guid StageId, string StageName, int StudentCount, int LectureCount);
