using AcademicSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AcademicSystem.Infrastructure.Persistence.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedNever();

            builder.Property(x => x.TokenHash).IsRequired().HasMaxLength(512);
            builder.Property(x => x.ExpiresAt).IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.Property(x => x.CreatedByIp).HasMaxLength(64);
            builder.Property(x => x.ReplacedByTokenHash).HasMaxLength(512);
            builder.Property(x => x.IsRevoked).IsRequired();

            builder.HasOne(x => x.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.UserId);
            builder.HasIndex(x => x.TokenHash);

            builder.Property(x => x.RowVersion).IsRowVersion().IsConcurrencyToken();
        }
    }
}
