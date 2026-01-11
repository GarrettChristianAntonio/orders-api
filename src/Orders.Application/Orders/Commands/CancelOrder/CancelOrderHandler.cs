using MediatR;
using Microsoft.Extensions.Logging;
using Orders.Application.Common.Interfaces;
using Orders.Domain.Entities;
using Orders.Domain.Exceptions;

namespace Orders.Application.Orders.Commands.CancelOrder;

public class CancelOrderHandler : IRequestHandler<CancelOrderCommand, CancelOrderResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CancelOrderHandler> _logger;

    public CancelOrderHandler(IUnitOfWork unitOfWork, ILogger<CancelOrderHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<CancelOrderResult> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _unitOfWork.Orders.GetByIdWithItemsAsync(request.OrderId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Order), request.OrderId);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var productIds = order.Items.Select(i => i.ProductId).ToList();
            var products = await _unitOfWork.Products.GetByIdsAsync(productIds, cancellationToken);

            foreach (var item in order.Items)
            {
                var product = products.FirstOrDefault(p => p.Id == item.ProductId);
                product?.ReleaseStock(item.Quantity);

                if (product != null)
                {
                    await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
                }
            }

            order.Cancel();
            await _unitOfWork.Orders.UpdateAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation(
                "Order {OrderNumber} has been cancelled",
                order.OrderNumber);

            return new CancelOrderResult
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                Success = true
            };
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
