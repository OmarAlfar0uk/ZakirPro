using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Data;
using ZakirPro.Domain.Entities;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.Teachers.Exams.DeleteQuestion;

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
                "You are not authorized to delete questions on behalf of another teacher.");

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
                "Questions can only be deleted from Draft exams.");

        var question = await _db.Questions
            .FirstOrDefaultAsync(
                q => q.Id == request.QuestionId && q.ExamId == request.ExamId,
                cancellationToken);

        if (question is null)
            return EndpointResponse<object>.NotFoundResponse("Question not found in this exam.");

        int pointsToRemove = question.Points;

        // Soft-delete the question (choices cascade-delete via DB constraint on hard delete,
        // but because this is soft-delete we simply mark the question; choices remain but
        // the global query filter on Question hides them from all endpoints)
        _uow.GetRepository<Question>().SoftDelete(question);

        // Atomic TotalPoints decrement
        await _db.Database.ExecuteSqlRawAsync(
            "UPDATE \"Exams\" SET \"TotalPoints\" = \"TotalPoints\" - {0}, \"UpdatedAt\" = {1} WHERE \"Id\" = {2}",
            pointsToRemove, DateTime.UtcNow, exam.Id);

        await _uow.SaveChangesAsync(cancellationToken);

        return EndpointResponse<object>.SuccessResponse(null, "Question deleted successfully.");
    }
}
