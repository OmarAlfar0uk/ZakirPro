using System;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Domain.Entities;

/// <summary>
/// A student's submission for an assignment.
/// Unique constraint on (AssignmentId, StudentId).
/// </summary>
public class AssignmentSubmission
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid AssignmentId { get; set; }
    public Assignment Assignment { get; set; } = null!;

    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public string FilePath { get; set; } = string.Empty;

    public AssignmentSubmissionStatus Status { get; set; } = AssignmentSubmissionStatus.Submitted;

    public decimal? Score { get; set; }
    public decimal MaxScore { get; set; } = 100m;
    public string? Feedback { get; set; }

    public DateTime? SubmittedAt { get; set; }
    public DateTime? GradedAt { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
