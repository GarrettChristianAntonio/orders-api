namespace Orders.Domain.Interfaces;

public interface IEntity<TId>
{
    TId Id { get; }
}

public interface IAuditableEntity
{
    DateTime CreatedAt { get; }
    DateTime? UpdatedAt { get; }
}
