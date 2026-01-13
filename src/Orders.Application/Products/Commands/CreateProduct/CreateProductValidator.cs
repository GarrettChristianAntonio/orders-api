using FluentValidation;

namespace Orders.Application.Products.Commands.CreateProduct;

public class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(200)
            .WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.Sku)
            .NotEmpty()
            .WithMessage("SKU is required")
            .MaximumLength(50)
            .WithMessage("SKU must not exceed 50 characters")
            .Matches("^[A-Za-z0-9-]+$")
            .WithMessage("SKU can only contain letters, numbers, and hyphens");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("Price must be greater than 0");

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Stock quantity cannot be negative");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Description must not exceed 1000 characters");
    }
}
