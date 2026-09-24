using AuthService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Infrastructure.Postgres.Configurations;

internal sealed class RefreshSessionConfiguration : IEntityTypeConfiguration<RefreshSession>
{
    public void Configure(EntityTypeBuilder<RefreshSession> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("refresh_sessions");

        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.AccountId).HasColumnName("account_id");

        builder.Property(s => s.TokenHash)
            .HasColumnName("token_hash")
            .HasMaxLength(RefreshSessionConstants.TOKEN_HASH_LENGTH)
            .IsRequired();

        builder.Property(s => s.CreatedAt).HasColumnName("created_at");
        builder.Property(s => s.ExpiresAt).HasColumnName("expires_at");
        builder.Property(s => s.RevokedAt).HasColumnName("revoked_at");
        builder.Property(s => s.ReplacedById).HasColumnName("replaced_by_id");

        builder.HasIndex(s => s.TokenHash).IsUnique();
        builder.HasIndex(s => s.AccountId);
        builder.HasIndex(s => s.ReplacedById);

        builder.HasOne(s => s.Account)
            .WithMany()
            .HasForeignKey(s => s.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}