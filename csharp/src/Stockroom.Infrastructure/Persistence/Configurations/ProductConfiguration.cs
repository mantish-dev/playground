using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stockroom.Domain.Entities;

namespace Stockroom.Infrastructure.Persistence.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Sku)
            .HasMaxLength(32)
            .IsRequired();
        builder.HasIndex(p => p.Sku)
            .IsUnique();

        builder.Property(p => p.Name)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(p => p.UnitPrice)
            .HasPrecision(18, 2);

        builder.Property(p => p.QuantityOnHand).IsRequired();
        builder.Property(p => p.QuantityReserved).IsRequired();
        builder.Property(p => p.IsDiscontinued).IsRequired();

        builder.Property(p => p.CreatedAt).IsRequired();
        builder.Property(p => p.UpdatedAt)
            .IsRequired()
            .IsConcurrencyToken();

        builder.Ignore(p => p.QuantityAvailable);
    }
}
