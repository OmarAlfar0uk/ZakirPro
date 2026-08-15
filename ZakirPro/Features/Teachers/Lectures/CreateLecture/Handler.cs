using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Data;
using ZakirPro.Domain.Entities;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.Teachers.Lectures.CreateLecture;

public class Handler : IRequestHandler<Command, EndpointResponse<LectureDto>>
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public Handler(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<EndpointResponse<LectureDto>> Handle(Command request, CancellationToken cancellationToken)
    {
        if (!_currentUser.UserId.HasValue)
            return EndpointResponse<LectureDto>.ForbiddenResponse("Not authenticated.");

        Guid effectiveTeacherId;
        if (_currentUser.Role == UserRole.Assistant)
        {
            var assistant = await _db.Assistants.FirstOrDefaultAsync(a => a.Id == _currentUser.UserId.Value, cancellationToken);
            if (assistant == null) return EndpointResponse<LectureDto>.ForbiddenResponse("Assistant not found.");
            effectiveTeacherId = assistant.TeacherId;
        }
        else
        {
            effectiveTeacherId = _currentUser.UserId.Value;
        }

        var tss = await _db.TeacherSubjectStages.FirstOrDefaultAsync(t => t.Id == request.TeacherSubjectStageId, cancellationToken);
        if (tss == null || tss.TeacherId != effectiveTeacherId)
            return EndpointResponse<LectureDto>.ForbiddenResponse("You do not own this subject/stage assignment.");

        var lecture = new Lecture
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            GoogleDriveLink = request.GoogleDriveLink.Trim(),
            TeacherSubjectStageId = request.TeacherSubjectStageId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Lectures.Add(lecture);
        await _db.SaveChangesAsync(cancellationToken);

        var dto = new LectureDto(lecture.Id, lecture.Title, lecture.Description, lecture.GoogleDriveLink, lecture.TeacherSubjectStageId, lecture.CreatedAt);
        return EndpointResponse<LectureDto>.SuccessResponse(dto, "Lecture created successfully.");
    }
}
