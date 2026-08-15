using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Domain.Entities;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.Students.Exams.StartAttempt;

public class Handler : IRequestHandler<Command, EndpointResponse<AttemptStartedDto>>
{
    private readonly IUnitOfWork        _uow;
    private readonly ICurrentUserService _currentUser;

    public Handler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow         = uow;
        _currentUser = currentUser;
    }

    public async Task<EndpointResponse<AttemptStartedDto>> Handle(
        Command request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId != request.StudentId)
            return EndpointResponse<AttemptStartedDto>.ForbiddenResponse(
                "You are not authorized to start an attempt on behalf of another student.");

        var now = DateTime.UtcNow;

        // Load exam
        var exam = await _uow.GetRepository<Exam>()
            .Query()
            .Include(e => e.Questions.Where(q => !q.IsDeleted))
            .FirstOrDefaultAsync(e => e.Id == request.ExamId, cancellationToken);

        if (exam is null)
            return EndpointResponse<AttemptStartedDto>.NotFoundResponse("Exam not found.");

        if (exam.Status != ExamStatus.Published)
            return EndpointResponse<AttemptStartedDto>.ErrorResponse("This exam is not available.");

        // Window enforcement
        if (now < exam.ScheduledStart)
            return EndpointResponse<AttemptStartedDto>.ForbiddenResponse(
                "Exam has not started yet.");

        if (now > exam.ScheduledEnd)
            return EndpointResponse<AttemptStartedDto>.ForbiddenResponse(
                "Exam window has closed.");

        // Student enrollment check
        var isEnrolled = await _uow.GetRepository<StudentTeacherSubjectStage>()
            .ExistsAsync(s =>
                s.StudentId == request.StudentId &&
                s.TeacherSubjectStageId == exam.TeacherSubjectStageId);

        if (!isEnrolled)
            return EndpointResponse<AttemptStartedDto>.ForbiddenResponse(
                "You are not enrolled in this course.");

        // No-retake check (design decision 7.3)
        var existingAttempt = await _uow.GetRepository<StudentExamAttempt>()
            .FirstOrDefaultAsync(a =>
                a.StudentId == request.StudentId &&
                a.ExamId == request.ExamId);

        if (existingAttempt is not null)
            return EndpointResponse<AttemptStartedDto>.ErrorResponse(
                "You have already attempted this exam.");

        // Compute deadline: MIN(StartedAt + DurationMinutes, ScheduledEnd) — design decision 7.4
        var deadline = new[] { now.AddMinutes(exam.DurationMinutes), exam.ScheduledEnd }.Min();

        var attempt = new StudentExamAttempt
        {
            StudentId     = request.StudentId,
            ExamId        = exam.Id,
            StartedAt     = now,
            DeadlineAt    = deadline,
            MaxScore      = exam.TotalPoints,
            AttemptStatus = AttemptStatus.InProgress
        };

        // Create stub StudentAnswer rows for every question so answered/unanswered count is trivial
        var stubAnswers = exam.Questions.Select(q => new StudentAnswer
        {
            AttemptId  = attempt.Id,
            QuestionId = q.Id,
            CreatedAt  = now
        }).ToList();

        attempt.Answers = stubAnswers;

        await _uow.GetRepository<StudentExamAttempt>().AddAsync(attempt);
        await _uow.SaveChangesAsync(cancellationToken);

        return EndpointResponse<AttemptStartedDto>.SuccessResponse(
            new AttemptStartedDto(attempt.Id, attempt.StartedAt, attempt.DeadlineAt, exam.Questions.Count),
            "Exam started. Good luck!");
    }
}
