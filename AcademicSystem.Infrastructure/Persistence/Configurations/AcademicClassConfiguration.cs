using AcademicSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicSystem.Infrastructure.Persistence.Configurations
{
    public class AcademicClassConfiguration : IEntityTypeConfiguration<AcademicClass>
    {
        public void Configure(EntityTypeBuilder<AcademicClass> builder)
        {
            builder.ToTable("AcademicClasses");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();

            builder.Property(x => x.Name).IsRequired().HasMaxLength(128);
            builder.Property(x => x.AcademicYear).IsRequired().HasMaxLength(64);
            builder.Property(x => x.Semester).IsRequired().HasMaxLength(64);

            builder.HasOne(x => x.Programme)
                .WithMany(p => p.AcademicClasses)
                .HasForeignKey(x => x.ProgrammeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();
            builder.HasQueryFilter(c => !c.IsDeleted);
        }
    }
}
