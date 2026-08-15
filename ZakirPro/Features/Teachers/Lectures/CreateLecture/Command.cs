using System;
using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Teachers.Lectures.CreateLecture;

public record Command(string Title, string? Description, string GoogleDriveLink, Guid TeacherSubjectStageId) : IRequest<EndpointResponse<LectureDto>>;

public record LectureDto(Guid Id, string Title, string? Description, string GoogleDriveLink, Guid TeacherSubjectStageId, DateTime CreatedAt);
