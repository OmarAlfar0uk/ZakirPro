using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Domain.Entities;

namespace ZakirPro.Features.Students.GetTeacherCards;

public class Handler : IRequestHandler<Query, EndpointResponse<List<TeacherCardDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public Handler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<EndpointResponse<List<TeacherCardDto>>> Handle(
        Query request,
        CancellationToken cancellationToken)
    {
        // Authorization: ensure the calling student matches the requested StudentId
        if (_currentUser.UserId != request.StudentId)
            return EndpointResponse<List<TeacherCardDto>>.ForbiddenResponse(
                "You are not authorized to view this student's data.");

        // Load all enrolment links with related navigation properties
        var links = await _uow.GetRepository<StudentTeacherSubjectStage>()
            .Query()
            .Where(s => s.StudentId == request.StudentId)
            .Include(s => s.TeacherSubjectStage)
                .ThenInclude(ts => ts.Teacher)
            .Include(s => s.TeacherSubjectStage)
                .ThenInclude(ts => ts.Subject)
            .Include(s => s.TeacherSubjectStage)
                .ThenInclude(ts => ts.Stage)
            .ToListAsync(cancellationToken);

        var result = new List<TeacherCardDto>(links.Count);

        foreach (var link in links)
        {
            var count = await _uow.GetRepository<StudentTeacherSubjectStage>()
                .Query()
                .CountAsync(
                    s => s.TeacherSubjectStageId == link.TeacherSubjectStageId,
                    cancellationToken);

            result.Add(new TeacherCardDto(
                TeacherId: link.TeacherSubjectStage.TeacherId,
                TeacherName: link.TeacherSubjectStage.Teacher.FullName,
                Subject: link.TeacherSubjectStage.Subject.Name,
                Stage: link.TeacherSubjectStage.Stage.Name,
                NumberOfStudents: count));
        }

        return EndpointResponse<List<TeacherCardDto>>.SuccessResponse(
            result,
            "Teacher cards retrieved successfully.");
    }
}
