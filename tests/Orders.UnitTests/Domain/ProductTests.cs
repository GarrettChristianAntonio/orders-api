using FluentAssertions;
using Orders.Domain.Entities;
using Orders.Domain.Exceptions;

namespace Orders.UnitTests.Domain;

public class ProductTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateProduct()
    {
        // Act
        var product = Product.Create("Test Product", "SKU-001", 29.99m, 100, "Description");

        // Assert
        product.Should().NotBeNull();
        product.Id.Should().NotBeEmpty();
        product.Name.Should().Be("Test Product");
        product.Sku.Should().Be("SKU-001");
        product.Price.Amount.Should().Be(29.99m);
        product.StockQuantity.Should().Be(100);
        product.Description.Should().Be("Description");
        product.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrowException()
    {
        // Act
        var act = () => Product.Create("", "SKU-001", 29.99m, 100);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithNegativeStock_ShouldThrowException()
    {
        // Act
        var act = () => Product.Create("Test", "SKU-001", 29.99m, -1);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ReserveStock_WithSufficientStock_ShouldReduceStock()
    {
        // Arrange
        var product = Product.Create("Test", "SKU-001", 29.99m, 100);

        // Act
        product.ReserveStock(30);

        // Assert
        product.StockQuantity.Should().Be(70);
    }

    [Fact]
    public void ReserveStock_WithInsufficientStock_ShouldThrowException()
    {
        // Arrange
        var product = Product.Create("Test", "SKU-001", 29.99m, 10);

        // Act
        var act = () => product.ReserveStock(20);

        // Assert
        act.Should().Throw<InsufficientStockException>();
    }

    [Fact]
    public void ReleaseStock_ShouldIncreaseStock()
    {
        // Arrange
        var product = Product.Create("Test", "SKU-001", 29.99m, 100);

        // Act
        product.ReleaseStock(50);

        // Assert
        product.StockQuantity.Should().Be(150);
    }

    [Fact]
    public void HasSufficientStock_WhenStockAvailable_ShouldReturnTrue()
    {
        // Arrange
        var product = Product.Create("Test", "SKU-001", 29.99m, 100);

        // Act & Assert
        product.HasSufficientStock(50).Should().BeTrue();
        product.HasSufficientStock(100).Should().BeTrue();
        product.HasSufficientStock(101).Should().BeFalse();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        // Arrange
        var product = Product.Create("Test", "SKU-001", 29.99m, 100);

        // Act
        product.Deactivate();

        // Assert
        product.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Update_ShouldUpdateProductDetails()
    {
        // Arrange
        var product = Product.Create("Test", "SKU-001", 29.99m, 100);

        // Act
        product.Update("Updated Name", "Updated Description", 39.99m);

        // Assert
        product.Name.Should().Be("Updated Name");
        product.Description.Should().Be("Updated Description");
        product.Price.Amount.Should().Be(39.99m);
    }
}
