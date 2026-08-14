using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Students.GetTeacherCards;

public record Query(Guid StudentId) : IRequest<EndpointResponse<List<TeacherCardDto>>>;

public record TeacherCardDto(
    Guid TeacherId,
    string TeacherName,
    string Subject,
    string Stage,
    int NumberOfStudents);
