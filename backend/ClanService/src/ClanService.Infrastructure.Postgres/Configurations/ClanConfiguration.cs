using ClanService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClanService.Infrastructure.Postgres.Configurations;

internal sealed class ClanConfiguration : IEntityTypeConfiguration<Clan>
{
    public void Configure(EntityTypeBuilder<Clan> builder)
    {
        builder.ToTable("clans");

        builder.HasKey(clan => clan.Id);

        builder.Property(clan => clan.Id).HasColumnName("id");

        builder.Property(clan => clan.Name)
            .HasColumnName("name")
            .HasMaxLength(ClanConstants.NAME_MAX_LENGTH)
            .IsRequired();

        builder.Property(clan => clan.NormalizedName)
            .HasColumnName("normalized_name")
            .HasMaxLength(ClanConstants.NAME_MAX_LENGTH)
            .IsRequired();

        builder.Property(clan => clan.Tag)
            .HasColumnName("tag")
            .HasMaxLength(ClanConstants.TAG_MAX_LENGTH)
            .IsRequired();

        builder.Property(clan => clan.Description)
            .HasColumnName("description")
            .HasMaxLength(ClanConstants.DESCRIPTION_MAX_LENGTH);

        builder.Property(clan => clan.LeaderId).HasColumnName("leader_id");
        builder.Property(clan => clan.CreatedAt).HasColumnName("created_at");
        builder.Property(clan => clan.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(clan => clan.NormalizedName).IsUnique();
        builder.HasIndex(clan => clan.Tag).IsUnique();

        builder.Navigation(clan => clan.Members)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(clan => clan.Members)
            .WithOne()
            .HasForeignKey(member => member.ClanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
