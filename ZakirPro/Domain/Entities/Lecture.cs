namespace ZakirPro.Domain.Entities;

public class Lecture
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>Google Drive share URL — video is never stored on the server.</summary>
    public string GoogleDriveLink { get; set; } = string.Empty;

    public Guid TeacherSubjectStageId { get; set; }
    public TeacherSubjectStage TeacherSubjectStage { get; set; } = null!;

    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
