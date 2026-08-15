using ZakirPro.Domain.Enums;

namespace ZakirPro.Domain.Entities;

/// <summary>
/// Records a single student's attempt at an exam.
/// Unique constraint on (StudentId, ExamId) — no retakes in the current phase.
/// </summary>
public class StudentExamAttempt
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public Guid ExamId { get; set; }
    public Exam Exam { get; set; } = null!;

    /// <summary>UTC timestamp when the student clicked "Start". Set server-side.</summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// Server-enforced hard deadline: MIN(StartedAt + Exam.DurationMinutes, Exam.ScheduledEnd).
    /// Stored so it can be checked without a join.
    /// </summary>
    public DateTime DeadlineAt { get; set; }

    /// <summary>UTC timestamp when the attempt was submitted (null while InProgress).</summary>
    public DateTime? SubmittedAt { get; set; }

    public AttemptStatus AttemptStatus { get; set; } = AttemptStatus.InProgress;

    /// <summary>
    /// Sum of points for correctly-answered SingleChoice/TrueFalse questions.
    /// Set immediately on submission. Null before submission.
    /// </summary>
    public int? AutoGradedScore { get; set; }

    /// <summary>
    /// Sum of teacher-assigned points for Essay answers.
    /// Null until all Essay answers have been graded by the teacher.
    /// </summary>
    public int? EssayScore { get; set; }

    /// <summary>AutoGradedScore + EssayScore. Null until status reaches Graded.</summary>
    public int? FinalScore { get; set; }

    /// <summary>
    /// Snapshot of Exam.TotalPoints at the moment the attempt was started.
    /// Insulates the grade calculation from future exam edits.
    /// </summary>
    public int MaxScore { get; set; }

    /// <summary>Null until Graded. True if FinalScore / MaxScore ≥ PassThresholdPercent.</summary>
    public bool? IsPassed { get; set; }

    /// <summary>(SubmittedAt − StartedAt).TotalSeconds. Null until submitted.</summary>
    public int? TimeTakenSeconds { get; set; }

    /// <summary>
    /// Per-attempt free-text feedback from the teacher, shown on the results screen.
    /// </summary>
    public string? TeacherFeedback { get; set; }

    /// <summary>True when the server force-submitted the attempt after the deadline elapsed.</summary>
    public bool IsAutoSubmitted { get; set; } = false;

    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<StudentAnswer> Answers { get; set; } = [];
}
