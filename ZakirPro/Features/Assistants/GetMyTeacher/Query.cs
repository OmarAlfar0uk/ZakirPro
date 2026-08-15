using System;
using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Assistants.GetMyTeacher;

public record Query(Guid AssistantId) : IRequest<EndpointResponse<TeacherProfileDto>>;

public record TeacherProfileDto(Guid Id, string FullName, string Email, string Gender, string? ProfileImagePath);
