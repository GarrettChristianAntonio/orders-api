using FluentAssertions;
using Orders.Domain.ValueObjects;

namespace Orders.UnitTests.Domain;

public class MoneyTests
{
    [Fact]
    public void Create_WithValidAmount_ShouldCreateMoney()
    {
        // Act
        var money = Money.Create(100.50m, "USD");

        // Assert
        money.Amount.Should().Be(100.50m);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldThrowException()
    {
        // Act
        var act = () => Money.Create(-10m);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Add_SameCurrency_ShouldReturnSum()
    {
        // Arrange
        var money1 = Money.Create(100m);
        var money2 = Money.Create(50m);

        // Act
        var result = money1.Add(money2);

        // Assert
        result.Amount.Should().Be(150m);
    }

    [Fact]
    public void Add_DifferentCurrency_ShouldThrowException()
    {
        // Arrange
        var money1 = Money.Create(100m, "USD");
        var money2 = Money.Create(50m, "EUR");

        // Act
        var act = () => money1.Add(money2);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Subtract_SameCurrency_ShouldReturnDifference()
    {
        // Arrange
        var money1 = Money.Create(100m);
        var money2 = Money.Create(30m);

        // Act
        var result = money1.Subtract(money2);

        // Assert
        result.Amount.Should().Be(70m);
    }

    [Fact]
    public void Subtract_ResultNegative_ShouldThrowException()
    {
        // Arrange
        var money1 = Money.Create(30m);
        var money2 = Money.Create(100m);

        // Act
        var act = () => money1.Subtract(money2);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Multiply_ShouldReturnProduct()
    {
        // Arrange
        var money = Money.Create(25m);

        // Act
        var result = money.Multiply(4);

        // Assert
        result.Amount.Should().Be(100m);
    }

    [Fact]
    public void Zero_ShouldReturnZeroAmount()
    {
        // Act
        var money = Money.Zero();

        // Assert
        money.Amount.Should().Be(0m);
    }

    [Theory]
    [InlineData(10.999, 11.00)]
    [InlineData(10.001, 10.00)]
    [InlineData(10.125, 10.12)] // Banker's rounding
    public void Create_ShouldRoundToTwoDecimalPlaces(decimal input, decimal expected)
    {
        // Act
        var money = Money.Create(input);

        // Assert
        money.Amount.Should().Be(expected);
    }
}
