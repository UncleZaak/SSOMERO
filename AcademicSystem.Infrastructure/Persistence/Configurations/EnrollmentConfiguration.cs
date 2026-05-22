using AcademicSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicSystem.Infrastructure.Persistence.Configurations
{
    public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
    {
        public void Configure(EntityTypeBuilder<Enrollment> builder)
        {
            builder.ToTable("Enrollments");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();

            builder.Property(x => x.EnrolledAt).IsRequired();
            builder.Property(x => x.Status).IsRequired();

            builder.HasOne(x => x.Student)
                .WithMany(u => u.Enrollments)
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Class)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(x => x.ClassId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.StudentId, x.ClassId })
                .IsUnique()
                .HasDatabaseName("UX_Enrollments_Student_Class");

            builder.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();
            builder.HasQueryFilter(e => !e.IsDeleted);
        }
    }
}
