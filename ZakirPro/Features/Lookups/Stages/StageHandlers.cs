using MediatR;
using Microsoft.EntityFrameworkCore;
using ZakirPro.Common.Abstractions;
using ZakirPro.Common.Models;
using ZakirPro.Domain.Entities;

namespace ZakirPro.Features.Lookups.Stages;

// ─────────────────────────────────────────────────────────────────────────────
// Create
// ─────────────────────────────────────────────────────────────────────────────
public class CreateStageHandler : IRequestHandler<CreateStageCommand, EndpointResponse<StageDto>>
{
    private readonly IUnitOfWork _uow;

    public CreateStageHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<EndpointResponse<StageDto>> Handle(
        CreateStageCommand request,
        CancellationToken cancellationToken)
    {
        // Uniqueness check (only against non-deleted stages via default filter)
        var exists = await _uow.GetRepository<Stage>()
            .ExistsAsync(s => s.Name == request.Name);

        if (exists)
            return EndpointResponse<StageDto>.ErrorResponse(
                $"A stage with the name '{request.Name}' already exists.");

        var stage = new Stage
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            OrderIndex = request.OrderIndex,
            CreatedAt = DateTime.UtcNow
        };

        await _uow.GetRepository<Stage>().AddAsync(stage);
        await _uow.SaveChangesAsync();

        return EndpointResponse<StageDto>.SuccessResponse(
            StageMapper.ToDto(stage),
            "Stage created successfully.");
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Update
// ─────────────────────────────────────────────────────────────────────────────
public class UpdateStageHandler : IRequestHandler<UpdateStageCommand, EndpointResponse<StageDto>>
{
    private readonly IUnitOfWork _uow;

    public UpdateStageHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<EndpointResponse<StageDto>> Handle(
        UpdateStageCommand request,
        CancellationToken cancellationToken)
    {
        var stage = await _uow.GetRepository<Stage>().GetByIdAsync(request.Id);
        if (stage is null)
            return EndpointResponse<StageDto>.NotFoundResponse(
                $"Stage with ID '{request.Id}' was not found.");

        // Uniqueness check against other stages
        var nameConflict = await _uow.GetRepository<Stage>()
            .ExistsAsync(s => s.Name == request.Name && s.Id != request.Id);

        if (nameConflict)
            return EndpointResponse<StageDto>.ErrorResponse(
                $"A stage with the name '{request.Name}' already exists.");

        stage.Name = request.Name;
        stage.OrderIndex = request.OrderIndex;
        _uow.GetRepository<Stage>().Update(stage);
        await _uow.SaveChangesAsync();

        return EndpointResponse<StageDto>.SuccessResponse(
            StageMapper.ToDto(stage),
            "Stage updated successfully.");
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Delete
// ─────────────────────────────────────────────────────────────────────────────
public class DeleteStageHandler : IRequestHandler<DeleteStageCommand, EndpointResponse<string>>
{
    private readonly IUnitOfWork _uow;

    public DeleteStageHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<EndpointResponse<string>> Handle(
        DeleteStageCommand request,
        CancellationToken cancellationToken)
    {
        var stage = await _uow.GetRepository<Stage>().GetByIdAsync(request.Id);
        if (stage is null)
            return EndpointResponse<string>.NotFoundResponse(
                $"Stage with ID '{request.Id}' was not found.");

        _uow.GetRepository<Stage>().SoftDelete(stage);
        await _uow.SaveChangesAsync();

        return EndpointResponse<string>.SuccessResponse(
            stage.Id.ToString(),
            "Stage deleted successfully.");
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Get (paginated)
// ─────────────────────────────────────────────────────────────────────────────
public class GetStagesHandler : IRequestHandler<GetStagesQuery, EndpointResponse<PaginatedResult<StageDto>>>
{
    private readonly IUnitOfWork _uow;

    public GetStagesHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<EndpointResponse<PaginatedResult<StageDto>>> Handle(
        GetStagesQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 50 : request.PageSize;

        var query = _uow.GetRepository<Stage>()
            .Query()
            .OrderBy(s => s.OrderIndex);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new StageDto(s.Id, s.Name, s.OrderIndex, s.CreatedAt))
            .ToListAsync(cancellationToken);

        var result = new PaginatedResult<StageDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };

        return EndpointResponse<PaginatedResult<StageDto>>.SuccessResponse(
            result,
            "Stages retrieved successfully.");
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// Shared mapper (file-scoped)
// ─────────────────────────────────────────────────────────────────────────────
file static class StageMapper
{
    internal static StageDto ToDto(Stage s) => new(s.Id, s.Name, s.OrderIndex, s.CreatedAt);
}
