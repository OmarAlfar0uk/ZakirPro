using Microsoft.EntityFrameworkCore;
using ZakirPro.Domain.Entities;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ── User hierarchy (TPH) ──────────────────────────────────────────────────
    public DbSet<User>           Users        => Set<User>();
    public DbSet<Teacher>        Teachers     => Set<Teacher>();
    public DbSet<Student>        Students     => Set<Student>();
    public DbSet<Assistant>      Assistants   => Set<Assistant>();
    public DbSet<Admin>          Admins       => Set<Admin>();
    public DbSet<SuperAdminUser> SuperAdmins  => Set<SuperAdminUser>();

    // ── Lookups ───────────────────────────────────────────────────────────────
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<Stage>   Stages   => Set<Stage>();

    // ── Core domain ──────────────────────────────────────────────────────────
    public DbSet<TeacherSubjectStage>        TeacherSubjectStages        => Set<TeacherSubjectStage>();
    public DbSet<StudentTeacherSubjectStage> StudentTeacherSubjectStages => Set<StudentTeacherSubjectStage>();
    public DbSet<Lecture>                    Lectures                    => Set<Lecture>();
    public DbSet<AuditLog>                   AuditLogs                   => Set<AuditLog>();

    // ── Attendance ────────────────────────────────────────────────────────────
    public DbSet<Attendance> Attendances => Set<Attendance>();

    // ── Exam engine ───────────────────────────────────────────────────────────
    public DbSet<Exam>               Exams               => Set<Exam>();
    public DbSet<Question>           Questions           => Set<Question>();
    public DbSet<Choice>             Choices             => Set<Choice>();
    public DbSet<StudentExamAttempt> StudentExamAttempts => Set<StudentExamAttempt>();
    public DbSet<StudentAnswer>      StudentAnswers      => Set<StudentAnswer>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        // ── TPH discriminator ─────────────────────────────────────────────────
        mb.Entity<User>()
            .HasDiscriminator<string>("UserType")
            .HasValue<Teacher>       ("Teacher")
            .HasValue<Student>       ("Student")
            .HasValue<Assistant>     ("Assistant")
            .HasValue<Admin>         ("Admin")
            .HasValue<SuperAdminUser>("SuperAdmin");

        // ── Global soft-delete filters ────────────────────────────────────────
        mb.Entity<User>()               .HasQueryFilter(u => !u.IsDeleted);
        mb.Entity<Subject>()            .HasQueryFilter(s => !s.IsDeleted);
        mb.Entity<Stage>()              .HasQueryFilter(s => !s.IsDeleted);
        mb.Entity<Lecture>()            .HasQueryFilter(l => !l.IsDeleted);
        mb.Entity<Exam>()               .HasQueryFilter(e => !e.IsDeleted);
        mb.Entity<Question>()           .HasQueryFilter(q => !q.IsDeleted);
        mb.Entity<StudentExamAttempt>() .HasQueryFilter(a => !a.IsDeleted);

        // ── Unique index: Email (among non-deleted users only) ─────────────────
        mb.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        // ── Unique index: TeacherSubjectStage triple ──────────────────────────
        mb.Entity<TeacherSubjectStage>()
            .HasIndex(t => new { t.TeacherId, t.SubjectId, t.StageId })
            .IsUnique();

        // ── Unique index: StudentTeacherSubjectStage enrollment ───────────────
        mb.Entity<StudentTeacherSubjectStage>()
            .HasIndex(s => new { s.StudentId, s.TeacherSubjectStageId })
            .IsUnique();

        // ── Unique index: one attempt per student per exam ────────────────────
        mb.Entity<StudentExamAttempt>()
            .HasIndex(a => new { a.StudentId, a.ExamId })
            .IsUnique();

        // ── Unique index: one answer row per question per attempt ─────────────
        mb.Entity<StudentAnswer>()
            .HasIndex(sa => new { sa.AttemptId, sa.QuestionId })
            .IsUnique();

        // ── Unique index: one OrderIndex per question in an exam ──────────────
        mb.Entity<Question>()
            .HasIndex(q => new { q.ExamId, q.OrderIndex })
            .IsUnique();

        // ── Unique index: one attendance record per student per lecture ─────────
        mb.Entity<Attendance>()
            .HasIndex(a => new { a.StudentId, a.LectureId })
            .IsUnique();

        // ── Relationships ──────────────────────────────────────────────────────

        // Assistant → Teacher
        mb.Entity<Assistant>()
            .HasOne(a => a.Teacher)
            .WithMany(t => t.Assistants)
            .HasForeignKey(a => a.TeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        // TeacherSubjectStage → Teacher
        mb.Entity<TeacherSubjectStage>()
            .HasOne(ts => ts.Teacher)
            .WithMany(t => t.TeacherSubjectStages)
            .HasForeignKey(ts => ts.TeacherId)
            .OnDelete(DeleteBehavior.Cascade);

        // TeacherSubjectStage → Subject
        mb.Entity<TeacherSubjectStage>()
            .HasOne(ts => ts.Subject)
            .WithMany(s => s.TeacherSubjectStages)
            .HasForeignKey(ts => ts.SubjectId)
            .OnDelete(DeleteBehavior.Restrict);

        // TeacherSubjectStage → Stage
        mb.Entity<TeacherSubjectStage>()
            .HasOne(ts => ts.Stage)
            .WithMany(s => s.TeacherSubjectStages)
            .HasForeignKey(ts => ts.StageId)
            .OnDelete(DeleteBehavior.Restrict);

        // StudentTeacherSubjectStage → Student
        mb.Entity<StudentTeacherSubjectStage>()
            .HasOne(s => s.Student)
            .WithMany(st => st.StudentTeacherLinks)
            .HasForeignKey(s => s.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        // StudentTeacherSubjectStage → TeacherSubjectStage
        mb.Entity<StudentTeacherSubjectStage>()
            .HasOne(s => s.TeacherSubjectStage)
            .WithMany(ts => ts.StudentLinks)
            .HasForeignKey(s => s.TeacherSubjectStageId)
            .OnDelete(DeleteBehavior.Cascade);

        // Lecture → TeacherSubjectStage
        mb.Entity<Lecture>()
            .HasOne(l => l.TeacherSubjectStage)
            .WithMany(ts => ts.Lectures)
            .HasForeignKey(l => l.TeacherSubjectStageId)
            .OnDelete(DeleteBehavior.Cascade);

        // Exam → TeacherSubjectStage
        mb.Entity<Exam>()
            .HasOne(e => e.TeacherSubjectStage)
            .WithMany(ts => ts.Exams)
            .HasForeignKey(e => e.TeacherSubjectStageId)
            .OnDelete(DeleteBehavior.Cascade);

        // Question → Exam
        mb.Entity<Question>()
            .HasOne(q => q.Exam)
            .WithMany(e => e.Questions)
            .HasForeignKey(q => q.ExamId)
            .OnDelete(DeleteBehavior.Cascade);

        // Choice → Question (hard-delete owned, so Cascade)
        mb.Entity<Choice>()
            .HasOne(c => c.Question)
            .WithMany(q => q.Choices)
            .HasForeignKey(c => c.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        // StudentExamAttempt → Student
        mb.Entity<StudentExamAttempt>()
            .HasOne(a => a.Student)
            .WithMany()
            .HasForeignKey(a => a.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        // StudentExamAttempt → Exam
        mb.Entity<StudentExamAttempt>()
            .HasOne(a => a.Exam)
            .WithMany(e => e.Attempts)
            .HasForeignKey(a => a.ExamId)
            .OnDelete(DeleteBehavior.Cascade);

        // StudentAnswer → StudentExamAttempt
        mb.Entity<StudentAnswer>()
            .HasOne(sa => sa.Attempt)
            .WithMany(a => a.Answers)
            .HasForeignKey(sa => sa.AttemptId)
            .OnDelete(DeleteBehavior.Cascade);

        // StudentAnswer → Question (Restrict — question soft-delete shouldn't wipe answers)
        mb.Entity<StudentAnswer>()
            .HasOne(sa => sa.Question)
            .WithMany(q => q.Answers)
            .HasForeignKey(sa => sa.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        // StudentAnswer → Choice (optional FK — Restrict)
        mb.Entity<StudentAnswer>()
            .HasOne(sa => sa.SelectedChoice)
            .WithMany()
            .HasForeignKey(sa => sa.SelectedChoiceId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // Attendance → Student
        mb.Entity<Attendance>()
            .HasOne(a => a.Student)
            .WithMany()
            .HasForeignKey(a => a.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Attendance → Lecture
        mb.Entity<Attendance>()
            .HasOne(a => a.Lecture)
            .WithMany(l => l.Attendances)
            .HasForeignKey(a => a.LectureId)
            .OnDelete(DeleteBehavior.Cascade);

        // ── Property constraints ───────────────────────────────────────────────
        mb.Entity<User>().Property(u => u.Email).HasMaxLength(256);
        mb.Entity<User>().Property(u => u.FullName).HasMaxLength(200);

        mb.Entity<Exam>().Property(e => e.Title).HasMaxLength(200);
        mb.Entity<Exam>().Property(e => e.PassThresholdPercent).HasPrecision(5, 2);

        mb.Entity<Question>().Property(q => q.Text).HasMaxLength(2000);

        mb.Entity<Choice>().Property(c => c.Text).HasMaxLength(500);

        mb.Entity<StudentAnswer>().Property(sa => sa.EssayResponse).HasMaxLength(10000);
    }
}
