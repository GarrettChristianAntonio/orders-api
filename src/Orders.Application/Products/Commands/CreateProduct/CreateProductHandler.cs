using MediatR;
using Microsoft.Extensions.Logging;
using Orders.Application.Common.Interfaces;
using Orders.Domain.Entities;
using Orders.Domain.Exceptions;

namespace Orders.Application.Products.Commands.CreateProduct;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, CreateProductResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CreateProductHandler> _logger;

    public CreateProductHandler(
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        ILogger<CreateProductHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<CreateProductResult> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var existingProduct = await _unitOfWork.Products.GetBySkuAsync(request.Sku, cancellationToken);
        if (existingProduct != null)
        {
            throw new DomainException("DUPLICATE_SKU", $"A product with SKU '{request.Sku}' already exists");
        }

        var product = Product.Create(
            request.Name,
            request.Sku,
            request.Price,
            request.StockQuantity,
            request.Description);

        await _unitOfWork.Products.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveByPrefixAsync("products:", cancellationToken);

        _logger.LogInformation(
            "Product {ProductName} ({Sku}) created with ID {ProductId}",
            product.Name,
            product.Sku,
            product.Id);

        return new CreateProductResult
        {
            ProductId = product.Id,
            Name = product.Name,
            Sku = product.Sku
        };
    }
}
