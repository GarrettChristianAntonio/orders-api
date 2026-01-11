using MediatR;

namespace Orders.Application.Orders.Commands.UpdateOrderStatus;

public record UpdateOrderStatusCommand : IRequest<UpdateOrderStatusResult>
{
    public Guid OrderId { get; init; }
    public required string Status { get; init; }
}

public record UpdateOrderStatusResult
{
    public Guid OrderId { get; init; }
    public string PreviousStatus { get; init; } = string.Empty;
    public string NewStatus { get; init; } = string.Empty;
}
