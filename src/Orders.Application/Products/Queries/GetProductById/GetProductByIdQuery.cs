using MediatR;

namespace Orders.Application.Products.Queries.GetProductById;

public record GetProductByIdQuery : IRequest<ProductDto?>
{
    public Guid ProductId { get; init; }
}

public record ProductDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Sku { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public string Currency { get; init; } = string.Empty;
    public int StockQuantity { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
