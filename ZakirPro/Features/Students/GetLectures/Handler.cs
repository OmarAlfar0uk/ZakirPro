using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Domain.Entities;

namespace ZakirPro.Features.Students.GetLectures;

public class Handler : IRequestHandler<Query, EndpointResponse<List<LectureDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public Handler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<EndpointResponse<List<LectureDto>>> Handle(
        Query request,
        CancellationToken cancellationToken)
    {
        // Authorization: ensure the calling student matches the requested StudentId
        if (_currentUser.UserId != request.StudentId)
            return EndpointResponse<List<LectureDto>>.ForbiddenResponse(
                "You are not authorized to view this student's data.");

        // Find the TeacherSubjectStage record matching teacher+subject+stage
        var tss = await _uow.GetRepository<TeacherSubjectStage>()
            .FirstOrDefaultAsync(t =>
                t.TeacherId == request.TeacherId &&
                t.SubjectId == request.SubjectId &&
                t.StageId == request.StageId);

        if (tss is null)
            return EndpointResponse<List<LectureDto>>.NotFoundResponse(
                "The specified Teacher/Subject/Stage combination was not found.");

        // Verify the student is enrolled in this combination
        var isEnrolled = await _uow.GetRepository<StudentTeacherSubjectStage>()
            .ExistsAsync(s =>
                s.StudentId == request.StudentId &&
                s.TeacherSubjectStageId == tss.Id);

        if (!isEnrolled)
            return EndpointResponse<List<LectureDto>>.ForbiddenResponse(
                "You are not enrolled in this Teacher/Subject/Stage combination.");

        // Retrieve lectures ordered by creation date
        var lectures = await _uow.GetRepository<Lecture>()
            .Query()
            .Where(l => l.TeacherSubjectStageId == tss.Id)
            .OrderBy(l => l.CreatedAt)
            .ToListAsync(cancellationToken);

        var result = lectures.Select(l => new LectureDto(
            Id: l.Id,
            Title: l.Title,
            Description: l.Description,
            GoogleDriveLink: l.GoogleDriveLink,
            CreatedAt: l.CreatedAt)).ToList();

        return EndpointResponse<List<LectureDto>>.SuccessResponse(
            result,
            "Lectures retrieved successfully.");
    }
}
