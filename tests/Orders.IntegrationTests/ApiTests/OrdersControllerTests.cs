using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Orders.Application.Common.Interfaces;
using Orders.Domain.Entities;
using Orders.IntegrationTests.Fixtures;

namespace Orders.IntegrationTests.ApiTests;

[Collection("Api Collection")]
public class OrdersControllerTests
{
    private readonly HttpClient _client;
    private readonly ApiFixture _fixture;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public OrdersControllerTests(ApiFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task GetAll_ShouldReturnEmptyList_WhenNoOrders()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/orders");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("\"items\":[]");
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenOrderDoesNotExist()
    {
        // Act
        var response = await _client.GetAsync($"/api/v1/orders/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateOrder_WithoutAuth_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new
        {
            customerId = Guid.NewGuid(),
            shippingAddress = new
            {
                street = "123 Main St",
                city = "Test City",
                state = "Test State",
                country = "Test Country",
                zipCode = "12345"
            },
            items = new[]
            {
                new { productId = Guid.NewGuid(), quantity = 1 }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateCustomer_ShouldReturnCreated()
    {
        // Arrange
        var request = new
        {
            email = $"test{Guid.NewGuid():N}@example.com",
            firstName = "John",
            lastName = "Doe",
            phone = "1234567890"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/customers", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("customerId");
        content.Should().Contain("token");
    }

    [Fact]
    public async Task CreateProduct_WithAuth_ShouldReturnCreated()
    {
        // Arrange - Create customer and get token
        var token = await CreateCustomerAndGetToken();

        var request = new
        {
            name = "Test Product",
            description = "Test Description",
            sku = $"SKU-{Guid.NewGuid():N}".Substring(0, 20),
            price = 29.99m,
            stockQuantity = 100
        };

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GetProducts_ShouldReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task FullOrderWorkflow_ShouldComplete()
    {
        // Arrange - Create customer
        var customerEmail = $"test{Guid.NewGuid():N}@example.com";
        var customerResponse = await _client.PostAsJsonAsync("/api/v1/customers", new
        {
            email = customerEmail,
            firstName = "Test",
            lastName = "User"
        });

        var customerResult = await customerResponse.Content.ReadFromJsonAsync<CustomerResponse>(JsonOptions);
        var token = customerResult!.Token;

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Create product
        var productSku = $"SKU-{Guid.NewGuid():N}".Substring(0, 20);
        var productResponse = await _client.PostAsJsonAsync("/api/v1/products", new
        {
            name = "Test Product",
            sku = productSku,
            price = 25.00m,
            stockQuantity = 100
        });

        var productResult = await productResponse.Content.ReadFromJsonAsync<ProductResponse>(JsonOptions);

        // Create order
        var orderResponse = await _client.PostAsJsonAsync("/api/v1/orders", new
        {
            customerId = customerResult.CustomerId,
            shippingAddress = new
            {
                street = "123 Test St",
                city = "Test City",
                state = "Test State",
                country = "Test Country",
                zipCode = "12345"
            },
            items = new[]
            {
                new { productId = productResult!.ProductId, quantity = 2 }
            }
        });

        // Assert
        orderResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var orderResult = await orderResponse.Content.ReadFromJsonAsync<OrderResponse>(JsonOptions);
        orderResult.Should().NotBeNull();
        orderResult!.OrderId.Should().NotBeEmpty();
        orderResult.OrderNumber.Should().StartWith("ORD-");
        orderResult.Total.Should().Be(55.00m); // 25 * 2 + 10% tax
    }

    private async Task<string> CreateCustomerAndGetToken()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/customers", new
        {
            email = $"test{Guid.NewGuid():N}@example.com",
            firstName = "Test",
            lastName = "User"
        });

        var result = await response.Content.ReadFromJsonAsync<CustomerResponse>(JsonOptions);
        return result!.Token!;
    }

    private record CustomerResponse
    {
        public Guid CustomerId { get; init; }
        public string? Token { get; init; }
    }

    private record ProductResponse
    {
        public Guid ProductId { get; init; }
    }

    private record OrderResponse
    {
        public Guid OrderId { get; init; }
        public string OrderNumber { get; init; } = string.Empty;
        public decimal Total { get; init; }
    }
}
