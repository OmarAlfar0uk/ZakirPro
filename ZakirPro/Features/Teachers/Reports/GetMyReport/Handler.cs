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

namespace ZakirPro.Features.Teachers.Reports.GetMyReport;

public class Handler : IRequestHandler<Query, EndpointResponse<TeacherSelfReportDto>>
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public Handler(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<EndpointResponse<TeacherSelfReportDto>> Handle(Query request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            return EndpointResponse<TeacherSelfReportDto>.ForbiddenResponse("Not authenticated.");

        Guid effectiveTeacherId;
        if (_currentUser.Role == UserRole.Assistant)
        {
            var assistant = await _db.Assistants.FirstOrDefaultAsync(a => a.Id == _currentUser.UserId.Value, cancellationToken);
            if (assistant == null) return EndpointResponse<TeacherSelfReportDto>.ForbiddenResponse("Assistant not found.");
            effectiveTeacherId = assistant.TeacherId;
        }
        else
        {
            if (_currentUser.UserId.Value != request.TeacherId)
                return EndpointResponse<TeacherSelfReportDto>.ForbiddenResponse("You are not authorized.");
            effectiveTeacherId = request.TeacherId;
        }

        var teacher = await _db.Teachers
            .Where(t => t.Id == effectiveTeacherId && !t.IsDeleted)
            .Select(t => new
            {
                t.Id,
                t.FullName,
                TotalStudents = t.TeacherSubjectStages.SelectMany(tss => tss.StudentLinks).Where(sl => sl.Student.IsActive && !sl.Student.IsDeleted).Select(sl => sl.StudentId).Distinct().Count(),
                TotalLectures = t.TeacherSubjectStages.SelectMany(tss => tss.Lectures).Count(l => !l.IsDeleted),
                TotalExams = t.TeacherSubjectStages.SelectMany(tss => tss.Exams).Count(e => !e.IsDeleted),
                TotalAttendances = t.TeacherSubjectStages.SelectMany(tss => tss.Lectures).SelectMany(l => l.Attendances).Count(),
                PresentAttendances = t.TeacherSubjectStages.SelectMany(tss => tss.Lectures).SelectMany(l => l.Attendances).Count(a => a.Status == AttendanceStatus.Present),
                GradedExamsCount = t.TeacherSubjectStages.SelectMany(tss => tss.Exams).SelectMany(e => e.Attempts).Count(sea => sea.AttemptStatus == AttemptStatus.Graded),
                TotalExamScore = t.TeacherSubjectStages.SelectMany(tss => tss.Exams).SelectMany(e => e.Attempts).Where(sea => sea.AttemptStatus == AttemptStatus.Graded).Sum(sea => (decimal?)sea.FinalScore) ?? 0m
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (teacher == null)
            return EndpointResponse<TeacherSelfReportDto>.NotFoundResponse("Teacher not found.");

        var attendanceRate = teacher.TotalAttendances > 0 ? (decimal)teacher.PresentAttendances / teacher.TotalAttendances : 0m;
        var avgExamScore = teacher.GradedExamsCount > 0 ? teacher.TotalExamScore / teacher.GradedExamsCount : 0m;

        var dto = new TeacherSelfReportDto(
            teacher.Id,
            teacher.FullName,
            teacher.TotalStudents,
            teacher.TotalLectures,
            teacher.TotalExams,
            avgExamScore,
            attendanceRate,
            0m // AssignmentSubmissionRate placeholder
        );

        return EndpointResponse<TeacherSelfReportDto>.SuccessResponse(dto);
    }
}
