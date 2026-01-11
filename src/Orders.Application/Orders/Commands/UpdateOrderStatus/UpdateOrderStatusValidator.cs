using FluentValidation;

namespace Orders.Application.Orders.Commands.UpdateOrderStatus;

public class UpdateOrderStatusValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    private static readonly string[] ValidStatuses = ["Confirmed", "Processing", "Shipped", "Delivered"];

    public UpdateOrderStatusValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Order ID is required");

        RuleFor(x => x.Status)
            .NotEmpty()
            .WithMessage("Status is required")
            .Must(status => ValidStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Status must be one of: {string.Join(", ", ValidStatuses)}");
    }
}
