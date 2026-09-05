using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quincena.Domain.Entities;

namespace Quincena.Infrastructure.Configurations;

public class CardConfiguration : IEntityTypeConfiguration<Card>
{
    public void Configure(EntityTypeBuilder<Card> builder)
    {
        builder.ToTable("cards");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(c => c.Name).HasColumnName("name").IsRequired();
        builder.Property(c => c.LastFour).HasColumnName("last_four").HasColumnType("char(4)");
        builder.Property(c => c.Bank).HasColumnName("bank");
        builder.Property(c => c.IsActive).HasColumnName("is_active").HasDefaultValue(true);
        builder.Property(c => c.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("NOW()");
    }
}
