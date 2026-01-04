namespace Orders.Domain.Interfaces;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
