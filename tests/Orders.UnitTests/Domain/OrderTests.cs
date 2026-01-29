using FluentAssertions;
using Orders.Domain.Entities;
using Orders.Domain.Exceptions;
using Orders.Domain.ValueObjects;

namespace Orders.UnitTests.Domain;

public class OrderTests
{
    [Fact]
    public void Create_ShouldCreateOrderWithPendingStatus()
    {
        // Arrange
        var customer = Customer.Create("test@example.com", "John", "Doe");
        var address = Address.Create("123 Main St", "City", "State", "Country", "12345");

        // Act
        var order = Order.Create(customer, address);

        // Assert
        order.Should().NotBeNull();
        order.Id.Should().NotBeEmpty();
        order.OrderNumber.Should().StartWith("ORD-");
        order.CustomerId.Should().Be(customer.Id);
        order.Status.Should().Be(OrderStatus.Pending);
        order.ShippingAddress.Should().Be(address);
    }

    [Fact]
    public void AddItem_ShouldAddItemToOrder()
    {
        // Arrange
        var customer = Customer.Create("test@example.com", "John", "Doe");
        var address = Address.Create("123 Main St", "City", "State", "Country", "12345");
        var order = Order.Create(customer, address);
        var product = Product.Create("Test Product", "SKU-001", 29.99m, 100);

        // Act
        order.AddItem(product, 2);

        // Assert
        order.Items.Should().HaveCount(1);
        order.Items.First().ProductId.Should().Be(product.Id);
        order.Items.First().Quantity.Should().Be(2);
        order.SubTotal.Amount.Should().Be(59.98m);
    }

    [Fact]
    public void AddItem_WhenSameProductAddedTwice_ShouldUpdateQuantity()
    {
        // Arrange
        var customer = Customer.Create("test@example.com", "John", "Doe");
        var address = Address.Create("123 Main St", "City", "State", "Country", "12345");
        var order = Order.Create(customer, address);
        var product = Product.Create("Test Product", "SKU-001", 10.00m, 100);

        // Act
        order.AddItem(product, 2);
        order.AddItem(product, 3);

        // Assert
        order.Items.Should().HaveCount(1);
        order.Items.First().Quantity.Should().Be(5);
        order.SubTotal.Amount.Should().Be(50.00m);
    }

    [Fact]
    public void RemoveItem_ShouldRemoveItemFromOrder()
    {
        // Arrange
        var customer = Customer.Create("test@example.com", "John", "Doe");
        var address = Address.Create("123 Main St", "City", "State", "Country", "12345");
        var order = Order.Create(customer, address);
        var product = Product.Create("Test Product", "SKU-001", 29.99m, 100);
        order.AddItem(product, 2);

        // Act
        order.RemoveItem(product.Id);

        // Assert
        order.Items.Should().BeEmpty();
        order.SubTotal.Amount.Should().Be(0m);
    }

    [Fact]
    public void Confirm_ShouldChangeStatusToConfirmed()
    {
        // Arrange
        var customer = Customer.Create("test@example.com", "John", "Doe");
        var address = Address.Create("123 Main St", "City", "State", "Country", "12345");
        var order = Order.Create(customer, address);
        var product = Product.Create("Test Product", "SKU-001", 29.99m, 100);
        order.AddItem(product, 1);

        // Act
        order.Confirm();

        // Assert
        order.Status.Should().Be(OrderStatus.Confirmed);
        order.DomainEvents.Should().Contain(e => e.GetType().Name == "OrderCreatedEvent");
    }

    [Fact]
    public void Cancel_WhenPending_ShouldChangeStatusToCancelled()
    {
        // Arrange
        var customer = Customer.Create("test@example.com", "John", "Doe");
        var address = Address.Create("123 Main St", "City", "State", "Country", "12345");
        var order = Order.Create(customer, address);

        // Act
        order.Cancel();

        // Assert
        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenShipped_ShouldThrowException()
    {
        // Arrange
        var customer = Customer.Create("test@example.com", "John", "Doe");
        var address = Address.Create("123 Main St", "City", "State", "Country", "12345");
        var order = Order.Create(customer, address);
        var product = Product.Create("Test Product", "SKU-001", 29.99m, 100);
        order.AddItem(product, 1);
        order.Confirm();
        order.Process();
        order.Ship();

        // Act
        var act = () => order.Cancel();

        // Assert
        act.Should().Throw<InvalidOrderStateException>();
    }

    [Fact]
    public void StatusTransition_ShouldFollowCorrectOrder()
    {
        // Arrange
        var customer = Customer.Create("test@example.com", "John", "Doe");
        var address = Address.Create("123 Main St", "City", "State", "Country", "12345");
        var order = Order.Create(customer, address);
        var product = Product.Create("Test Product", "SKU-001", 29.99m, 100);
        order.AddItem(product, 1);

        // Act & Assert
        order.Status.Should().Be(OrderStatus.Pending);

        order.Confirm();
        order.Status.Should().Be(OrderStatus.Confirmed);

        order.Process();
        order.Status.Should().Be(OrderStatus.Processing);

        order.Ship();
        order.Status.Should().Be(OrderStatus.Shipped);

        order.Deliver();
        order.Status.Should().Be(OrderStatus.Delivered);
    }

    [Fact]
    public void AddItem_WhenOrderNotPending_ShouldThrowException()
    {
        // Arrange
        var customer = Customer.Create("test@example.com", "John", "Doe");
        var address = Address.Create("123 Main St", "City", "State", "Country", "12345");
        var order = Order.Create(customer, address);
        var product = Product.Create("Test Product", "SKU-001", 29.99m, 100);
        order.AddItem(product, 1);
        order.Confirm();

        // Act
        var act = () => order.AddItem(product, 1);

        // Assert
        act.Should().Throw<InvalidOrderStateException>();
    }
}
