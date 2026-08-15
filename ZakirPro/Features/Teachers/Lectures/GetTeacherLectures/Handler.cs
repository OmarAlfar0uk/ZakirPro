using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Data;
using ZakirPro.Domain.Enums;
using ZakirPro.Features.Teachers.Lectures.CreateLecture;

namespace ZakirPro.Features.Teachers.Lectures.GetTeacherLectures;

public class Handler : IRequestHandler<Query, EndpointResponse<List<LectureDto>>>
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public Handler(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<EndpointResponse<List<LectureDto>>> Handle(Query request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            return EndpointResponse<List<LectureDto>>.ForbiddenResponse("Not authenticated.");

        Guid effectiveTeacherId;
        if (_currentUser.Role == UserRole.Assistant)
        {
            var assistant = await _db.Assistants.FirstOrDefaultAsync(a => a.Id == _currentUser.UserId.Value, cancellationToken);
            if (assistant == null) return EndpointResponse<List<LectureDto>>.ForbiddenResponse("Assistant not found.");
            effectiveTeacherId = assistant.TeacherId;
        }
        else
        {
            effectiveTeacherId = _currentUser.UserId.Value;
        }

        var query = _db.Lectures
            .Include(l => l.TeacherSubjectStage)
            .Where(l => l.TeacherSubjectStage.TeacherId == effectiveTeacherId);

        if (request.TeacherSubjectStageId.HasValue)
        {
            query = query.Where(l => l.TeacherSubjectStageId == request.TeacherSubjectStageId.Value);
        }

        var lectures = await query
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => new LectureDto(l.Id, l.Title, l.Description, l.GoogleDriveLink, l.TeacherSubjectStageId, l.CreatedAt))
            .ToListAsync(cancellationToken);

        return EndpointResponse<List<LectureDto>>.SuccessResponse(lectures);
    }
}
