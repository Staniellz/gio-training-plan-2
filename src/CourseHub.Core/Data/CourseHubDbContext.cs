using CourseHub.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace CourseHub.Core.Data;

public class CourseHubDbContext : DbContext
{
    public CourseHubDbContext(DbContextOptions<CourseHubDbContext> options) : base(options)
    {
    }

    public DbSet<Course> Courses => Set<Course>();
    public DbSet<CourseModule> Modules => Set<CourseModule>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Course>(b =>
        {
            b.Property(c => c.Title).HasMaxLength(200);
            b.HasMany(c => c.Modules).WithOne().HasForeignKey(m => m.CourseId);
            b.HasMany(c => c.Enrollments).WithOne(e => e.Course).HasForeignKey(e => e.CourseId);
        });

        modelBuilder.Entity<Enrollment>(b =>
        {
            // 0 is EnrollmentStatus.Active. Dropped rows stay, so a learner can enroll again.
            b.HasIndex(e => new { e.CourseId, e.LearnerId })
                .IsUnique()
                .HasFilter("\"Status\" = 0")
                .HasDatabaseName("IX_Enrollments_OneActivePerLearner");
        });
    }
}
