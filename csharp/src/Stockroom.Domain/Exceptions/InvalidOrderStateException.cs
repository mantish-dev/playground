using Stockroom.Domain.Entities;

namespace Stockroom.Domain.Exceptions;

public sealed class InvalidOrderStateException : DomainException
{
    public InvalidOrderStateException(Guid orderId, OrderStatus status, string action)
        : base($"Order {orderId} is {status} and cannot be {action}.")
    {
        OrderId = orderId;
        Status = status;
    }

    public InvalidOrderStateException()
    {
    }

    public InvalidOrderStateException(string message)
        : base(message)
    {
    }

    public InvalidOrderStateException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public Guid OrderId { get; }

    public OrderStatus Status { get; }
}
