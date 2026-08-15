using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Domain.Entities;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.Teachers.Exams.UpdateExam;

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
                "You are not authorized to edit exams on behalf of another teacher.");

        var exam = await _uow.GetRepository<Exam>()
            .Query()
            .Include(e => e.TeacherSubjectStage)
            .FirstOrDefaultAsync(e => e.Id == request.ExamId, cancellationToken);

        if (exam is null)
            return EndpointResponse<object>.NotFoundResponse("Exam not found.");

        if (exam.TeacherSubjectStage.TeacherId != request.TeacherId)
            return EndpointResponse<object>.ForbiddenResponse(
                "This exam does not belong to you.");

        // Archived exams may not be edited at all
        if (exam.Status == ExamStatus.Archived)
            return EndpointResponse<object>.ErrorResponse(
                "Archived exams cannot be edited.");

        // Apply metadata patch
        if (request.Title is not null)
            exam.Title = request.Title;

        if (request.Description is not null)
            exam.Description = request.Description;

        if (request.ScheduledEnd.HasValue)
        {
            // New end must still be after current start
            if (request.ScheduledEnd.Value <= exam.ScheduledStart)
                return EndpointResponse<object>.ErrorResponse(
                    "ScheduledEnd must be after ScheduledStart.");

            // Per design decision 7.2: existing InProgress DeadlineAt values are
            // NOT updated — only future attempts benefit from the extended window.
            exam.ScheduledEnd = request.ScheduledEnd.Value;
        }

        exam.UpdatedAt = DateTime.UtcNow;
        _uow.GetRepository<Exam>().Update(exam);
        await _uow.SaveChangesAsync(cancellationToken);

        return EndpointResponse<object>.SuccessResponse(null, "Exam updated successfully.");
    }
}
