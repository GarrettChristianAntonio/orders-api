using MediatR;
using Orders.Application.Common.Models;
using Orders.Application.Products.Queries.GetProductById;

namespace Orders.Application.Products.Queries.GetAllProducts;

public record GetAllProductsQuery : IRequest<PagedResult<ProductDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public bool? IsActive { get; init; }
}
