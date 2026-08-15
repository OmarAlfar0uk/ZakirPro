namespace ZakirPro.Domain.Enums;

public enum ExamStatus
{
    /// <summary>Exam is being authored — not visible to students.</summary>
    Draft = 0,

    /// <summary>Exam is live — students can start it within the scheduled window.</summary>
    Published = 1,

    /// <summary>Exam has been retired — no new attempts allowed.</summary>
    Archived = 2
}
