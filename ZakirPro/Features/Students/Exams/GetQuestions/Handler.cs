using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Domain.Entities;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.Students.Exams.GetQuestions;

public class Handler : IRequestHandler<Query, EndpointResponse<ActiveExamDto>>
{
    private readonly IUnitOfWork        _uow;
    private readonly ICurrentUserService _currentUser;

    public Handler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow         = uow;
        _currentUser = currentUser;
    }

    public async Task<EndpointResponse<ActiveExamDto>> Handle(
        Query request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId != request.StudentId)
            return EndpointResponse<ActiveExamDto>.ForbiddenResponse(
                "You are not authorized to access another student's attempt.");

        // Ownership check: attempt must belong to this student
        var attempt = await _uow.GetRepository<StudentExamAttempt>()
            .Query()
            .Include(a => a.Answers)
            .FirstOrDefaultAsync(a =>
                a.Id == request.AttemptId &&
                a.StudentId == request.StudentId,
                cancellationToken);

        if (attempt is null)
            return EndpointResponse<ActiveExamDto>.NotFoundResponse("Attempt not found.");

        if (attempt.AttemptStatus != AttemptStatus.InProgress)
            return EndpointResponse<ActiveExamDto>.ErrorResponse(
                "This attempt has already been submitted.");

        // Server-side deadline check (timer is cosmetic on client)
        if (DateTime.UtcNow > attempt.DeadlineAt)
            return EndpointResponse<ActiveExamDto>.ErrorResponse(
                "Exam time has expired. Please submit your attempt.");

        // Fetch questions with choices — IsCorrect deliberately excluded from DTO
        var questions = await _uow.GetRepository<Question>()
            .Query()
            .Include(q => q.Choices.OrderBy(c => c.OrderIndex))
            .Where(q => q.ExamId == attempt.ExamId)
            .OrderBy(q => q.OrderIndex)
            .ToListAsync(cancellationToken);

        var answerByQuestion = attempt.Answers.ToDictionary(a => a.QuestionId);

        var questionDtos = questions.Select(q =>
        {
            answerByQuestion.TryGetValue(q.Id, out var savedAnswer);

            return new QuestionDto(
                q.Id,
                q.OrderIndex,
                q.Text,
                q.Type.ToString(),
                q.Points,
                // SECURITY: IsCorrect is explicitly NOT included in ChoiceDto
                q.Choices.Select(c => new ChoiceDto(c.Id, c.Text, c.OrderIndex)).ToList(),
                savedAnswer?.SelectedChoiceId,
                savedAnswer?.EssayResponse,
                savedAnswer?.IsMarkedForReview ?? false);
        }).ToList();

        return EndpointResponse<ActiveExamDto>.SuccessResponse(
            new ActiveExamDto(attempt.Id, attempt.DeadlineAt, questions.Count, questionDtos),
            "Questions retrieved successfully.");
    }
}
