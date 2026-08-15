using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Domain.Entities;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.Teachers.Exams.GetExams;

public class Handler : IRequestHandler<Query, EndpointResponse<List<TeacherExamDto>>>
{
    private readonly IUnitOfWork        _uow;
    private readonly ICurrentUserService _currentUser;

    public Handler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow         = uow;
        _currentUser = currentUser;
    }

    public async Task<EndpointResponse<List<TeacherExamDto>>> Handle(
        Query request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId != request.TeacherId)
            return EndpointResponse<List<TeacherExamDto>>.ForbiddenResponse(
                "You are not authorized to view exams on behalf of another teacher.");

        var query = _uow.GetRepository<Exam>()
            .Query()
            .Include(e => e.TeacherSubjectStage)
                .ThenInclude(tss => tss.Subject)
            .Include(e => e.TeacherSubjectStage)
                .ThenInclude(tss => tss.Stage)
            .Include(e => e.Attempts)
            .Where(e => e.TeacherSubjectStage.TeacherId == request.TeacherId);

        if (request.TeacherSubjectStageId.HasValue)
            query = query.Where(e => e.TeacherSubjectStageId == request.TeacherSubjectStageId.Value);

        if (!string.IsNullOrWhiteSpace(request.StatusFilter) &&
            Enum.TryParse<ExamStatus>(request.StatusFilter, ignoreCase: true, out var statusFilter))
            query = query.Where(e => e.Status == statusFilter);

        var exams = await query
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);

        var result = exams.Select(e => new TeacherExamDto(
            e.Id,
            e.Title,
            e.Description,
            e.TeacherSubjectStageId,
            e.TeacherSubjectStage.Subject.Name,
            e.TeacherSubjectStage.Stage.Name,
            e.ScheduledStart,
            e.ScheduledEnd,
            e.DurationMinutes,
            e.PassThresholdPercent,
            e.TotalPoints,
            e.Status.ToString(),
            e.Attempts.Count,
            e.CreatedAt)).ToList();

        return EndpointResponse<List<TeacherExamDto>>.SuccessResponse(result, "Exams retrieved successfully.");
    }
}
