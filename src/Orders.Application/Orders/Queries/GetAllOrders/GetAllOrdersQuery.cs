using MediatR;
using Orders.Application.Common.Models;
using Orders.Application.Orders.Queries.GetOrderById;

namespace Orders.Application.Orders.Queries.GetAllOrders;

public record GetAllOrdersQuery : IRequest<PagedResult<OrderDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
