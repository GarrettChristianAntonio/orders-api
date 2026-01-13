using MediatR;
using Orders.Application.Common.Interfaces;
using Orders.Application.Common.Models;
using Orders.Application.Products.Queries.GetProductById;

namespace Orders.Application.Products.Queries.GetAllProducts;

public class GetAllProductsHandler : IRequestHandler<GetAllProductsQuery, PagedResult<ProductDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(5);

    public GetAllProductsHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<PagedResult<ProductDto>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 10 : Math.Min(request.PageSize, 100);

        var cacheKey = $"products:all:{pageNumber}:{pageSize}:{request.IsActive}";

        var cachedResult = await _cacheService.GetAsync<PagedResult<ProductDto>>(cacheKey, cancellationToken);
        if (cachedResult != null)
        {
            return cachedResult;
        }

        var products = await _unitOfWork.Products.GetAllAsync(pageNumber, pageSize, request.IsActive, cancellationToken);
        var totalCount = await _unitOfWork.Products.GetTotalCountAsync(request.IsActive, cancellationToken);

        var productDtos = products.Select(product => new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Sku = product.Sku,
            Price = product.Price.Amount,
            Currency = product.Price.Currency,
            StockQuantity = product.StockQuantity,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        }).ToList();

        var result = new PagedResult<ProductDto>(productDtos, pageNumber, pageSize, totalCount);

        await _cacheService.SetAsync(cacheKey, result, CacheExpiration, cancellationToken);

        return result;
    }
}
