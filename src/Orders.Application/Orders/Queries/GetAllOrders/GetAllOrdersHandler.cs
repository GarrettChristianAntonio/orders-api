using MediatR;
using Orders.Application.Common.Interfaces;
using Orders.Application.Common.Models;
using Orders.Application.Orders.Queries.GetOrderById;

namespace Orders.Application.Orders.Queries.GetAllOrders;

public class GetAllOrdersHandler : IRequestHandler<GetAllOrdersQuery, PagedResult<OrderDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllOrdersHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<OrderDto>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 10 : Math.Min(request.PageSize, 100);

        var orders = await _unitOfWork.Orders.GetAllAsync(pageNumber, pageSize, cancellationToken);
        var totalCount = await _unitOfWork.Orders.GetTotalCountAsync(cancellationToken);

        var customerIds = orders.Select(o => o.CustomerId).Distinct().ToList();
        var customers = new Dictionary<Guid, string>();

        foreach (var customerId in customerIds)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId, cancellationToken);
            if (customer != null)
            {
                customers[customerId] = customer.FullName;
            }
        }

        var orderDtos = orders.Select(order => new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            CustomerName = customers.GetValueOrDefault(order.CustomerId, "Unknown"),
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

        return new PagedResult<OrderDto>(orderDtos, pageNumber, pageSize, totalCount);
    }
}
