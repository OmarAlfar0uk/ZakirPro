using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Data;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.Teachers.Attendance.GetLectureAttendance;

public class Handler : IRequestHandler<Query, EndpointResponse<LectureAttendanceDto>>
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public Handler(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<EndpointResponse<LectureAttendanceDto>> Handle(Query request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            return EndpointResponse<LectureAttendanceDto>.ForbiddenResponse("Not authenticated.");

        Guid effectiveTeacherId;
        if (_currentUser.Role == UserRole.Assistant)
        {
            var assistant = await _db.Assistants.FirstOrDefaultAsync(a => a.Id == _currentUser.UserId.Value, cancellationToken);
            if (assistant == null) return EndpointResponse<LectureAttendanceDto>.ForbiddenResponse("Assistant not found.");
            effectiveTeacherId = assistant.TeacherId;
        }
        else
        {
            effectiveTeacherId = _currentUser.UserId.Value;
        }

        var lecture = await _db.Lectures
            .Include(l => l.TeacherSubjectStage)
            .FirstOrDefaultAsync(l => l.Id == request.LectureId, cancellationToken);

        if (lecture == null)
            return EndpointResponse<LectureAttendanceDto>.NotFoundResponse("Lecture not found.");

        if (lecture.TeacherSubjectStage.TeacherId != effectiveTeacherId)
            return EndpointResponse<LectureAttendanceDto>.ForbiddenResponse("You do not have permission to view attendance for this lecture.");

        var enrolledStudents = await _db.StudentTeacherSubjectStages
            .Include(st => st.Student)
            .Where(st => st.TeacherSubjectStageId == lecture.TeacherSubjectStageId)
            .ToListAsync(cancellationToken);

        var existingAttendances = await _db.Attendances
            .Where(a => a.LectureId == request.LectureId)
            .ToListAsync(cancellationToken);

        var records = enrolledStudents.Select(st => 
        {
            var att = existingAttendances.FirstOrDefault(a => a.StudentId == st.StudentId);
            return new AttendanceRecordDto(
                st.StudentId,
                st.Student.FullName,
                att?.Status.ToString(),
                att?.UpdatedAt ?? att?.CreatedAt
            );
        }).ToList();

        var dto = new LectureAttendanceDto(lecture.Id, lecture.Title, enrolledStudents.Count, records);
        return EndpointResponse<LectureAttendanceDto>.SuccessResponse(dto);
    }
}
