using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Domain.Entities;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.Students.Exams.GetResults;

public class Handler : IRequestHandler<Query, EndpointResponse<ExamResultDto>>
{
    private readonly IUnitOfWork        _uow;
    private readonly ICurrentUserService _currentUser;

    public Handler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow         = uow;
        _currentUser = currentUser;
    }

    public async Task<EndpointResponse<ExamResultDto>> Handle(
        Query request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId != request.StudentId)
            return EndpointResponse<ExamResultDto>.ForbiddenResponse(
                "You are not authorized to view another student's results.");

        var attempt = await _uow.GetRepository<StudentExamAttempt>()
            .Query()
            .Include(a => a.Exam)
                .ThenInclude(e => e.TeacherSubjectStage)
                    .ThenInclude(tss => tss.Subject)
            .Include(a => a.Exam)
                .ThenInclude(e => e.TeacherSubjectStage)
                    .ThenInclude(tss => tss.Teacher)
            .Include(a => a.Answers)
                .ThenInclude(sa => sa.Question)
            .FirstOrDefaultAsync(a =>
                a.Id == request.AttemptId &&
                a.StudentId == request.StudentId,
                cancellationToken);

        if (attempt is null)
            return EndpointResponse<ExamResultDto>.NotFoundResponse("Attempt not found.");

        // Results are only available after submission
        if (attempt.AttemptStatus == AttemptStatus.InProgress)
            return EndpointResponse<ExamResultDto>.ForbiddenResponse(
                "Results are not available until the exam is submitted.");

        var answers     = attempt.Answers.ToList();
        int total       = answers.Count;
        int answered    = answers.Count(sa =>
            sa.SelectedChoiceId.HasValue || !string.IsNullOrWhiteSpace(sa.EssayResponse));
        int correct     = answers.Count(sa => sa.IsCorrect == true);
        int wrong       = answers.Count(sa => sa.IsCorrect == false);
        int pendingEssay = answers.Count(sa =>
            sa.Question.Type == QuestionType.Essay && sa.PointsAwarded is null);

        string gradingStatus = attempt.AttemptStatus == AttemptStatus.PendingGrading
            ? "PendingEssayGrading"
            : "Complete";

        decimal? scorePercent = null;
        if (attempt.FinalScore.HasValue && attempt.MaxScore > 0)
            scorePercent = Math.Round(
                (decimal)attempt.FinalScore.Value / attempt.MaxScore * 100m, 2);

        return EndpointResponse<ExamResultDto>.SuccessResponse(
            new ExamResultDto(
                attempt.Id,
                attempt.Exam.Title,
                attempt.Exam.TeacherSubjectStage.Subject.Name,
                attempt.Exam.TeacherSubjectStage.Teacher.FullName,
                attempt.Exam.ScheduledStart,
                attempt.AttemptStatus.ToString(),
                gradingStatus,
                attempt.FinalScore,
                attempt.MaxScore,
                scorePercent,
                attempt.IsPassed,
                attempt.TimeTakenSeconds,
                attempt.TeacherFeedback,
                new PerformanceSummaryDto(total, answered, correct, wrong, pendingEssay)),
            "Results retrieved successfully.");
    }
}
