using MediatR;
using Microsoft.Extensions.Logging;
using Orders.Application.Common.Interfaces;
using Orders.Domain.Entities;
using Orders.Domain.Exceptions;

namespace Orders.Application.Customers.Commands.CreateCustomer;

public class CreateCustomerHandler : IRequestHandler<CreateCustomerCommand, CreateCustomerResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<CreateCustomerHandler> _logger;

    public CreateCustomerHandler(
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService,
        ILogger<CreateCustomerHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<CreateCustomerResult> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var existingCustomer = await _unitOfWork.Customers.ExistsAsync(request.Email, cancellationToken);
        if (existingCustomer)
        {
            throw new DomainException("DUPLICATE_EMAIL", $"A customer with email '{request.Email}' already exists");
        }

        var customer = Customer.Create(
            request.Email,
            request.FirstName,
            request.LastName,
            request.Phone);

        await _unitOfWork.Customers.AddAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var token = _jwtTokenService.GenerateToken(customer);

        _logger.LogInformation(
            "Customer {CustomerEmail} created with ID {CustomerId}",
            customer.Email,
            customer.Id);

        return new CreateCustomerResult
        {
            CustomerId = customer.Id,
            Email = customer.Email,
            FullName = customer.FullName,
            Token = token
        };
    }
}
