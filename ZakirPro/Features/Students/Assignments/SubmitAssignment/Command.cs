using System;
using MediatR;
using Microsoft.AspNetCore.Http;
using ZakirPro.Common.Models;

namespace ZakirPro.Features.Students.Assignments.SubmitAssignment;

public record Command(
    Guid      StudentId,
    Guid      AssignmentId,
    IFormFile File
) : IRequest<EndpointResponse<StudentSubmissionDto>>;

public record StudentSubmissionDto(
    Guid      SubmissionId,
    Guid      AssignmentId,
    Guid      StudentId,
    string    FilePath,
    string    Status,
    decimal?  Score,
    decimal   MaxScore,
    string?   Feedback,
    DateTime? SubmittedAt,
    DateTime? GradedAt
);
