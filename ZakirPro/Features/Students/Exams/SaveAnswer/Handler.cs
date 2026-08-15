using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Data;
using ZakirPro.Domain.Entities;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.Students.Exams.SaveAnswer;

public class Handler : IRequestHandler<Command, EndpointResponse<SavedAnswerDto>>
{
    private readonly IUnitOfWork        _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly AppDbContext       _db;

    public Handler(IUnitOfWork uow, ICurrentUserService currentUser, AppDbContext db)
    {
        _uow         = uow;
        _currentUser = currentUser;
        _db          = db;
    }

    public async Task<EndpointResponse<SavedAnswerDto>> Handle(
        Command request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId != request.StudentId)
            return EndpointResponse<SavedAnswerDto>.ForbiddenResponse(
                "You are not authorized to save answers on behalf of another student.");

        // Load attempt with ownership check
        var attempt = await _uow.GetRepository<StudentExamAttempt>()
            .FirstOrDefaultAsync(a =>
                a.Id == request.AttemptId &&
                a.StudentId == request.StudentId);

        if (attempt is null)
            return EndpointResponse<SavedAnswerDto>.NotFoundResponse("Attempt not found.");

        if (attempt.AttemptStatus != AttemptStatus.InProgress)
            return EndpointResponse<SavedAnswerDto>.ErrorResponse(
                "This attempt has already been submitted.");

        // Server-side deadline enforcement — reject saves after deadline
        if (DateTime.UtcNow > attempt.DeadlineAt)
            return EndpointResponse<SavedAnswerDto>.ErrorResponse(
                "Exam time has expired. Submit your attempt.");

        // Verify the question belongs to this exam
        var question = await _uow.GetRepository<Question>()
            .FirstOrDefaultAsync(q =>
                q.Id == request.QuestionId &&
                q.ExamId == attempt.ExamId);

        if (question is null)
            return EndpointResponse<SavedAnswerDto>.NotFoundResponse(
                "Question not found in this exam.");

        // Validate selected choice ownership
        if (request.SelectedChoiceId.HasValue)
        {
            if (question.Type == QuestionType.Essay)
                return EndpointResponse<SavedAnswerDto>.ErrorResponse(
                    "Essay questions do not accept a selected choice.");

            var choiceExists = await _uow.GetRepository<Choice>()
                .ExistsAsync(c =>
                    c.Id == request.SelectedChoiceId.Value &&
                    c.QuestionId == question.Id);

            if (!choiceExists)
                return EndpointResponse<SavedAnswerDto>.ErrorResponse(
                    "The selected choice does not belong to this question.");
        }

        // Upsert the StudentAnswer row — load with tracking via DbContext
        var existingAnswer = await _db.StudentAnswers
            .FirstOrDefaultAsync(sa =>
                sa.AttemptId == request.AttemptId &&
                sa.QuestionId == request.QuestionId,
                cancellationToken);

        var now = DateTime.UtcNow;

        if (existingAnswer is not null)
        {
            existingAnswer.SelectedChoiceId  = request.SelectedChoiceId;
            existingAnswer.EssayResponse     = request.EssayResponse;
            existingAnswer.IsMarkedForReview = request.IsMarkedForReview;
            existingAnswer.UpdatedAt         = now;
        }
        else
        {
            // Stub was already created on StartAttempt; this branch handles edge cases
            await _db.StudentAnswers.AddAsync(new StudentAnswer
            {
                AttemptId        = request.AttemptId,
                QuestionId       = request.QuestionId,
                SelectedChoiceId = request.SelectedChoiceId,
                EssayResponse    = request.EssayResponse,
                IsMarkedForReview = request.IsMarkedForReview,
                CreatedAt        = now,
                UpdatedAt        = now
            }, cancellationToken);
        }

        await _db.SaveChangesAsync(cancellationToken);

        return EndpointResponse<SavedAnswerDto>.SuccessResponse(
            new SavedAnswerDto(
                request.QuestionId,
                request.SelectedChoiceId,
                request.EssayResponse,
                request.IsMarkedForReview,
                now),
            "Answer saved.");
    }
}
