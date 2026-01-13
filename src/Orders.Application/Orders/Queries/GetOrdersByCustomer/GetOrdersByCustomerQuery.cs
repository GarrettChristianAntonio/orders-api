using MediatR;
using Orders.Application.Orders.Queries.GetOrderById;

namespace Orders.Application.Orders.Queries.GetOrdersByCustomer;

public record GetOrdersByCustomerQuery : IRequest<IReadOnlyList<OrderDto>>
{
    public Guid CustomerId { get; init; }
}
