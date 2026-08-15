using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Domain.Entities;

namespace ZakirPro.Features.Teachers.Exams.GetAttempts;

public class Handler : IRequestHandler<Query, EndpointResponse<List<AttemptSummaryDto>>>
{
    private readonly IUnitOfWork        _uow;
    private readonly ICurrentUserService _currentUser;

    public Handler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow         = uow;
        _currentUser = currentUser;
    }

    public async Task<EndpointResponse<List<AttemptSummaryDto>>> Handle(
        Query request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId != request.TeacherId)
            return EndpointResponse<List<AttemptSummaryDto>>.ForbiddenResponse(
                "You are not authorized to view attempts on behalf of another teacher.");

        var exam = await _uow.GetRepository<Exam>()
            .Query()
            .Include(e => e.TeacherSubjectStage)
            .FirstOrDefaultAsync(e => e.Id == request.ExamId, cancellationToken);

        if (exam is null)
            return EndpointResponse<List<AttemptSummaryDto>>.NotFoundResponse("Exam not found.");

        if (exam.TeacherSubjectStage.TeacherId != request.TeacherId)
            return EndpointResponse<List<AttemptSummaryDto>>.ForbiddenResponse(
                "This exam does not belong to you.");

        var attempts = await _uow.GetRepository<StudentExamAttempt>()
            .Query()
            .Include(a => a.Student)
            .Where(a => a.ExamId == request.ExamId)
            .OrderByDescending(a => a.StartedAt)
            .ToListAsync(cancellationToken);

        var result = attempts.Select(a => new AttemptSummaryDto(
            a.Id,
            a.StudentId,
            a.Student.FullName,
            a.AttemptStatus.ToString(),
            a.FinalScore,
            a.MaxScore,
            a.IsPassed,
            a.SubmittedAt)).ToList();

        return EndpointResponse<List<AttemptSummaryDto>>.SuccessResponse(result, "Attempts retrieved successfully.");
    }
}
