using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssomero.Api.Entities;

namespace Ssomero.Api.Data.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasIndex(s => s.Email).IsUnique();
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.CreatedAt);
        builder.HasIndex(s => s.IsDeleted);

        builder.Property(s => s.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(s => s.SecondName).HasMaxLength(100).IsRequired();
        builder.Property(s => s.OtherNames).HasMaxLength(200);
        builder.Property(s => s.Email).HasMaxLength(200).IsRequired();
        builder.Property(s => s.Phone).HasMaxLength(20).IsRequired();
        builder.Property(s => s.Gender).HasMaxLength(20).IsRequired();
        builder.Property(s => s.Photo).HasMaxLength(500);

        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(UserStatus.Active);

        builder.HasQueryFilter(s => !s.IsDeleted);

        builder.HasOne(s => s.University).WithMany()
            .HasForeignKey(s => s.UniversityId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
