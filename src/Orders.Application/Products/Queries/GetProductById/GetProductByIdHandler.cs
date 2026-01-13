using MediatR;
using Orders.Application.Common.Interfaces;

namespace Orders.Application.Products.Queries.GetProductById;

public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, ProductDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(5);

    public GetProductByIdHandler(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<ProductDto?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"products:{request.ProductId}";

        var cachedProduct = await _cacheService.GetAsync<ProductDto>(cacheKey, cancellationToken);
        if (cachedProduct != null)
        {
            return cachedProduct;
        }

        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);

        if (product == null)
        {
            return null;
        }

        var productDto = new ProductDto
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
        };

        await _cacheService.SetAsync(cacheKey, productDto, CacheExpiration, cancellationToken);

        return productDto;
    }
}
