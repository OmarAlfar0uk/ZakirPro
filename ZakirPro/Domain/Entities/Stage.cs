namespace ZakirPro.Domain.Entities;

public class Stage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;

    /// <summary>Display order (e.g. Primary 1 = 1, Secondary 3 = 9).</summary>
    public int OrderIndex { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<TeacherSubjectStage> TeacherSubjectStages { get; set; } = [];
}
