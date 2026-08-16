using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Models;
using ZakirPro.Data;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.Admins.Reports.GetTeachersReport;

public class Handler : IRequestHandler<Query, EndpointResponse<PaginatedResult<TeacherReportDto>>>
{
    private readonly AppDbContext _db;

    public Handler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<EndpointResponse<PaginatedResult<TeacherReportDto>>> Handle(Query request, CancellationToken cancellationToken)
    {
        var query = _db.Teachers
            .Where(t => !t.IsDeleted && t.IsActive)
            .OrderBy(t => t.FullName);

        var total = await query.CountAsync(cancellationToken);

        var teachers = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new
            {
                t.Id,
                t.FullName,
                ActiveStudentCount = t.TeacherSubjectStages.SelectMany(tss => tss.StudentLinks).Where(sl => sl.Student.IsActive && !sl.Student.IsDeleted).Select(sl => sl.StudentId).Distinct().Count(),
                LectureCount = t.TeacherSubjectStages.SelectMany(tss => tss.Lectures).Count(l => !l.IsDeleted),
                ExamCount = t.TeacherSubjectStages.SelectMany(tss => tss.Exams).Count(e => !e.IsDeleted),
                TotalAttendances = t.TeacherSubjectStages.SelectMany(tss => tss.Lectures).SelectMany(l => l.Attendances).Count(),
                PresentAttendances = t.TeacherSubjectStages.SelectMany(tss => tss.Lectures).SelectMany(l => l.Attendances).Count(a => a.Status == AttendanceStatus.Present),
                TotalSubmissions = t.TeacherSubjectStages.SelectMany(tss => tss.Assignments).SelectMany(a => a.Submissions).Count(s => !s.IsDeleted && s.Status != AssignmentSubmissionStatus.NotSubmitted),
                ExpectedSubmissions = t.TeacherSubjectStages.Sum(tss => tss.Assignments.Count(a => !a.IsDeleted) * tss.StudentLinks.Count(sl => sl.Student.IsActive && !sl.Student.IsDeleted))
            })
            .ToListAsync(cancellationToken);

        var items = teachers.Select(t => new TeacherReportDto(
            t.Id,
            t.FullName,
            t.ActiveStudentCount,
            t.LectureCount,
            t.ExamCount,
            t.TotalAttendances > 0 ? (decimal)t.PresentAttendances / t.TotalAttendances : 0m,
            t.ExpectedSubmissions > 0 ? (decimal)t.TotalSubmissions / t.ExpectedSubmissions : 0m
        )).ToList();

        return EndpointResponse<PaginatedResult<TeacherReportDto>>.SuccessResponse(new PaginatedResult<TeacherReportDto>(items, total, request.Page, request.PageSize));
    }
}
