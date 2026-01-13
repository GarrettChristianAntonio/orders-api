using MediatR;

namespace Orders.Application.Products.Commands.UpdateProduct;

public record UpdateProductCommand : IRequest<UpdateProductResult>
{
    public Guid ProductId { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public int? StockQuantity { get; init; }
    public bool? IsActive { get; init; }
}

public record UpdateProductResult
{
    public Guid ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool Success { get; init; }
}
