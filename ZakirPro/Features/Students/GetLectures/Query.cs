using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Students.GetLectures;

public record Query(
    Guid StudentId,
    Guid TeacherId,
    Guid SubjectId,
    Guid StageId) : IRequest<EndpointResponse<List<LectureDto>>>;

public record LectureDto(
    Guid Id,
    string Title,
    string? Description,
    string GoogleDriveLink,
    DateTime CreatedAt);
