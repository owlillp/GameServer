using ClanService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClanService.Infrastructure.Postgres.Configurations;

internal sealed class ClanMemberConfiguration : IEntityTypeConfiguration<ClanMember>
{
    public void Configure(EntityTypeBuilder<ClanMember> builder)
    {
        builder.ToTable("clan_members");

        builder.HasKey(member => new { member.ClanId, member.UserId });

        builder.Property(member => member.ClanId).HasColumnName("clan_id");
        builder.Property(member => member.UserId).HasColumnName("user_id");
        builder.Property(member => member.JoinedAt).HasColumnName("joined_at");

        builder.HasIndex(member => member.UserId);
    }
}
