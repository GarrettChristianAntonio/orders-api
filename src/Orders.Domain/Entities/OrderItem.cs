using Orders.Domain.ValueObjects;

namespace Orders.Domain.Entities;

public class OrderItem : BaseEntity<Guid>
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; } = string.Empty;
    public string ProductSku { get; private set; } = string.Empty;
    public Money UnitPrice { get; private set; } = Money.Zero();
    public int Quantity { get; private set; }
    public Money TotalPrice => UnitPrice.Multiply(Quantity);

    public Product? Product { get; private set; }
    public Order? Order { get; private set; }

    private OrderItem() { }

    internal static OrderItem Create(Guid orderId, Product product, int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
        }

        return new OrderItem
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ProductId = product.Id,
            ProductName = product.Name,
            ProductSku = product.Sku,
            UnitPrice = product.Price,
            Quantity = quantity,
            CreatedAt = DateTime.UtcNow
        };
    }

    internal void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
        }

        Quantity = quantity;
        SetUpdatedAt();
    }
}
