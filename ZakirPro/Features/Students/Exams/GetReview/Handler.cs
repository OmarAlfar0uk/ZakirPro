using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Domain.Entities;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.Students.Exams.GetReview;

public class Handler : IRequestHandler<Query, EndpointResponse<ReviewDto>>
{
    private readonly IUnitOfWork        _uow;
    private readonly ICurrentUserService _currentUser;

    public Handler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow         = uow;
        _currentUser = currentUser;
    }

    public async Task<EndpointResponse<ReviewDto>> Handle(
        Query request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId != request.StudentId)
            return EndpointResponse<ReviewDto>.ForbiddenResponse(
                "You are not authorized to review another student's attempt.");

        var attempt = await _uow.GetRepository<StudentExamAttempt>()
            .Query()
            .Include(a => a.Answers)
                .ThenInclude(sa => sa.Question)
                    .ThenInclude(q => q.Choices.OrderBy(c => c.OrderIndex))
            .FirstOrDefaultAsync(a =>
                a.Id == request.AttemptId &&
                a.StudentId == request.StudentId,
                cancellationToken);

        if (attempt is null)
            return EndpointResponse<ReviewDto>.NotFoundResponse("Attempt not found.");

        // Review is only accessible after submission (Graded or PendingGrading)
        if (attempt.AttemptStatus == AttemptStatus.InProgress)
            return EndpointResponse<ReviewDto>.ForbiddenResponse(
                "Review is not available until the exam is submitted.");

        var reviewQuestions = attempt.Answers
            .OrderBy(sa => sa.Question.OrderIndex)
            .Select(sa =>
            {
                string? essayGradeLabel = null;
                if (sa.Question.Type == QuestionType.Essay)
                {
                    essayGradeLabel = sa.PointsAwarded switch
                    {
                        null                                          => "pending",
                        int p when p == sa.Question.Points            => "full",
                        int p when p == 0                             => "none",
                        _                                             => "partial"
                    };
                }

                return new ReviewQuestionDto(
                    sa.QuestionId,
                    sa.Question.OrderIndex,
                    sa.Question.Text,
                    sa.Question.Type.ToString(),
                    sa.Question.Points,
                    // IsCorrect is now revealed — exam is over
                    sa.Question.Choices
                        .Select(c => new ReviewChoiceDto(c.Id, c.Text, c.OrderIndex, c.IsCorrect))
                        .ToList(),
                    sa.SelectedChoiceId,
                    sa.EssayResponse,
                    sa.IsCorrect,
                    sa.PointsAwarded,
                    essayGradeLabel);
            })
            .ToList();

        return EndpointResponse<ReviewDto>.SuccessResponse(
            new ReviewDto(attempt.Id, attempt.AttemptStatus.ToString(), reviewQuestions),
            "Review retrieved successfully.");
    }
}
