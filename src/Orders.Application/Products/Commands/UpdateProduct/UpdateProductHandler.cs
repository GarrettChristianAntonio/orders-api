using MediatR;
using Microsoft.Extensions.Logging;
using Orders.Application.Common.Interfaces;
using Orders.Domain.Entities;
using Orders.Domain.Exceptions;

namespace Orders.Application.Products.Commands.UpdateProduct;

public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, UpdateProductResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ILogger<UpdateProductHandler> _logger;

    public UpdateProductHandler(
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        ILogger<UpdateProductHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<UpdateProductResult> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Product), request.ProductId);

        product.Update(request.Name, request.Description, request.Price);

        if (request.StockQuantity.HasValue)
        {
            product.UpdateStock(request.StockQuantity.Value);
        }

        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value)
            {
                product.Activate();
            }
            else
            {
                product.Deactivate();
            }
        }

        await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _cacheService.RemoveAsync($"products:{product.Id}", cancellationToken);
        await _cacheService.RemoveByPrefixAsync("products:all:", cancellationToken);

        _logger.LogInformation(
            "Product {ProductId} updated successfully",
            product.Id);

        return new UpdateProductResult
        {
            ProductId = product.Id,
            Name = product.Name,
            Success = true
        };
    }
}
