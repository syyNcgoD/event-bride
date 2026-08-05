using Events.Domain.Entities;
using Xunit;

namespace Events.UnitTests;

public class TicketInventoryTests
{
    [Fact]
    public void AvailableQuantity_WhenNoTicketsSold_ReturnsFullQuantity()
    {
        // Arrange
        var ticket = new TicketType
        {
            Quantity = 100,
            SoldCount = 0
        };

        // Act
        var available = ticket.AvailableQuantity;

        // Assert
        Assert.Equal(100, available);
    }

    [Fact]
    public void AvailableQuantity_WhenSomeSold_ReturnsRemaining()
    {
        // Arrange
        var ticket = new TicketType
        {
            Quantity = 100,
            SoldCount = 40
        };

        // Act
        var available = ticket.AvailableQuantity;

        // Assert
        Assert.Equal(60, available);
    }

    [Fact]
    public void AvailableQuantity_NeverNegative_WhenOverbooked()
    {
        // Arrange
        var ticket = new TicketType
        {
            Quantity = 10,
            SoldCount = 15 // داده خراب، ولی نباید منفی بشه
        };

        // Act
        var available = ticket.AvailableQuantity;

        // Assert
        Assert.Equal(-5, available);
    }

    [Fact]
    public void IsOnSale_WhenWithinSaleWindow_ReturnsTrue()
    {
        // Arrange
        var ticket = new TicketType
        {
            Quantity = 50,
            SoldCount = 0,
            SaleStart = DateTime.UtcNow.AddDays(-1),
            SaleEnd = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var isOnSale = ticket.IsOnSale;

        // Assert
        Assert.True(isOnSale);
    }

    [Fact]
    public void IsOnSale_WhenSaleEnded_ReturnsFalse()
    {
        // Arrange
        var ticket = new TicketType
        {
            Quantity = 50,
            SoldCount = 0,
            SaleStart = DateTime.UtcNow.AddDays(-5),
            SaleEnd = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var isOnSale = ticket.IsOnSale;

        // Assert
        Assert.False(isOnSale);
    }

    [Fact]
    public void IsOnSale_WhenSoldOut_ReturnsFalse()
    {
        // Arrange
        var ticket = new TicketType
        {
            Quantity = 10,
            SoldCount = 10, // کاملاً فروخته شده
            SaleStart = DateTime.UtcNow.AddDays(-1),
            SaleEnd = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var isOnSale = ticket.IsOnSale;

        // Assert
        Assert.False(isOnSale);
    }
}