using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Domain.Entities;

namespace ZakirPro.Features.Teachers.SelfService.GetMyStudents;

public class Handler : IRequestHandler<Query, EndpointResponse<PaginatedResult<StudentDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;

    public Handler(IUnitOfWork uow, ICurrentUserService currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<EndpointResponse<PaginatedResult<StudentDto>>> Handle(Query request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId != request.TeacherId)
            return EndpointResponse<PaginatedResult<StudentDto>>.ForbiddenResponse("You are not authorized.");

        var query = _uow.GetRepository<StudentTeacherSubjectStage>().Query()
            .Include(stss => stss.Student)
            .Include(stss => stss.TeacherSubjectStage).ThenInclude(tss => tss.Subject)
            .Include(stss => stss.TeacherSubjectStage).ThenInclude(tss => tss.Stage)
            .Where(stss => stss.TeacherSubjectStage.TeacherId == request.TeacherId);

        if (request.TeacherSubjectStageId.HasValue)
        {
            query = query.Where(stss => stss.TeacherSubjectStageId == request.TeacherSubjectStageId.Value);
        }

        if (!string.IsNullOrEmpty(request.Search))
        {
            var s = request.Search.ToLower();
            query = query.Where(stss => stss.Student.FullName.ToLower().Contains(s) || stss.Student.Email.ToLower().Contains(s));
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(stss => stss.Student.FullName)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(stss => new StudentDto(
                stss.StudentId,
                stss.Student.FullName,
                stss.Student.Email,
                stss.Student.IsActive,
                stss.TeacherSubjectStageId,
                stss.TeacherSubjectStage.Subject.Name,
                stss.TeacherSubjectStage.Stage.Name
            ))
            .ToListAsync(cancellationToken);

        return EndpointResponse<PaginatedResult<StudentDto>>.SuccessResponse(new PaginatedResult<StudentDto>(items, total, request.Page, request.PageSize));
    }
}
