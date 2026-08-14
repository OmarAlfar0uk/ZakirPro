namespace ZakirPro.Domain.Entities;

/// <summary>
/// Enrols a student in a specific Teacher+Subject+Stage combination.
/// Unique constraint on (StudentId, TeacherSubjectStageId).
/// </summary>
public class StudentTeacherSubjectStage
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public Guid TeacherSubjectStageId { get; set; }
    public TeacherSubjectStage TeacherSubjectStage { get; set; } = null!;
}
