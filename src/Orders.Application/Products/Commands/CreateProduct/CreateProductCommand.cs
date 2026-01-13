using MediatR;

namespace Orders.Application.Products.Commands.CreateProduct;

public record CreateProductCommand : IRequest<CreateProductResult>
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required string Sku { get; init; }
    public decimal Price { get; init; }
    public int StockQuantity { get; init; }
}

public record CreateProductResult
{
    public Guid ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Sku { get; init; } = string.Empty;
}
