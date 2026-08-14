namespace ZakirPro.Domain.Entities;

public class Student : User
{
    /// <summary>Cryptographically random 8-char activation code sent via email.</summary>
    public string? ActivationCode { get; set; }
    public DateTime? ActivationCodeExpiry { get; set; }
    public bool IsActivated { get; set; } = false;

    // Navigation
    public ICollection<StudentTeacherSubjectStage> StudentTeacherLinks { get; set; } = [];
}
