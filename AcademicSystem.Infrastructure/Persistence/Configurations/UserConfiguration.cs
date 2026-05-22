using AcademicSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicSystem.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();

            builder.Property(x => x.Email).IsRequired().HasMaxLength(256);
            builder.Property(x => x.NormalizedEmail).IsRequired().HasMaxLength(256);
            builder.Property(x => x.PasswordHash).HasMaxLength(512);

            builder.Property(x => x.Role).IsRequired();

            builder.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();

            builder.HasQueryFilter(u => !u.IsDeleted);

            builder.HasIndex(x => x.NormalizedEmail)
                .IsUnique()
                .HasDatabaseName("IX_Users_NormalizedEmail")
                .HasFilter("[IsDeleted] = 0");

            builder.HasOne(x => x.University)
                .WithMany(u => u.Users)
                .HasForeignKey(x => x.UniversityId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
