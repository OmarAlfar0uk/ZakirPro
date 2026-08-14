using Microsoft.EntityFrameworkCore;
using ZakirPro.Domain.Entities;
using ZakirPro.Domain.Enums;

namespace ZakirPro.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ── User hierarchy (TPH) ──────────────────────────────────────────────────
    public DbSet<User>            Users            => Set<User>();
    public DbSet<Teacher>         Teachers         => Set<Teacher>();
    public DbSet<Student>         Students         => Set<Student>();
    public DbSet<Assistant>       Assistants       => Set<Assistant>();
    public DbSet<Admin>           Admins           => Set<Admin>();
    public DbSet<SuperAdminUser>  SuperAdmins      => Set<SuperAdminUser>();

    // ── Lookups ───────────────────────────────────────────────────────────────
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<Stage>   Stages   => Set<Stage>();

    // ── Core domain ──────────────────────────────────────────────────────────
    public DbSet<TeacherSubjectStage>        TeacherSubjectStages        => Set<TeacherSubjectStage>();
    public DbSet<StudentTeacherSubjectStage> StudentTeacherSubjectStages => Set<StudentTeacherSubjectStage>();
    public DbSet<Lecture>                    Lectures                    => Set<Lecture>();
    public DbSet<AuditLog>                   AuditLogs                   => Set<AuditLog>();

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
        mb.Entity<User>()    .HasQueryFilter(u => !u.IsDeleted);
        mb.Entity<Subject>() .HasQueryFilter(s => !s.IsDeleted);
        mb.Entity<Stage>()   .HasQueryFilter(s => !s.IsDeleted);
        mb.Entity<Lecture>() .HasQueryFilter(l => !l.IsDeleted);

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

        // ── Property constraints ───────────────────────────────────────────────
        mb.Entity<User>().Property(u => u.Email).HasMaxLength(256);
        mb.Entity<User>().Property(u => u.FullName).HasMaxLength(200);
    }
}
