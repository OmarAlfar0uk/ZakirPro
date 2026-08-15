using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Domain.Entities;

namespace ZakirPro.Features.Teachers.Exams.CreateExam;

public class Handler : IRequestHandler<Command, EndpointResponse<ExamCreatedDto>>
{
    private readonly IUnitOfWork        _uow;
    private readonly ICurrentUserService _currentUser;

    public Handler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow         = uow;
        _currentUser = currentUser;
    }

    public async Task<EndpointResponse<ExamCreatedDto>> Handle(
        Command request, CancellationToken cancellationToken)
    {
        // ── Ownership: caller must be the teacher ─────────────────────────────
        if (_currentUser.UserId != request.TeacherId)
            return EndpointResponse<ExamCreatedDto>.ForbiddenResponse(
                "You are not authorized to create exams on behalf of another teacher.");

        // ── Verify the TSS belongs to this teacher ────────────────────────────
        var tss = await _uow.GetRepository<TeacherSubjectStage>()
            .FirstOrDefaultAsync(t =>
                t.Id == request.TeacherSubjectStageId &&
                t.TeacherId == request.TeacherId);

        if (tss is null)
            return EndpointResponse<ExamCreatedDto>.NotFoundResponse(
                "TeacherSubjectStage not found or does not belong to you.");

        var exam = new Exam
        {
            Title                 = request.Title,
            Description           = request.Description,
            TeacherSubjectStageId = request.TeacherSubjectStageId,
            ScheduledStart        = request.ScheduledStart,
            ScheduledEnd          = request.ScheduledEnd,
            DurationMinutes       = request.DurationMinutes,
            PassThresholdPercent  = request.PassThresholdPercent
        };

        await _uow.GetRepository<Exam>().AddAsync(exam);
        await _uow.SaveChangesAsync(cancellationToken);

        return EndpointResponse<ExamCreatedDto>.SuccessResponse(
            new ExamCreatedDto(
                exam.Id,
                exam.Title,
                exam.Description,
                exam.TeacherSubjectStageId,
                exam.ScheduledStart,
                exam.ScheduledEnd,
                exam.DurationMinutes,
                exam.PassThresholdPercent,
                exam.TotalPoints,
                exam.Status.ToString(),
                exam.CreatedAt),
            "Exam created successfully.");
    }
}
