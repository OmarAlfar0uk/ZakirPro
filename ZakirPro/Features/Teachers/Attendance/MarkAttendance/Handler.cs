using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Data;
using ZakirPro.Domain.Entities;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.Teachers.Attendance.MarkAttendance;

public class Handler : IRequestHandler<Command, EndpointResponse<MarkAttendanceResponse>>
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public Handler(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<EndpointResponse<MarkAttendanceResponse>> Handle(Command request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            return EndpointResponse<MarkAttendanceResponse>.ForbiddenResponse("Not authenticated.");

        Guid effectiveTeacherId;
        if (_currentUser.Role == UserRole.Assistant)
        {
            var assistant = await _db.Assistants.FirstOrDefaultAsync(a => a.Id == _currentUser.UserId.Value, cancellationToken);
            if (assistant == null) return EndpointResponse<MarkAttendanceResponse>.ForbiddenResponse("Assistant not found.");
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
            return EndpointResponse<MarkAttendanceResponse>.NotFoundResponse("Lecture not found.");

        if (lecture.TeacherSubjectStage.TeacherId != effectiveTeacherId)
            return EndpointResponse<MarkAttendanceResponse>.ForbiddenResponse("You do not have permission to mark attendance for this lecture.");

        var enrolledStudentIds = await _db.StudentTeacherSubjectStages
            .Where(st => st.TeacherSubjectStageId == lecture.TeacherSubjectStageId)
            .Select(st => st.StudentId)
            .ToListAsync(cancellationToken);

        var existingAttendances = await _db.Attendances
            .Where(a => a.LectureId == request.LectureId)
            .ToListAsync(cancellationToken);

        int updatedCount = 0;

        foreach (var entry in request.Entries)
        {
            if (!enrolledStudentIds.Contains(entry.StudentId)) continue;
            
            var status = Enum.Parse<AttendanceStatus>(entry.Status, ignoreCase: true);

            var existing = existingAttendances.FirstOrDefault(a => a.StudentId == entry.StudentId);
            if (existing != null)
            {
                existing.Status = status;
                existing.MarkedByUserId = _currentUser.UserId.Value;
                existing.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _db.Attendances.Add(new ZakirPro.Domain.Entities.Attendance
                {
                    StudentId = entry.StudentId,
                    LectureId = request.LectureId,
                    Status = status,
                    MarkedByUserId = _currentUser.UserId.Value,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            updatedCount++;
        }

        await _db.SaveChangesAsync(cancellationToken);

        return EndpointResponse<MarkAttendanceResponse>.SuccessResponse(
            new MarkAttendanceResponse(updatedCount),
            $"Attendance marked for {updatedCount} students.");
    }
}
