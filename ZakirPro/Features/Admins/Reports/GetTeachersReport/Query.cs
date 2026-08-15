using System;
using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Admins.Reports.GetTeachersReport;

public record Query(int Page = 1, int PageSize = 20) : IRequest<EndpointResponse<PaginatedResult<TeacherReportDto>>>;

public record TeacherReportDto(Guid TeacherId, string FullName, int ActiveStudentCount, int LectureCount, int ExamCount, decimal AttendanceRate, decimal AssignmentSubmissionRate);
