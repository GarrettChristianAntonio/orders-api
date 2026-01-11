using MediatR;

namespace Orders.Application.Orders.Commands.CreateOrder;

public record CreateOrderCommand : IRequest<CreateOrderResult>
{
    public Guid CustomerId { get; init; }
    public required AddressDto ShippingAddress { get; init; }
    public required List<OrderItemDto> Items { get; init; }
    public string? Notes { get; init; }
}

public record AddressDto
{
    public required string Street { get; init; }
    public required string City { get; init; }
    public required string State { get; init; }
    public required string Country { get; init; }
    public required string ZipCode { get; init; }
}

public record OrderItemDto
{
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
}

public record CreateOrderResult
{
    public Guid OrderId { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public decimal Total { get; init; }
}
