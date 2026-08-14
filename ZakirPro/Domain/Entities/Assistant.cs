using ZakirPro.Domain.Enums;

namespace ZakirPro.Domain.Entities;

public class Assistant : User
{
    public Gender Gender { get; set; }
    public Guid TeacherId { get; set; }

    // Navigation
    public Teacher Teacher { get; set; } = null!;
}
