using System;
using System.Collections.Generic;
using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Teachers.SelfService.GetMySubjectStages;

public record Query(Guid TeacherId) : IRequest<EndpointResponse<List<TssDto>>>;

public record TssDto(Guid Id, Guid SubjectId, string SubjectName, Guid StageId, string StageName, int StudentCount, int LectureCount);
