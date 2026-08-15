using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Domain.Entities;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Features.Teachers.Exams.ArchiveExam;

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
                "You are not authorized to archive exams on behalf of another teacher.");

        var exam = await _uow.GetRepository<Exam>()
            .Query()
            .Include(e => e.TeacherSubjectStage)
            .FirstOrDefaultAsync(e => e.Id == request.ExamId, cancellationToken);

        if (exam is null)
            return EndpointResponse<object>.NotFoundResponse("Exam not found.");

        if (exam.TeacherSubjectStage.TeacherId != request.TeacherId)
            return EndpointResponse<object>.ForbiddenResponse("This exam does not belong to you.");

        if (exam.Status == ExamStatus.Archived)
            return EndpointResponse<object>.ErrorResponse("Exam is already archived.");

        exam.Status    = ExamStatus.Archived;
        exam.UpdatedAt = DateTime.UtcNow;
        _uow.GetRepository<Exam>().Update(exam);
        await _uow.SaveChangesAsync(cancellationToken);

        return EndpointResponse<object>.SuccessResponse(null, "Exam archived successfully.");
    }
}
