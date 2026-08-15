using System;
using System.Collections.Generic;
using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Teachers.Attendance.GetLectureAttendance;

public record Query(Guid LectureId) : IRequest<EndpointResponse<LectureAttendanceDto>>;

public record LectureAttendanceDto(Guid LectureId, string LectureTitle, int TotalEnrolled, List<AttendanceRecordDto> Records);

public record AttendanceRecordDto(Guid StudentId, string StudentFullName, string? Status, DateTime? MarkedAt);
