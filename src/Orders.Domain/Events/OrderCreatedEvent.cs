using Orders.Domain.Interfaces;

namespace Orders.Domain.Events;

public sealed record OrderCreatedEvent : IDomainEvent
{
    public Guid OrderId { get; }
    public Guid CustomerId { get; }
    public decimal TotalAmount { get; }
    public DateTime OccurredOn { get; }

    public OrderCreatedEvent(Guid orderId, Guid customerId, decimal totalAmount)
    {
        OrderId = orderId;
        CustomerId = customerId;
        TotalAmount = totalAmount;
        OccurredOn = DateTime.UtcNow;
    }
}
