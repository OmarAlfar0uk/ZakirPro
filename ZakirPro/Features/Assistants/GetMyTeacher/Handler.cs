using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Domain.Entities;
using ZakirPro.Data;

namespace ZakirPro.Features.Assistants.GetMyTeacher;

public class Handler : IRequestHandler<Query, EndpointResponse<TeacherProfileDto>>
{
    private readonly AppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public Handler(AppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<EndpointResponse<TeacherProfileDto>> Handle(Query request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId != request.AssistantId)
            return EndpointResponse<TeacherProfileDto>.ForbiddenResponse("You are not authorized.");

        var assistant = await _db.Assistants
            .Include(a => a.Teacher)
            .FirstOrDefaultAsync(a => a.Id == request.AssistantId, cancellationToken);

        if (assistant == null) return EndpointResponse<TeacherProfileDto>.NotFoundResponse("Assistant not found.");

        var dto = new TeacherProfileDto(
            assistant.Teacher.Id,
            assistant.Teacher.FullName,
            assistant.Teacher.Email,
            assistant.Teacher.Gender.ToString(),
            assistant.Teacher.ProfileImagePath
        );

        return EndpointResponse<TeacherProfileDto>.SuccessResponse(dto);
    }
}
