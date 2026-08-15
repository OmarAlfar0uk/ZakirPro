using System;
using System.Collections.Generic;
using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Students.Attendance.GetStudentAttendance;

public record Query(Guid StudentId, Guid? TeacherSubjectStageId) : IRequest<EndpointResponse<List<StudentAttendanceDto>>>;

public record StudentAttendanceDto(Guid LectureId, string LectureTitle, string SubjectName, string StageName, string Status, DateTime? MarkedAt);
