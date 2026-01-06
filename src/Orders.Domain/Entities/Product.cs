using Orders.Domain.Exceptions;
using Orders.Domain.ValueObjects;

namespace Orders.Domain.Entities;

public class Product : BaseEntity<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string Sku { get; private set; } = string.Empty;
    public Money Price { get; private set; } = Money.Zero();
    public int StockQuantity { get; private set; }
    public bool IsActive { get; private set; }

    private Product() { }

    public static Product Create(string name, string sku, decimal price, int stockQuantity, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name cannot be empty", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(sku))
        {
            throw new ArgumentException("SKU cannot be empty", nameof(sku));
        }

        if (stockQuantity < 0)
        {
            throw new ArgumentException("Stock quantity cannot be negative", nameof(stockQuantity));
        }

        return new Product
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description?.Trim(),
            Sku = sku.Trim().ToUpperInvariant(),
            Price = Money.Create(price),
            StockQuantity = stockQuantity,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string? description, decimal price)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            Name = name.Trim();
        }

        Description = description?.Trim();
        Price = Money.Create(price);
        SetUpdatedAt();
    }

    public void UpdateStock(int quantity)
    {
        if (quantity < 0)
        {
            throw new ArgumentException("Stock quantity cannot be negative", nameof(quantity));
        }

        StockQuantity = quantity;
        SetUpdatedAt();
    }

    public void ReserveStock(int quantity)
    {
        if (quantity > StockQuantity)
        {
            throw new InsufficientStockException(Name, quantity, StockQuantity);
        }

        StockQuantity -= quantity;
        SetUpdatedAt();
    }

    public void ReleaseStock(int quantity)
    {
        StockQuantity += quantity;
        SetUpdatedAt();
    }

    public void Activate()
    {
        IsActive = true;
        SetUpdatedAt();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }

    public bool HasSufficientStock(int quantity) => StockQuantity >= quantity;
}
