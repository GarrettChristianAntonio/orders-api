using MediatR;
using Orders.Application.Common.Interfaces;
using Orders.Application.Orders.Queries.GetOrderById;

namespace Orders.Application.Orders.Queries.GetOrdersByCustomer;

public class GetOrdersByCustomerHandler : IRequestHandler<GetOrdersByCustomerQuery, IReadOnlyList<OrderDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetOrdersByCustomerHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<OrderDto>> Handle(GetOrdersByCustomerQuery request, CancellationToken cancellationToken)
    {
        var orders = await _unitOfWork.Orders.GetByCustomerIdAsync(request.CustomerId, cancellationToken);
        var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId, cancellationToken);

        return orders.Select(order => new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            CustomerName = customer?.FullName ?? "Unknown",
            Status = order.Status.Value,
            ShippingAddress = new AddressDto
            {
                Street = order.ShippingAddress.Street,
                City = order.ShippingAddress.City,
                State = order.ShippingAddress.State,
                Country = order.ShippingAddress.Country,
                ZipCode = order.ShippingAddress.ZipCode
            },
            SubTotal = order.SubTotal.Amount,
            Tax = order.Tax.Amount,
            Total = order.Total.Amount,
            Notes = order.Notes,
            Items = order.Items.Select(i => new OrderItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                ProductSku = i.ProductSku,
                UnitPrice = i.UnitPrice.Amount,
                Quantity = i.Quantity,
                TotalPrice = i.TotalPrice.Amount
            }).ToList(),
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt
        }).ToList();
    }
}
