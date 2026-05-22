using AcademicSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicSystem.Infrastructure.Persistence.Configurations
{
    public class CourseConfiguration : IEntityTypeConfiguration<Course>
    {
        public void Configure(EntityTypeBuilder<Course> builder)
        {
            builder.ToTable("Courses");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();

            builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
            builder.Property(x => x.Title).IsRequired().HasMaxLength(256);
            builder.Property(x => x.Description).HasMaxLength(4000);
            builder.Property(x => x.Credits);

            builder.HasOne(x => x.Programme)
                .WithMany(p => p.Courses)
                .HasForeignKey(x => x.ProgrammeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();
            builder.HasQueryFilter(c => !c.IsDeleted);
        }
    }
}
