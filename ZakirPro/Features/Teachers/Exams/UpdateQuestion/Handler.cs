using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Data;
using ZakirPro.Domain.Entities;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.Teachers.Exams.UpdateQuestion;

public class Handler : IRequestHandler<Command, EndpointResponse<object>>
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

    public async Task<EndpointResponse<object>> Handle(
        Command request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId != request.TeacherId)
            return EndpointResponse<object>.ForbiddenResponse(
                "You are not authorized to update questions on behalf of another teacher.");

        var exam = await _uow.GetRepository<Exam>()
            .Query()
            .Include(e => e.TeacherSubjectStage)
            .FirstOrDefaultAsync(e => e.Id == request.ExamId, cancellationToken);

        if (exam is null)
            return EndpointResponse<object>.NotFoundResponse("Exam not found.");

        if (exam.TeacherSubjectStage.TeacherId != request.TeacherId)
            return EndpointResponse<object>.ForbiddenResponse("This exam does not belong to you.");

        if (exam.Status != ExamStatus.Draft)
            return EndpointResponse<object>.ErrorResponse(
                "Questions can only be edited on Draft exams.");

        // Load question with tracking so we can update it
        var question = await _db.Questions
            .Include(q => q.Choices)
            .FirstOrDefaultAsync(q => q.Id == request.QuestionId && q.ExamId == request.ExamId, cancellationToken);

        if (question is null)
            return EndpointResponse<object>.NotFoundResponse("Question not found in this exam.");

        // Validate choice count matches question type
        var expectedChoiceCount = question.Type switch
        {
            QuestionType.SingleChoice => 4,
            QuestionType.TrueFalse    => 2,
            QuestionType.Essay        => 0,
            _                         => 0
        };

        if (request.Choices.Count != expectedChoiceCount)
            return EndpointResponse<object>.ErrorResponse(
                $"{question.Type} questions must have exactly {expectedChoiceCount} choices.");

        if (question.Type != QuestionType.Essay &&
            request.Choices.Count(c => c.IsCorrect) != 1)
            return EndpointResponse<object>.ErrorResponse(
                "Exactly one correct choice must be specified.");

        int oldPoints = question.Points;

        // Update question fields
        question.Text      = request.Text;
        question.Points    = request.Points;
        question.UpdatedAt = DateTime.UtcNow;

        // Replace choices atomically: remove old, add new
        _db.Choices.RemoveRange(question.Choices);
        question.Choices = request.Choices.Select(c => new Choice
        {
            QuestionId = question.Id,
            Text       = c.Text,
            IsCorrect  = c.IsCorrect,
            OrderIndex = c.OrderIndex
        }).ToList();

        // Atomic TotalPoints delta update
        int delta = request.Points - oldPoints;
        if (delta != 0)
        {
            await _db.Database.ExecuteSqlRawAsync(
                "UPDATE \"Exams\" SET \"TotalPoints\" = \"TotalPoints\" + {0}, \"UpdatedAt\" = {1} WHERE \"Id\" = {2}",
                delta, DateTime.UtcNow, exam.Id);
        }

        await _db.SaveChangesAsync(cancellationToken);

        return EndpointResponse<object>.SuccessResponse(null, "Question updated successfully.");
    }
}
