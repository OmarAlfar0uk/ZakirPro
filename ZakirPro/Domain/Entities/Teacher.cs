using ZakirPro.Domain.Enums;

namespace ZakirPro.Domain.Entities;

public class Teacher : User
{
    public Gender Gender { get; set; }
    public string? ProfileImagePath { get; set; }

    // Navigation
    public ICollection<TeacherSubjectStage> TeacherSubjectStages { get; set; } = [];
    public ICollection<Assistant> Assistants { get; set; } = [];
}
