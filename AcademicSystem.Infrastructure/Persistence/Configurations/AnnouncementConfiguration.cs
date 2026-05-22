using AcademicSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicSystem.Infrastructure.Persistence.Configurations
{
    public class AnnouncementConfiguration : IEntityTypeConfiguration<Announcement>
    {
        public void Configure(EntityTypeBuilder<Announcement> builder)
        {
            builder.ToTable("Announcements");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();

            builder.Property(x => x.Title).IsRequired().HasMaxLength(256);
            builder.Property(x => x.Content).IsRequired();
            builder.Property(x => x.PostedAt).IsRequired();

            builder.HasOne(x => x.Class)
                .WithMany(c => c.Announcements)
                .HasForeignKey(x => x.ClassId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.PostedBy)
                .WithMany(u => u.Announcements)
                .HasForeignKey(x => x.PostedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();
            builder.HasQueryFilter(a => !a.IsDeleted);
        }
    }
}
