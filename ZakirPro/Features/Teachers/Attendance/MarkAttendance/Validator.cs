using FluentValidation;
using ZakirPro.Domain.Enums;
using System;

namespace ZakirPro.Features.Teachers.Attendance.MarkAttendance;

public class Validator : AbstractValidator<Command>
{
    public Validator()
    {
        RuleFor(x => x.LectureId).NotEmpty();
        RuleFor(x => x.Entries).NotEmpty();
        RuleForEach(x => x.Entries).ChildRules(entry => {
            entry.RuleFor(e => e.Status).Must(s => Enum.TryParse<AttendanceStatus>(s, true, out _))
                 .WithMessage("Invalid attendance status.");
        });
    }
}
