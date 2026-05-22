using AcademicSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicSystem.Infrastructure.Persistence.Configurations
{
    public class UniversityConfiguration : IEntityTypeConfiguration<University>
    {
        public void Configure(EntityTypeBuilder<University> builder)
        {
            builder.ToTable("Universities");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();

            builder.Property(x => x.Name).IsRequired().HasMaxLength(256);
            builder.Property(x => x.Code).HasMaxLength(50);

            builder.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();

            builder.HasQueryFilter(u => !u.IsDeleted);
        }
    }
}
