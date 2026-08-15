namespace ZakirPro.Domain.Entities;

/// <summary>
/// Stores the student's saved answer for one question within an attempt.
/// One row per (AttemptId, QuestionId) — upserted on every autosave.
/// A stub row (all answer fields null) is created for every question when
/// the attempt is started, so the "answered / unanswered" count is always
/// accurate by counting non-null answer fields.
/// </summary>
public class StudentAnswer
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid AttemptId { get; set; }
    public StudentExamAttempt Attempt { get; set; } = null!;

    public Guid QuestionId { get; set; }
    public Question Question { get; set; } = null!;

    /// <summary>
    /// The choice the student selected. Null for Essay questions or
    /// unanswered SingleChoice/TrueFalse questions.
    /// Must belong to the same QuestionId — validated in the handler.
    /// </summary>
    public Guid? SelectedChoiceId { get; set; }
    public Choice? SelectedChoice { get; set; }

    /// <summary>Student's written response for Essay questions. Null for choice questions.</summary>
    public string? EssayResponse { get; set; }

    /// <summary>Whether the student has flagged this question for later review.</summary>
    public bool IsMarkedForReview { get; set; } = false;

    /// <summary>
    /// For SingleChoice/TrueFalse: set server-side on submission (true/false).
    /// For Essay: always null — IsCorrect is not applicable to partial-credit answers.
    /// </summary>
    public bool? IsCorrect { get; set; }

    /// <summary>
    /// For SingleChoice/TrueFalse: auto-filled on submission (Question.Points or 0).
    /// For Essay: filled by teacher during grading (0 … Question.Points).
    /// Null until graded.
    /// </summary>
    public int? PointsAwarded { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
