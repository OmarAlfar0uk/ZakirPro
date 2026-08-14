using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Lookups.Stages;

public record CreateStageCommand(string Name, int OrderIndex) : IRequest<EndpointResponse<StageDto>>;

public record UpdateStageCommand(Guid Id, string Name, int OrderIndex) : IRequest<EndpointResponse<StageDto>>;

public record DeleteStageCommand(Guid Id) : IRequest<EndpointResponse<string>>;

public record GetStagesQuery(int Page = 1, int PageSize = 50)
    : IRequest<EndpointResponse<PaginatedResult<StageDto>>>;

public record StageDto(Guid Id, string Name, int OrderIndex, DateTime CreatedAt);
