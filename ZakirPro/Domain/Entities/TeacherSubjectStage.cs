namespace ZakirPro.Domain.Entities;

/// <summary>
/// Represents a teacher's assignment to teach a specific subject at a specific stage.
/// Unique constraint on (TeacherId, SubjectId, StageId).
/// </summary>
public class TeacherSubjectStage
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TeacherId { get; set; }
    public Teacher Teacher { get; set; } = null!;

    public Guid SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;

    public Guid StageId { get; set; }
    public Stage Stage { get; set; } = null!;

    // Navigation
    public ICollection<Lecture> Lectures { get; set; } = [];
    public ICollection<StudentTeacherSubjectStage> StudentLinks { get; set; } = [];
}
