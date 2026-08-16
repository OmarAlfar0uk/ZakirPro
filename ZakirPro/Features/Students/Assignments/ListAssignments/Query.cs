using System;
using System.Collections.Generic;
using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Students.Assignments.ListAssignments;

public record Query(
    Guid  StudentId,
    Guid? TeacherSubjectStageId
) : IRequest<EndpointResponse<List<StudentAssignmentItemDto>>>;

public record StudentAssignmentItemDto(
    Guid      AssignmentId,
    string    Title,
    string?   Description,
    DateTime  DueDate,
    decimal   MaxScore,
    Guid      TeacherSubjectStageId,
    string    TeacherName,
    string    SubjectName,
    string    StageName,
    string    SubmissionStatus, // NotSubmitted, Submitted, Late, Graded
    Guid?     SubmissionId,
    decimal?  Score,
    string?   Feedback,
    DateTime? SubmittedAt,
    DateTime? GradedAt
);
