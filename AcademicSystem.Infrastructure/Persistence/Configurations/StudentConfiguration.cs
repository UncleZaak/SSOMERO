using AcademicSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicSystem.Infrastructure.Persistence.Configurations
{
    public class StudentConfiguration : IEntityTypeConfiguration<Student>
    {
        public void Configure(EntityTypeBuilder<Student> builder)
        {
            builder.ToTable("Students");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(x => x.NormalizedEmail)
                .IsRequired()
                .HasMaxLength(256);

            // Map RowVersion as concurrency token
            builder.Property(x => x.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            // Soft delete global query filter
            builder.HasQueryFilter(s => !s.IsDeleted);

            // Unique index on NormalizedEmail for case-insensitive uniqueness
            builder.HasIndex(x => x.NormalizedEmail)
                .IsUnique()
                .HasDatabaseName("IX_Students_NormalizedEmail")
                .HasFilter("[IsDeleted] = 0");

            // Configure other fields
            builder.Property(x => x.PasswordHash).HasMaxLength(512);

            builder.Property(x => x.CreatedAt).IsRequired();
            builder.Property(x => x.IsDeleted).IsRequired();
        }
    }
}
