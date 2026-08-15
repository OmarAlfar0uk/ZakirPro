using System;
using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Teachers.SelfService.GetMyStudents;

public record Query(Guid TeacherId, Guid? TeacherSubjectStageId, string? Search, int Page = 1, int PageSize = 20) : IRequest<EndpointResponse<PaginatedResult<StudentDto>>>;

public record StudentDto(Guid Id, string FullName, string Email, bool IsActive, Guid TeacherSubjectStageId, string SubjectName, string StageName);
