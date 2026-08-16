using System;
using System.Collections.Generic;
using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Teachers.Assignments.GetTeacherAssignments;

public record Query(Guid? TeacherSubjectStageId) : IRequest<EndpointResponse<List<TeacherAssignmentItemDto>>>;

public record TeacherAssignmentItemDto(
    Guid     Id,
    string   Title,
    string?  Description,
    DateTime DueDate,
    decimal  MaxScore,
    Guid     TeacherSubjectStageId,
    string   SubjectName,
    string   StageName,
    int      TotalSubmissions,
    int      GradedSubmissions,
    DateTime CreatedAt
);
