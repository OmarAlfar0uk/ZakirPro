using System;
using System.Collections.Generic;

namespace ZakirPro.Domain.Entities;

/// <summary>
/// An assignment created by a teacher under a specific TeacherSubjectStage.
/// </summary>
public class Assignment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DueDate { get; set; }
    public decimal MaxScore { get; set; } = 100m;

    public Guid TeacherSubjectStageId { get; set; }
    public TeacherSubjectStage TeacherSubjectStage { get; set; } = null!;

    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<AssignmentSubmission> Submissions { get; set; } = [];
}
