using MediatR;
using Microsoft.Extensions.Logging;
using Orders.Application.Common.Interfaces;
using Orders.Domain.Entities;
using Orders.Domain.Exceptions;
using Orders.Domain.ValueObjects;

namespace Orders.Application.Orders.Commands.UpdateOrderStatus;

public class UpdateOrderStatusHandler : IRequestHandler<UpdateOrderStatusCommand, UpdateOrderStatusResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateOrderStatusHandler> _logger;

    public UpdateOrderStatusHandler(IUnitOfWork unitOfWork, ILogger<UpdateOrderStatusHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<UpdateOrderStatusResult> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Order), request.OrderId);

        var previousStatus = order.Status.Value;
        var newStatus = OrderStatus.FromString(request.Status);

        switch (newStatus.Value.ToLowerInvariant())
        {
            case "confirmed":
                order.Confirm();
                break;
            case "processing":
                order.Process();
                break;
            case "shipped":
                order.Ship();
                break;
            case "delivered":
                order.Deliver();
                break;
        }

        await _unitOfWork.Orders.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Order {OrderId} status updated from {PreviousStatus} to {NewStatus}",
            order.Id,
            previousStatus,
            newStatus.Value);

        return new UpdateOrderStatusResult
        {
            OrderId = order.Id,
            PreviousStatus = previousStatus,
            NewStatus = newStatus.Value
        };
    }
}
