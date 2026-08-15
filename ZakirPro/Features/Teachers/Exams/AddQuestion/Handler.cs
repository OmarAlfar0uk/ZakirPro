using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Data;
using ZakirPro.Domain.Entities;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.Teachers.Exams.AddQuestion;

public class Handler : IRequestHandler<Command, EndpointResponse<QuestionCreatedDto>>
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

    public async Task<EndpointResponse<QuestionCreatedDto>> Handle(
        Command request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId != request.TeacherId)
            return EndpointResponse<QuestionCreatedDto>.ForbiddenResponse(
                "You are not authorized to add questions on behalf of another teacher.");

        var exam = await _uow.GetRepository<Exam>()
            .Query()
            .Include(e => e.TeacherSubjectStage)
            .FirstOrDefaultAsync(e => e.Id == request.ExamId, cancellationToken);

        if (exam is null)
            return EndpointResponse<QuestionCreatedDto>.NotFoundResponse("Exam not found.");

        if (exam.TeacherSubjectStage.TeacherId != request.TeacherId)
            return EndpointResponse<QuestionCreatedDto>.ForbiddenResponse(
                "This exam does not belong to you.");

        if (exam.Status != ExamStatus.Draft)
            return EndpointResponse<QuestionCreatedDto>.ErrorResponse(
                "Questions can only be added to Draft exams.");

        var questionType = Enum.Parse<QuestionType>(request.Type, ignoreCase: true);

        // Determine OrderIndex: use supplied value or auto-append
        int orderIndex = request.OrderIndex ?? await GetNextOrderIndexAsync(exam.Id, cancellationToken);

        // Check for OrderIndex conflict
        var conflictExists = await _uow.GetRepository<Question>()
            .ExistsAsync(q => q.ExamId == exam.Id && q.OrderIndex == orderIndex);
        if (conflictExists)
            return EndpointResponse<QuestionCreatedDto>.ErrorResponse(
                $"A question with OrderIndex {orderIndex} already exists in this exam.");

        var question = new Question
        {
            ExamId     = exam.Id,
            Text       = request.Text,
            Type       = questionType,
            Points     = request.Points,
            OrderIndex = orderIndex
        };

        question.Choices = request.Choices.Select(c => new Choice
        {
            QuestionId = question.Id,
            Text       = c.Text,
            IsCorrect  = c.IsCorrect,
            OrderIndex = c.OrderIndex
        }).ToList();

        await _uow.GetRepository<Question>().AddAsync(question);

        // Atomic TotalPoints increment via raw SQL to prevent lost-update races (design decision 7.6)
        await _db.Database.ExecuteSqlRawAsync(
            "UPDATE \"Exams\" SET \"TotalPoints\" = \"TotalPoints\" + {0}, \"UpdatedAt\" = {1} WHERE \"Id\" = {2}",
            request.Points, DateTime.UtcNow, exam.Id);

        await _uow.SaveChangesAsync(cancellationToken);

        var dto = new QuestionCreatedDto(
            question.Id,
            question.Text,
            question.Type.ToString(),
            question.Points,
            question.OrderIndex,
            question.Choices.Select(c => new ChoiceDto(c.Id, c.Text, c.OrderIndex)).ToList());

        return EndpointResponse<QuestionCreatedDto>.SuccessResponse(dto, "Question added successfully.");
    }

    private async Task<int> GetNextOrderIndexAsync(Guid examId, CancellationToken ct)
    {
        var maxOrder = await _uow.GetRepository<Question>()
            .Query()
            .Where(q => q.ExamId == examId)
            .MaxAsync(q => (int?)q.OrderIndex, ct);
        return (maxOrder ?? 0) + 1;
    }
}
