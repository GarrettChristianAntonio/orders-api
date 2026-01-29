using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Orders.Application.Common.Interfaces;
using Orders.Application.Orders.Commands.CreateOrder;
using Orders.Domain.Entities;
using Orders.Domain.Exceptions;

namespace Orders.UnitTests.Application;

public class CreateOrderHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IDistributedLockService> _lockServiceMock;
    private readonly Mock<ILogger<CreateOrderHandler>> _loggerMock;
    private readonly CreateOrderHandler _handler;

    public CreateOrderHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _lockServiceMock = new Mock<IDistributedLockService>();
        _loggerMock = new Mock<ILogger<CreateOrderHandler>>();

        _handler = new CreateOrderHandler(
            _unitOfWorkMock.Object,
            _lockServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WhenCustomerNotFound_ShouldThrowException()
    {
        // Arrange
        var command = CreateValidCommand();
        _unitOfWorkMock.Setup(x => x.Customers.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenProductNotFound_ShouldThrowException()
    {
        // Arrange
        var command = CreateValidCommand();
        var customer = Customer.Create("test@example.com", "John", "Doe");

        _unitOfWorkMock.Setup(x => x.Customers.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _unitOfWorkMock.Setup(x => x.Products.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>());

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenLockCannotBeAcquired_ShouldThrowException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = CreateValidCommandWithProductId(productId);
        var customer = Customer.Create("test@example.com", "John", "Doe");
        var product = CreateProductWithId(productId, "Test Product", "SKU-001", 29.99m, 100);

        _unitOfWorkMock.Setup(x => x.Customers.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _unitOfWorkMock.Setup(x => x.Products.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { product });

        _lockServiceMock.Setup(x => x.AcquireLockAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IAsyncDisposable?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*lock*");
    }

    [Fact]
    public async Task Handle_WhenProductIsInactive_ShouldThrowException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = CreateValidCommandWithProductId(productId);
        var customer = Customer.Create("test@example.com", "John", "Doe");
        var product = CreateProductWithId(productId, "Test Product", "SKU-001", 29.99m, 100);
        product.Deactivate();

        SetupSuccessfulMocks(customer, new[] { product });

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*not available*");
    }

    [Fact]
    public async Task Handle_WhenInsufficientStock_ShouldThrowException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            ShippingAddress = new AddressDto
            {
                Street = "123 Main St",
                City = "City",
                State = "State",
                Country = "Country",
                ZipCode = "12345"
            },
            Items = new List<OrderItemDto>
            {
                new() { ProductId = productId, Quantity = 1000 }
            }
        };

        var customer = Customer.Create("test@example.com", "John", "Doe");
        var product = CreateProductWithId(productId, "Test Product", "SKU-001", 29.99m, 10);

        SetupSuccessfulMocks(customer, new[] { product });

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InsufficientStockException>();
    }

    [Fact]
    public async Task Handle_WhenValid_ShouldCreateOrderAndReturnResult()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var command = CreateValidCommandWithProductId(productId);

        var customer = Customer.Create("test@example.com", "John", "Doe");
        var product = CreateProductWithId(productId, "Test Product", "SKU-001", 29.99m, 100);

        SetupSuccessfulMocks(customer, new[] { product });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.OrderId.Should().NotBeEmpty();
        result.OrderNumber.Should().StartWith("ORD-");
        result.Total.Should().BeGreaterThan(0);

        _unitOfWorkMock.Verify(x => x.Orders.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static CreateOrderCommand CreateValidCommand()
    {
        return new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            ShippingAddress = new AddressDto
            {
                Street = "123 Main St",
                City = "City",
                State = "State",
                Country = "Country",
                ZipCode = "12345"
            },
            Items = new List<OrderItemDto>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 2 }
            }
        };
    }

    private static CreateOrderCommand CreateValidCommandWithProductId(Guid productId)
    {
        return new CreateOrderCommand
        {
            CustomerId = Guid.NewGuid(),
            ShippingAddress = new AddressDto
            {
                Street = "123 Main St",
                City = "City",
                State = "State",
                Country = "Country",
                ZipCode = "12345"
            },
            Items = new List<OrderItemDto>
            {
                new() { ProductId = productId, Quantity = 2 }
            }
        };
    }

    private static Product CreateProductWithId(Guid id, string name, string sku, decimal price, int stock)
    {
        var product = Product.Create(name, sku, price, stock);
        typeof(BaseEntity<Guid>).GetProperty("Id")!.SetValue(product, id);
        return product;
    }

    private void SetupSuccessfulMocks(Customer customer, IEnumerable<Product> products)
    {
        _unitOfWorkMock.Setup(x => x.Customers.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _unitOfWorkMock.Setup(x => x.Products.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(products.ToList());

        _lockServiceMock.Setup(x => x.AcquireLockAsync(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<IAsyncDisposable>());

        _unitOfWorkMock.Setup(x => x.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.Orders.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order o, CancellationToken _) => o);

        _unitOfWorkMock.Setup(x => x.Products.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _unitOfWorkMock.Setup(x => x.CommitTransactionAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }
}
