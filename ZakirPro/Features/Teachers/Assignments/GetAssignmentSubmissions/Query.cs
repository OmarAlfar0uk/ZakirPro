using System;
using System.Collections.Generic;
using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Teachers.Assignments.GetAssignmentSubmissions;

public record Query(Guid AssignmentId) : IRequest<EndpointResponse<AssignmentSubmissionsOverviewDto>>;

public record AssignmentSubmissionsOverviewDto(
    Guid                         AssignmentId,
    string                       Title,
    DateTime                     DueDate,
    decimal                      MaxScore,
    int                          TotalEnrolled,
    int                          SubmittedCount,
    int                          GradedCount,
    List<StudentSubmissionRosterDto> Submissions
);

public record StudentSubmissionRosterDto(
    Guid      StudentId,
    string    StudentFullName,
    string    StudentEmail,
    Guid?     SubmissionId,
    string    Status,
    string?   FilePath,
    decimal?  Score,
    decimal   MaxScore,
    string?   Feedback,
    DateTime? SubmittedAt,
    DateTime? GradedAt
);
