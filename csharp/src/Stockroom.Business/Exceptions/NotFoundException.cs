namespace Stockroom.Business.Exceptions;

public sealed class NotFoundException : Exception
{
    public NotFoundException(string entityName, Guid id)
        : base($"{entityName} with id {id} was not found.")
    {
        EntityName = entityName;
        Id = id;
    }

    public NotFoundException()
    {
    }

    public NotFoundException(string message)
        : base(message)
    {
    }

    public NotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public string EntityName { get; } = string.Empty;

    public Guid Id { get; }
}
