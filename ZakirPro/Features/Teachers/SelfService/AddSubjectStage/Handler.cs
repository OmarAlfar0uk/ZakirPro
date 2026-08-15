using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Domain.Entities;
using ZakirPro.Data;

namespace ZakirPro.Features.Teachers.SelfService.AddSubjectStage;

public class Handler : IRequestHandler<Command, EndpointResponse<TssDto>>
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public Handler(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<EndpointResponse<TssDto>> Handle(Command request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId != request.TeacherId)
            return EndpointResponse<TssDto>.ForbiddenResponse("You are not authorized.");

        var exists = await _db.TeacherSubjectStages.AnyAsync(t => t.TeacherId == request.TeacherId && t.SubjectId == request.SubjectId && t.StageId == request.StageId, cancellationToken);
        if (exists)
            return EndpointResponse<TssDto>.ErrorResponse("This subject and stage assignment already exists.");

        var subject = await _db.Subjects.FindAsync(new object[] { request.SubjectId }, cancellationToken);
        if (subject == null) return EndpointResponse<TssDto>.NotFoundResponse("Subject not found.");

        var stage = await _db.Stages.FindAsync(new object[] { request.StageId }, cancellationToken);
        if (stage == null) return EndpointResponse<TssDto>.NotFoundResponse("Stage not found.");

        var tss = new TeacherSubjectStage
        {
            TeacherId = request.TeacherId,
            SubjectId = request.SubjectId,
            StageId = request.StageId
        };

        _db.TeacherSubjectStages.Add(tss);
        await _db.SaveChangesAsync(cancellationToken);

        var dto = new TssDto(tss.Id, tss.SubjectId, subject.Name, tss.StageId, stage.Name, 0, 0);
        return EndpointResponse<TssDto>.SuccessResponse(dto, "Subject stage added successfully.");
    }
}
