using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Domain.Entities;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.Teachers.Exams.PublishExam;

public class Handler : IRequestHandler<Command, EndpointResponse<object>>
{
    private readonly IUnitOfWork        _uow;
    private readonly ICurrentUserService _currentUser;

    public Handler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow         = uow;
        _currentUser = currentUser;
    }

    public async Task<EndpointResponse<object>> Handle(
        Command request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId != request.TeacherId)
            return EndpointResponse<object>.ForbiddenResponse(
                "You are not authorized to publish exams on behalf of another teacher.");

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
                $"Only Draft exams can be published. Current status: {exam.Status}.");

        if (exam.TotalPoints <= 0)
            return EndpointResponse<object>.ErrorResponse(
                "Exam must have at least one question with points before it can be published.");

        if (exam.ScheduledStart <= DateTime.UtcNow)
            return EndpointResponse<object>.ErrorResponse(
                "ScheduledStart must be in the future before publishing.");

        // Verify at least one non-deleted question exists
        var hasQuestions = await _uow.GetRepository<Question>()
            .ExistsAsync(q => q.ExamId == exam.Id);

        if (!hasQuestions)
            return EndpointResponse<object>.ErrorResponse(
                "Exam must contain at least one question before it can be published.");

        exam.Status    = ExamStatus.Published;
        exam.UpdatedAt = DateTime.UtcNow;
        _uow.GetRepository<Exam>().Update(exam);
        await _uow.SaveChangesAsync(cancellationToken);

        return EndpointResponse<object>.SuccessResponse(null, "Exam published successfully.");
    }
}
