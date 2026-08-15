using ZakirPro.Domain.Enums;

namespace ZakirPro.Domain.Entities;

/// <summary>
/// A single question belonging to an exam.
/// Choice rows are only present for SingleChoice and TrueFalse types.
/// </summary>
public class Question
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ExamId { get; set; }
    public Exam Exam { get; set; } = null!;

    public string Text { get; set; } = string.Empty;

    public QuestionType Type { get; set; }

    /// <summary>Points awarded for a fully-correct answer (≥ 1).</summary>
    public int Points { get; set; }

    /// <summary>1-based display order within the exam ("Question X of Y").</summary>
    public int OrderIndex { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<Choice>        Choices { get; set; } = [];
    public ICollection<StudentAnswer> Answers { get; set; } = [];
}
