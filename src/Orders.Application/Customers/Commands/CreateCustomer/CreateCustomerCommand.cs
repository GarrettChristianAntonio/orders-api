using MediatR;

namespace Orders.Application.Customers.Commands.CreateCustomer;

public record CreateCustomerCommand : IRequest<CreateCustomerResult>
{
    public required string Email { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? Phone { get; init; }
}

public record CreateCustomerResult
{
    public Guid CustomerId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string? Token { get; init; }
}
