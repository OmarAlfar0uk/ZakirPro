using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Domain.Entities;
using ZakirPro.Data;

namespace ZakirPro.Features.Teachers.SelfService.DeleteSubjectStage;

public class Handler : IRequestHandler<Command, EndpointResponse<bool>>
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public Handler(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<EndpointResponse<bool>> Handle(Command request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId != request.TeacherId)
            return EndpointResponse<bool>.ForbiddenResponse("You are not authorized.");

        var tss = await _db.TeacherSubjectStages.FirstOrDefaultAsync(t => t.Id == request.TssId, cancellationToken);
        if (tss == null)
            return EndpointResponse<bool>.NotFoundResponse("Assignment not found.");

        if (tss.TeacherId != request.TeacherId)
            return EndpointResponse<bool>.ForbiddenResponse("You do not own this assignment.");

        var activeStudentsCount = await _db.StudentTeacherSubjectStages.CountAsync(st => st.TeacherSubjectStageId == request.TssId, cancellationToken);
        if (activeStudentsCount > 0)
        {
            return EndpointResponse<bool>.ErrorResponse($"Cannot remove this assignment — {activeStudentsCount} student(s) are still enrolled. Remove students first.");
        }

        _db.TeacherSubjectStages.Remove(tss);
        await _db.SaveChangesAsync(cancellationToken);

        return EndpointResponse<bool>.SuccessResponse(true, "Assignment removed successfully.");
    }
}
