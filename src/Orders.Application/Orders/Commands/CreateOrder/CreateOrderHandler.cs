using MediatR;
using Microsoft.Extensions.Logging;
using Orders.Application.Common.Interfaces;
using Orders.Domain.Entities;
using Orders.Domain.Exceptions;
using Orders.Domain.ValueObjects;

namespace Orders.Application.Orders.Commands.CreateOrder;

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, CreateOrderResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDistributedLockService _lockService;
    private readonly ILogger<CreateOrderHandler> _logger;

    public CreateOrderHandler(
        IUnitOfWork unitOfWork,
        IDistributedLockService lockService,
        ILogger<CreateOrderHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _lockService = lockService;
        _logger = logger;
    }

    public async Task<CreateOrderResult> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Customer), request.CustomerId);

        var productIds = request.Items.Select(i => i.ProductId).ToList();
        var products = await _unitOfWork.Products.GetByIdsAsync(productIds, cancellationToken);

        var missingProducts = productIds.Except(products.Select(p => p.Id)).ToList();
        if (missingProducts.Count != 0)
        {
            throw new EntityNotFoundException(nameof(Product), string.Join(", ", missingProducts));
        }

        var lockKey = $"order:create:{request.CustomerId}";
        await using var orderLock = await _lockService.AcquireLockAsync(lockKey, TimeSpan.FromSeconds(30), cancellationToken)
            ?? throw new DomainException("LOCK_FAILED", "Could not acquire lock for order creation. Please try again.");

        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            var shippingAddress = Address.Create(
                request.ShippingAddress.Street,
                request.ShippingAddress.City,
                request.ShippingAddress.State,
                request.ShippingAddress.Country,
                request.ShippingAddress.ZipCode);

            var order = Order.Create(customer, shippingAddress, request.Notes);

            foreach (var item in request.Items)
            {
                var product = products.First(p => p.Id == item.ProductId);

                if (!product.IsActive)
                {
                    throw new DomainException("PRODUCT_INACTIVE", $"Product '{product.Name}' is not available");
                }

                product.ReserveStock(item.Quantity);
                order.AddItem(product, item.Quantity);
            }

            await _unitOfWork.Orders.AddAsync(order, cancellationToken);

            foreach (var product in products)
            {
                await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
            }

            order.Confirm();

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation(
                "Order {OrderNumber} created successfully for customer {CustomerId}",
                order.OrderNumber,
                customer.Id);

            return new CreateOrderResult
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                Total = order.Total.Amount
            };
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
