using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Orders.Application.Common.Interfaces;

namespace Orders.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthController(IUnitOfWork unitOfWork, IJwtTokenService jwtTokenService)
    {
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
    }

    /// <summary>
    /// Login with email to get JWT token
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var customer = await _unitOfWork.Customers.GetByEmailAsync(request.Email);

        if (customer == null)
        {
            return Unauthorized(new { message = "Invalid credentials" });
        }

        var token = _jwtTokenService.GenerateToken(customer);

        return Ok(new LoginResponse
        {
            Token = token,
            CustomerId = customer.Id,
            Email = customer.Email,
            FullName = customer.FullName
        });
    }

    /// <summary>
    /// Validate JWT token
    /// </summary>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(ValidateResponse), StatusCodes.Status200OK)]
    public IActionResult Validate([FromBody] ValidateRequest request)
    {
        var isValid = _jwtTokenService.ValidateToken(request.Token);
        var customerId = isValid ? _jwtTokenService.GetCustomerIdFromToken(request.Token) : null;

        return Ok(new ValidateResponse
        {
            IsValid = isValid,
            CustomerId = customerId
        });
    }
}

public record LoginRequest
{
    public required string Email { get; init; }
}

public record LoginResponse
{
    public string Token { get; init; } = string.Empty;
    public Guid CustomerId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
}

public record ValidateRequest
{
    public required string Token { get; init; }
}

public record ValidateResponse
{
    public bool IsValid { get; init; }
    public Guid? CustomerId { get; init; }
}
