namespace ZakirPro.Domain.Entities;

/// <summary>
/// A selectable answer option for a SingleChoice or TrueFalse question.
/// Essay questions have no Choice rows.
/// Choice rows are hard-deleted together with their owning Question.
/// </summary>
public class Choice
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid QuestionId { get; set; }
    public Question Question { get; set; } = null!;

    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is the correct answer. Meaningful only for
    /// SingleChoice and TrueFalse questions.
    /// NEVER exposed in payloads sent to students during an active attempt.
    /// </summary>
    public bool IsCorrect { get; set; }

    /// <summary>Display order within the question (A=1, B=2, C=3, D=4).</summary>
    public int OrderIndex { get; set; }
}
