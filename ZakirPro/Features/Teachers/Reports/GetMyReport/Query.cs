using System;
using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Teachers.Reports.GetMyReport;

public record Query(Guid TeacherId) : IRequest<EndpointResponse<TeacherSelfReportDto>>;

public record TeacherSelfReportDto(Guid TeacherId, string FullName, int TotalStudents, int TotalLectures, int TotalExams, decimal AverageExamScore, decimal AttendanceRate, decimal AssignmentSubmissionRate);
