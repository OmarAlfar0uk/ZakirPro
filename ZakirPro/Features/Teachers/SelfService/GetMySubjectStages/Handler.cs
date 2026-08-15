using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Domain.Entities;
using ZakirPro.Data;

namespace ZakirPro.Features.Teachers.SelfService.GetMySubjectStages;

public class Handler : IRequestHandler<Query, EndpointResponse<List<TssDto>>>
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public Handler(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<EndpointResponse<List<TssDto>>> Handle(Query request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId != request.TeacherId)
            return EndpointResponse<List<TssDto>>.ForbiddenResponse("You are not authorized.");

        var list = await _db.TeacherSubjectStages
            .Include(t => t.Subject)
            .Include(t => t.Stage)
            .Where(t => t.TeacherId == request.TeacherId)
            .Select(t => new TssDto(
                t.Id,
                t.SubjectId,
                t.Subject.Name,
                t.StageId,
                t.Stage.Name,
                t.StudentLinks.Count,
                t.Lectures.Count
            ))
            .ToListAsync(cancellationToken);

        return EndpointResponse<List<TssDto>>.SuccessResponse(list);
    }
}
