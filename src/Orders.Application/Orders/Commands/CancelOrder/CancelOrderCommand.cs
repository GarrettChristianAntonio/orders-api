using MediatR;

namespace Orders.Application.Orders.Commands.CancelOrder;

public record CancelOrderCommand : IRequest<CancelOrderResult>
{
    public Guid OrderId { get; init; }
}

public record CancelOrderResult
{
    public Guid OrderId { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public bool Success { get; init; }
}
