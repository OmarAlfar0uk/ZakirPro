using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Domain.Entities;

namespace ZakirPro.Features.Lookups.Subjects;

// ─────────────────────────────────────────────────────────────────────────────
// Create
// ─────────────────────────────────────────────────────────────────────────────
public class CreateSubjectHandler : IRequestHandler<CreateSubjectCommand, EndpointResponse<SubjectDto>>
{
    private readonly IUnitOfWork _uow;

    public CreateSubjectHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<EndpointResponse<SubjectDto>> Handle(
        CreateSubjectCommand request,
        CancellationToken cancellationToken)
    {
        // Uniqueness check (only against non-deleted subjects via default filter)
        var exists = await _uow.GetRepository<Subject>()
            .ExistsAsync(s => s.Name == request.Name);

        if (exists)
            return EndpointResponse<SubjectDto>.ErrorResponse(
                $"A subject with the name '{request.Name}' already exists.");

        var subject = new Subject
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            CreatedAt = DateTime.UtcNow
        };

        await _uow.GetRepository<Subject>().AddAsync(subject);
        await _uow.SaveChangesAsync();

        return EndpointResponse<SubjectDto>.SuccessResponse(
            SubjectMapper.ToDto(subject),
            "Subject created successfully.");
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Update
// ─────────────────────────────────────────────────────────────────────────────
public class UpdateSubjectHandler : IRequestHandler<UpdateSubjectCommand, EndpointResponse<SubjectDto>>
{
    private readonly IUnitOfWork _uow;

    public UpdateSubjectHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<EndpointResponse<SubjectDto>> Handle(
        UpdateSubjectCommand request,
        CancellationToken cancellationToken)
    {
        var subject = await _uow.GetRepository<Subject>().GetByIdAsync(request.Id);
        if (subject is null)
            return EndpointResponse<SubjectDto>.NotFoundResponse(
                $"Subject with ID '{request.Id}' was not found.");

        // Uniqueness check against other subjects
        var nameConflict = await _uow.GetRepository<Subject>()
            .ExistsAsync(s => s.Name == request.Name && s.Id != request.Id);

        if (nameConflict)
            return EndpointResponse<SubjectDto>.ErrorResponse(
                $"A subject with the name '{request.Name}' already exists.");

        subject.Name = request.Name;
        _uow.GetRepository<Subject>().Update(subject);
        await _uow.SaveChangesAsync();

        return EndpointResponse<SubjectDto>.SuccessResponse(
            SubjectMapper.ToDto(subject),
            "Subject updated successfully.");
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Delete
// ─────────────────────────────────────────────────────────────────────────────
public class DeleteSubjectHandler : IRequestHandler<DeleteSubjectCommand, EndpointResponse<string>>
{
    private readonly IUnitOfWork _uow;

    public DeleteSubjectHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<EndpointResponse<string>> Handle(
        DeleteSubjectCommand request,
        CancellationToken cancellationToken)
    {
        var subject = await _uow.GetRepository<Subject>().GetByIdAsync(request.Id);
        if (subject is null)
            return EndpointResponse<string>.NotFoundResponse(
                $"Subject with ID '{request.Id}' was not found.");

        _uow.GetRepository<Subject>().SoftDelete(subject);
        await _uow.SaveChangesAsync();

        return EndpointResponse<string>.SuccessResponse(
            subject.Id.ToString(),
            "Subject deleted successfully.");
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Get (paginated)
// ─────────────────────────────────────────────────────────────────────────────
public class GetSubjectsHandler : IRequestHandler<GetSubjectsQuery, EndpointResponse<PaginatedResult<SubjectDto>>>
{
    private readonly IUnitOfWork _uow;

    public GetSubjectsHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<EndpointResponse<PaginatedResult<SubjectDto>>> Handle(
        GetSubjectsQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 50 : request.PageSize;

        var query = _uow.GetRepository<Subject>()
            .Query()
            .OrderBy(s => s.Name);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new SubjectDto(s.Id, s.Name, s.CreatedAt))
            .ToListAsync(cancellationToken);

        var result = new PaginatedResult<SubjectDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };

        return EndpointResponse<PaginatedResult<SubjectDto>>.SuccessResponse(
            result,
            "Subjects retrieved successfully.");
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Shared file-scoped mapper
// ─────────────────────────────────────────────────────────────────────────────
file static class SubjectMapper
{
    internal static SubjectDto ToDto(Subject s) => new(s.Id, s.Name, s.CreatedAt);
}
