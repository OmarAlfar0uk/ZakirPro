using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Data;
using ZakirPro.Domain.Enums;
using ZakirPro.Features.Teachers.Lectures.CreateLecture;

namespace ZakirPro.Features.Teachers.Lectures.UpdateLecture;

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

        var lecture = await _db.Lectures
            .Include(l => l.TeacherSubjectStage)
            .FirstOrDefaultAsync(l => l.Id == request.LectureId, cancellationToken);

        if (lecture == null)
            return EndpointResponse<LectureDto>.NotFoundResponse("Lecture not found.");

        if (lecture.TeacherSubjectStage.TeacherId != effectiveTeacherId)
            return EndpointResponse<LectureDto>.ForbiddenResponse("You do not have permission to update this lecture.");

        lecture.Title = request.Title.Trim();
        lecture.Description = request.Description?.Trim();
        lecture.GoogleDriveLink = request.GoogleDriveLink.Trim();
        lecture.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        var dto = new LectureDto(lecture.Id, lecture.Title, lecture.Description, lecture.GoogleDriveLink, lecture.TeacherSubjectStageId, lecture.CreatedAt);
        return EndpointResponse<LectureDto>.SuccessResponse(dto, "Lecture updated successfully.");
    }
}
