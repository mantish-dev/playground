using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stockroom.Domain.Entities;

namespace Stockroom.Infrastructure.Persistence.Configurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.CustomerEmail).HasMaxLength(320).IsRequired();
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(o => o.PlacedAt).IsRequired();

        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.PlacedAt);

        builder.Ignore(o => o.Total);

        // The line never needs to know its order's id: EF keeps the foreign key as a shadow
        // property so the domain model does not carry a persistence-only field.
        builder.HasMany(o => o.Lines)
            .WithOne()
            .HasForeignKey("OrderId")
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(o => o.Lines)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();
    }
}
