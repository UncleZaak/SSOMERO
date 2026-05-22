using AcademicSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicSystem.Infrastructure.Persistence.Configurations
{
    public class ProgrammeConfiguration : IEntityTypeConfiguration<Programme>
    {
        public void Configure(EntityTypeBuilder<Programme> builder)
        {
            builder.ToTable("Programmes");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();

            builder.Property(x => x.Name).IsRequired().HasMaxLength(256);
            builder.Property(x => x.Code).HasMaxLength(50);

            builder.HasOne(x => x.University)
                .WithMany(u => u.Programmes)
                .HasForeignKey(x => x.UniversityId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();
            builder.HasQueryFilter(p => !p.IsDeleted);
        }
    }
}
