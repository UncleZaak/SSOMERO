using AcademicSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicSystem.Infrastructure.Persistence.Configurations
{
    public class AssessmentConfiguration : IEntityTypeConfiguration<Assessment>
    {
        public void Configure(EntityTypeBuilder<Assessment> builder)
        {
            builder.ToTable("Assessments");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();

            builder.Property(x => x.Title).IsRequired().HasMaxLength(256);
            builder.Property(x => x.Description).HasMaxLength(4000);
            builder.Property(x => x.MaxScore).HasPrecision(18, 2).IsRequired();

            builder.HasOne(x => x.ClassCourse)
                .WithMany(cc => cc.Assessments)
                .HasForeignKey(x => x.ClassCourseId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();
            builder.HasQueryFilter(a => !a.IsDeleted);
        }
    }
}
