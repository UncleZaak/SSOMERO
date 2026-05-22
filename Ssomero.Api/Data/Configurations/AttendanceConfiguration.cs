using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ssomero.Api.Entities;

namespace Ssomero.Api.Data.Configurations;

public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
{
    public void Configure(EntityTypeBuilder<Attendance> builder)
    {
        builder.HasKey(a => a.Id);

        builder.HasIndex(a => new { a.StudentId, a.SessionId }).IsUnique();
        builder.HasIndex(a => a.Date);
        builder.HasIndex(a => a.StudentId);
        builder.HasIndex(a => a.ClassId);

        builder.Property(a => a.Notes).HasMaxLength(500);

        builder.HasOne(a => a.Student)
            .WithMany()
            .HasForeignKey(a => a.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Class)
            .WithMany()
            .HasForeignKey(a => a.ClassId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
