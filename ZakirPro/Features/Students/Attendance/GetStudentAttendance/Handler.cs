using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Domain.Entities;
using ZakirPro.Data;

namespace ZakirPro.Features.Students.Attendance.GetStudentAttendance;

public class Handler : IRequestHandler<Query, EndpointResponse<List<StudentAttendanceDto>>>
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public Handler(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<EndpointResponse<List<StudentAttendanceDto>>> Handle(Query request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId != request.StudentId)
            return EndpointResponse<List<StudentAttendanceDto>>.ForbiddenResponse("You are not authorized to view this data.");

        var query = _db.Attendances
            .Include(a => a.Lecture)
                .ThenInclude(l => l.TeacherSubjectStage)
                    .ThenInclude(tss => tss.Subject)
            .Include(a => a.Lecture)
                .ThenInclude(l => l.TeacherSubjectStage)
                    .ThenInclude(tss => tss.Stage)
            .Where(a => a.StudentId == request.StudentId);

        if (request.TeacherSubjectStageId.HasValue)
        {
            query = query.Where(a => a.Lecture.TeacherSubjectStageId == request.TeacherSubjectStageId.Value);
        }

        var results = await query
            .OrderByDescending(a => a.Lecture.CreatedAt)
            .Select(a => new StudentAttendanceDto(
                a.LectureId,
                a.Lecture.Title,
                a.Lecture.TeacherSubjectStage.Subject.Name,
                a.Lecture.TeacherSubjectStage.Stage.Name,
                a.Status.ToString(),
                a.UpdatedAt
            ))
            .ToListAsync(cancellationToken);

        return EndpointResponse<List<StudentAttendanceDto>>.SuccessResponse(results);
    }
}
