using System;
using System.Collections.Generic;
using MediatR;
using ZakirPro.Common.Models;
using ZakirPro.Features.Teachers.Lectures.CreateLecture;

namespace ZakirPro.Features.Teachers.Lectures.GetTeacherLectures;

public record Query(Guid? TeacherSubjectStageId) : IRequest<EndpointResponse<List<LectureDto>>>;
