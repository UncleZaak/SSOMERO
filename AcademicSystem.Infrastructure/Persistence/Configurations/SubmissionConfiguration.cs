using AcademicSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicSystem.Infrastructure.Persistence.Configurations
{
    public class SubmissionConfiguration : IEntityTypeConfiguration<Submission>
    {
        public void Configure(EntityTypeBuilder<Submission> builder)
        {
            builder.ToTable("Submissions");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();

            builder.Property(x => x.FileName).IsRequired().HasMaxLength(512);
            builder.Property(x => x.ContentType).IsRequired().HasMaxLength(256);
            builder.Property(x => x.FileSize).IsRequired();
            builder.Property(x => x.StoragePath).IsRequired().HasMaxLength(1000);

            builder.HasOne(x => x.Assessment)
                .WithMany(a => a.Submissions)
                .HasForeignKey(x => x.AssessmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Student)
                .WithMany(u => u.Submissions)
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.AssessmentId, x.StudentId });

            builder.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();
            builder.HasQueryFilter(s => !s.IsDeleted);
        }
    }
}
