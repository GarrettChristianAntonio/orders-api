namespace Orders.Domain.Exceptions;

public class DomainException : Exception
{
    public string Code { get; }

    public DomainException(string message) : base(message)
    {
        Code = "DOMAIN_ERROR";
    }

    public DomainException(string code, string message) : base(message)
    {
        Code = code;
    }

    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
        Code = "DOMAIN_ERROR";
    }
}

public class EntityNotFoundException : DomainException
{
    public EntityNotFoundException(string entityName, object id)
        : base("ENTITY_NOT_FOUND", $"{entityName} with id '{id}' was not found.")
    {
    }
}

public class InvalidOrderStateException : DomainException
{
    public InvalidOrderStateException(string message)
        : base("INVALID_ORDER_STATE", message)
    {
    }
}

public class InsufficientStockException : DomainException
{
    public InsufficientStockException(string productName, int requested, int available)
        : base("INSUFFICIENT_STOCK", $"Insufficient stock for '{productName}'. Requested: {requested}, Available: {available}")
    {
    }
}
