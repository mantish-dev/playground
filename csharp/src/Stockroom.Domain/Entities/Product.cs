using Stockroom.Domain.Exceptions;

namespace Stockroom.Domain.Entities;

public sealed class Product
{
    private Product()
    {
    }

    public Guid Id { get; private set; }

    public string Sku { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public decimal UnitPrice { get; private set; }

    public int QuantityOnHand { get; private set; }

    public int QuantityReserved { get; private set; }

    public bool IsDiscontinued { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public int QuantityAvailable => QuantityOnHand - QuantityReserved;

    public static Product Create(string sku, string name, decimal unitPrice, int initialStock, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sku);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegative(unitPrice);
        ArgumentOutOfRangeException.ThrowIfNegative(initialStock);

        return new Product
        {
            Id = Guid.NewGuid(),
            Sku = sku.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            UnitPrice = unitPrice,
            QuantityOnHand = initialStock,
            QuantityReserved = 0,
            CreatedAt = now,
            UpdatedAt = now,
        };
    }

    public void Rename(string name, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
        UpdatedAt = now;
    }

    public void ChangePrice(decimal unitPrice, DateTimeOffset now)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(unitPrice);
        UnitPrice = unitPrice;
        UpdatedAt = now;
    }

    public void Discontinue(DateTimeOffset now)
    {
        if (QuantityReserved > 0)
        {
            throw new DomainException($"Cannot discontinue {Sku} while {QuantityReserved} units are reserved by open orders.");
        }

        IsDiscontinued = true;
        UpdatedAt = now;
    }

    public void Restock(int quantity, DateTimeOffset now)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        EnsureActive();
        QuantityOnHand += quantity;
        UpdatedAt = now;
    }

    public void Reserve(int quantity, DateTimeOffset now)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        EnsureActive();

        if (quantity > QuantityAvailable)
        {
            throw new InsufficientStockException(Sku, quantity, QuantityAvailable);
        }

        QuantityReserved += quantity;
        UpdatedAt = now;
    }

    public void Release(int quantity, DateTimeOffset now)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        EnsureReserved(quantity, "release");

        QuantityReserved -= quantity;
        UpdatedAt = now;
    }

    public void Consume(int quantity, DateTimeOffset now)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(quantity);
        EnsureReserved(quantity, "consume");

        QuantityReserved -= quantity;
        QuantityOnHand -= quantity;
        UpdatedAt = now;
    }

    private void EnsureActive()
    {
        if (IsDiscontinued)
        {
            throw new DomainException($"Product {Sku} is discontinued.");
        }
    }

    private void EnsureReserved(int quantity, string action)
    {
        if (quantity > QuantityReserved)
        {
            throw new DomainException($"Cannot {action} {quantity} units of {Sku}: only {QuantityReserved} are reserved.");
        }
    }
}
