using ZakirPro.Domain.Enums;

namespace ZakirPro.Domain.Entities;

/// <summary>
/// Records attendance for one student at one lecture.
/// Unique constraint on (StudentId, LectureId) — upsert semantics on marking.
/// </summary>
public class Attendance
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public Guid LectureId { get; set; }
    public Lecture Lecture { get; set; } = null!;

    public AttendanceStatus Status { get; set; }

    /// <summary>Id of the Teacher or Assistant who marked this record.</summary>
    public Guid MarkedByUserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
