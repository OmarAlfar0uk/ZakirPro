namespace ZakirPro.Domain.Enums;

public enum AttemptStatus
{
    /// <summary>Student has started but not submitted.</summary>
    InProgress = 0,

    /// <summary>
    /// Submitted and fully auto-graded — only reached when the exam has
    /// no Essay questions, making the attempt immediately Graded.
    /// This intermediate value is effectively skipped in the no-essay path
    /// but kept for future extensibility.
    /// </summary>
    Submitted = 1,

    /// <summary>
    /// Submitted but contains Essay questions awaiting teacher grading.
    /// Auto-graded score is available; final score is not yet set.
    /// </summary>
    PendingGrading = 2,

    /// <summary>All answers graded — FinalScore, IsPassed, TimeTakenSeconds are set.</summary>
    Graded = 3
}
