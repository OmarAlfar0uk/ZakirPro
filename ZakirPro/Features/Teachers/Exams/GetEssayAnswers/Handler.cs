using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Domain.Entities;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.Teachers.Exams.GetEssayAnswers;

public class Handler : IRequestHandler<Query, EndpointResponse<List<EssayAnswerDto>>>
{
    private readonly IUnitOfWork        _uow;
    private readonly ICurrentUserService _currentUser;

    public Handler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow         = uow;
        _currentUser = currentUser;
    }

    public async Task<EndpointResponse<List<EssayAnswerDto>>> Handle(
        Query request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId != request.TeacherId)
            return EndpointResponse<List<EssayAnswerDto>>.ForbiddenResponse(
                "You are not authorized to grade exams on behalf of another teacher.");

        // Verify exam ownership
        var exam = await _uow.GetRepository<Exam>()
            .Query()
            .Include(e => e.TeacherSubjectStage)
            .FirstOrDefaultAsync(e => e.Id == request.ExamId, cancellationToken);

        if (exam is null)
            return EndpointResponse<List<EssayAnswerDto>>.NotFoundResponse("Exam not found.");

        if (exam.TeacherSubjectStage.TeacherId != request.TeacherId)
            return EndpointResponse<List<EssayAnswerDto>>.ForbiddenResponse(
                "This exam does not belong to you.");

        // Verify attempt belongs to this exam
        var attempt = await _uow.GetRepository<StudentExamAttempt>()
            .FirstOrDefaultAsync(a => a.Id == request.AttemptId && a.ExamId == request.ExamId);

        if (attempt is null)
            return EndpointResponse<List<EssayAnswerDto>>.NotFoundResponse(
                "Attempt not found for this exam.");

        if (attempt.AttemptStatus == AttemptStatus.InProgress)
            return EndpointResponse<List<EssayAnswerDto>>.ErrorResponse(
                "Cannot grade an attempt that is still in progress.");

        // Fetch essay answers (null PointsAwarded = pending)
        var essayAnswers = await _uow.GetRepository<StudentAnswer>()
            .Query()
            .Include(sa => sa.Question)
            .Where(sa =>
                sa.AttemptId == request.AttemptId &&
                sa.Question.Type == QuestionType.Essay)
            .OrderBy(sa => sa.Question.OrderIndex)
            .ToListAsync(cancellationToken);

        var result = essayAnswers.Select(sa => new EssayAnswerDto(
            sa.Id,
            sa.QuestionId,
            sa.Question.Text,
            sa.Question.Points,
            sa.EssayResponse,
            sa.PointsAwarded)).ToList();

        return EndpointResponse<List<EssayAnswerDto>>.SuccessResponse(result, "Essay answers retrieved successfully.");
    }
}
