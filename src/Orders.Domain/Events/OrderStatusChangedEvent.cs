using Orders.Domain.Interfaces;
using Orders.Domain.ValueObjects;

namespace Orders.Domain.Events;

public sealed record OrderStatusChangedEvent : IDomainEvent
{
    public Guid OrderId { get; }
    public string PreviousStatus { get; }
    public string NewStatus { get; }
    public DateTime OccurredOn { get; }

    public OrderStatusChangedEvent(Guid orderId, OrderStatus previousStatus, OrderStatus newStatus)
    {
        OrderId = orderId;
        PreviousStatus = previousStatus.Value;
        NewStatus = newStatus.Value;
        OccurredOn = DateTime.UtcNow;
    }
}
