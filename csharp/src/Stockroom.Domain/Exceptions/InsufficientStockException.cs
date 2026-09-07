namespace Stockroom.Domain.Exceptions;

public sealed class InsufficientStockException : DomainException
{
    public InsufficientStockException(string sku, int requested, int available)
        : base($"Insufficient stock for {sku}: requested {requested}, available {available}.")
    {
        Sku = sku;
        Requested = requested;
        Available = available;
    }

    public InsufficientStockException()
    {
    }

    public InsufficientStockException(string message)
        : base(message)
    {
    }

    public InsufficientStockException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public string Sku { get; } = string.Empty;

    public int Requested { get; }

    public int Available { get; }
}
