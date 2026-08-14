using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Lookups.Subjects;

public record CreateSubjectCommand(string Name) : IRequest<EndpointResponse<SubjectDto>>;

public record UpdateSubjectCommand(Guid Id, string Name) : IRequest<EndpointResponse<SubjectDto>>;

public record DeleteSubjectCommand(Guid Id) : IRequest<EndpointResponse<string>>;

public record GetSubjectsQuery(int Page = 1, int PageSize = 50)
    : IRequest<EndpointResponse<PaginatedResult<SubjectDto>>>;

public record SubjectDto(Guid Id, string Name, DateTime CreatedAt);
