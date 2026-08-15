using System;
using MediatR;
using ZakirPro.Common.Models;
using ZakirPro.Features.Teachers.Lectures.CreateLecture;

namespace ZakirPro.Features.Teachers.Lectures.UpdateLecture;

public record Command(Guid LectureId, string Title, string? Description, string GoogleDriveLink) : IRequest<EndpointResponse<LectureDto>>;
