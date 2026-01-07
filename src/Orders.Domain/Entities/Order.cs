using Orders.Domain.Events;
using Orders.Domain.Exceptions;
using Orders.Domain.ValueObjects;

namespace Orders.Domain.Entities;

public class Order : BaseEntity<Guid>
{
    public string OrderNumber { get; private set; } = string.Empty;
    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; } = OrderStatus.Pending;
    public Address ShippingAddress { get; private set; } = null!;
    public Money SubTotal { get; private set; } = Money.Zero();
    public Money Tax { get; private set; } = Money.Zero();
    public Money Total { get; private set; } = Money.Zero();
    public string? Notes { get; private set; }

    private readonly List<OrderItem> _items = [];
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public Customer? Customer { get; private set; }

    private Order() { }

    public static Order Create(Customer customer, Address shippingAddress, string? notes = null)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = GenerateOrderNumber(),
            CustomerId = customer.Id,
            ShippingAddress = shippingAddress,
            Notes = notes?.Trim(),
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        return order;
    }

    public void AddItem(Product product, int quantity)
    {
        if (Status != OrderStatus.Pending)
        {
            throw new InvalidOrderStateException("Cannot add items to a non-pending order");
        }

        var existingItem = _items.FirstOrDefault(i => i.ProductId == product.Id);
        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            var item = OrderItem.Create(Id, product, quantity);
            _items.Add(item);
        }

        RecalculateTotals();
        SetUpdatedAt();
    }

    public void RemoveItem(Guid productId)
    {
        if (Status != OrderStatus.Pending)
        {
            throw new InvalidOrderStateException("Cannot remove items from a non-pending order");
        }

        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            _items.Remove(item);
            RecalculateTotals();
            SetUpdatedAt();
        }
    }

    public void UpdateItemQuantity(Guid productId, int quantity)
    {
        if (Status != OrderStatus.Pending)
        {
            throw new InvalidOrderStateException("Cannot update items in a non-pending order");
        }

        var item = _items.FirstOrDefault(i => i.ProductId == productId)
            ?? throw new EntityNotFoundException("OrderItem", productId);

        item.UpdateQuantity(quantity);
        RecalculateTotals();
        SetUpdatedAt();
    }

    public void Confirm()
    {
        UpdateStatus(OrderStatus.Confirmed);
        AddDomainEvent(new OrderCreatedEvent(Id, CustomerId, Total.Amount));
    }

    public void Process()
    {
        UpdateStatus(OrderStatus.Processing);
    }

    public void Ship()
    {
        UpdateStatus(OrderStatus.Shipped);
    }

    public void Deliver()
    {
        UpdateStatus(OrderStatus.Delivered);
    }

    public void Cancel()
    {
        var previousStatus = Status;
        UpdateStatus(OrderStatus.Cancelled);
        AddDomainEvent(new OrderStatusChangedEvent(Id, previousStatus, OrderStatus.Cancelled));
    }

    private void UpdateStatus(OrderStatus newStatus)
    {
        if (!Status.CanTransitionTo(newStatus))
        {
            throw new InvalidOrderStateException(
                $"Cannot transition order from '{Status}' to '{newStatus}'");
        }

        var previousStatus = Status;
        Status = newStatus;
        SetUpdatedAt();

        if (newStatus != OrderStatus.Cancelled)
        {
            AddDomainEvent(new OrderStatusChangedEvent(Id, previousStatus, newStatus));
        }
    }

    private void RecalculateTotals()
    {
        SubTotal = _items.Aggregate(Money.Zero(), (acc, item) => acc.Add(item.TotalPrice));
        Tax = Money.Create(SubTotal.Amount * 0.10m); // 10% tax
        Total = SubTotal.Add(Tax);
    }

    private static string GenerateOrderNumber()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = Random.Shared.Next(1000, 9999);
        return $"ORD-{timestamp}-{random}";
    }
}
