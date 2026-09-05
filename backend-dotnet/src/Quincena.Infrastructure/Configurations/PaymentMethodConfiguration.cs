using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quincena.Domain.Entities;

namespace Quincena.Infrastructure.Configurations;

public class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
{
    public void Configure(EntityTypeBuilder<PaymentMethod> builder)
    {
        builder.ToTable("payment_methods");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(p => p.Name).HasColumnName("name").IsRequired();
        builder.Property(p => p.RequiresCard).HasColumnName("requires_card").HasDefaultValue(false);
        builder.Property(p => p.IsActive).HasColumnName("is_active").HasDefaultValue(true);
    }
}
