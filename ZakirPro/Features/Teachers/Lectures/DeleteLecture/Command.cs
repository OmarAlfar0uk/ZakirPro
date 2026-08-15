using System;
using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Teachers.Lectures.DeleteLecture;

public record Command(Guid LectureId) : IRequest<EndpointResponse<bool>>;
