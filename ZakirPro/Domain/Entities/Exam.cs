using ZakirPro.Domain.Enums;

namespace ZakirPro.Domain.Entities;

/// <summary>
/// An exam authored by a teacher for a specific TeacherSubjectStage.
/// Scoped to a scheduled window; each student gets a per-attempt countdown timer.
/// </summary>
public class Exam
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>The Teacher+Subject+Stage this exam belongs to.</summary>
    public Guid TeacherSubjectStageId { get; set; }
    public TeacherSubjectStage TeacherSubjectStage { get; set; } = null!;

    /// <summary>UTC datetime after which students may start the exam.</summary>
    public DateTime ScheduledStart { get; set; }

    /// <summary>UTC datetime after which no new attempts may be started.</summary>
    public DateTime ScheduledEnd { get; set; }

    /// <summary>
    /// Per-student countdown timer in minutes, measured from their personal StartedAt.
    /// DeadlineAt = MIN(StartedAt + DurationMinutes, ScheduledEnd).
    /// </summary>
    public int DurationMinutes { get; set; }

    /// <summary>
    /// Minimum percentage of TotalPoints a student must earn to pass (e.g. 50.00).
    /// </summary>
    public decimal PassThresholdPercent { get; set; }

    /// <summary>
    /// Denormalised sum of all Question.Points. Kept in sync via atomic DB-level updates
    /// when questions are added, edited, or deleted.
    /// </summary>
    public int TotalPoints { get; set; } = 0;

    public ExamStatus Status { get; set; } = ExamStatus.Draft;

    /// <summary>Reserved for future use — always false in the current phase.</summary>
    public bool AllowRetakes { get; set; } = false;

    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<Question>           Questions { get; set; } = [];
    public ICollection<StudentExamAttempt> Attempts  { get; set; } = [];
}
