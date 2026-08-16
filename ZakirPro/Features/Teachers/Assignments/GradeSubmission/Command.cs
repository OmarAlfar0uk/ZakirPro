using System;
using MediatR;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Teachers.Assignments.GradeSubmission;

public record Command(
    Guid    AssignmentId,
    Guid    SubmissionId,
    decimal Score,
    string? Feedback
) : IRequest<EndpointResponse<GradedSubmissionDto>>;

public record GradedSubmissionDto(
    Guid      SubmissionId,
    Guid      AssignmentId,
    Guid      StudentId,
    decimal   Score,
    decimal   MaxScore,
    string?   Feedback,
    string    Status,
    DateTime? GradedAt
);
