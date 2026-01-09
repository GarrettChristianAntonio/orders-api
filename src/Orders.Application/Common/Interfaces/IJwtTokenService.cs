using Orders.Domain.Entities;

namespace Orders.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(Customer customer);
    bool ValidateToken(string token);
    Guid? GetCustomerIdFromToken(string token);
}
