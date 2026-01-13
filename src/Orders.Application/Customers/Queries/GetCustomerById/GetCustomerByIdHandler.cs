using MediatR;
using Orders.Application.Common.Interfaces;

namespace Orders.Application.Customers.Queries.GetCustomerById;

public class GetCustomerByIdHandler : IRequestHandler<GetCustomerByIdQuery, CustomerDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCustomerByIdHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CustomerDto?> Handle(GetCustomerByIdQuery request, CancellationToken cancellationToken)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(request.CustomerId, cancellationToken);

        if (customer == null)
        {
            return null;
        }

        return new CustomerDto
        {
            Id = customer.Id,
            Email = customer.Email,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            FullName = customer.FullName,
            Phone = customer.Phone,
            ShippingAddress = customer.ShippingAddress != null
                ? new AddressDto
                {
                    Street = customer.ShippingAddress.Street,
                    City = customer.ShippingAddress.City,
                    State = customer.ShippingAddress.State,
                    Country = customer.ShippingAddress.Country,
                    ZipCode = customer.ShippingAddress.ZipCode
                }
                : null,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        };
    }
}
