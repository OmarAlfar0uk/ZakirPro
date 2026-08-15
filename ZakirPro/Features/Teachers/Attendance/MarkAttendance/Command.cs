using System;
using System.Collections.Generic;
using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Teachers.Attendance.MarkAttendance;

public record Command(Guid LectureId, List<AttendanceEntry> Entries) : IRequest<EndpointResponse<MarkAttendanceResponse>>;

public record AttendanceEntry(Guid StudentId, string Status);

public record MarkAttendanceResponse(int MarkedCount);
