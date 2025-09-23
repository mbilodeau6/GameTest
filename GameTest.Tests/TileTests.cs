using Xunit;
using GameTest.Models;

namespace GameTest.Tests;

public class TileTests
{
    [Fact]
    public void Constructor_ValidParameters_CreatesExpectedTile()
    {
        // Arrange
        var resource = ResourceType.Grain;
        var diceNumber = 8;
        var x = 2;
        var y = 3;

        // Act
        var tile = new Tile(resource, diceNumber, x, y);

        // Assert
        Assert.Equal(resource, tile.Resource);
        Assert.Equal(diceNumber, tile.DiceNumber);
        Assert.Equal(x, tile.X);
        Assert.Equal(y, tile.Y);
        Assert.False(tile.HasRobber);
    }

    [Fact]
    public void Constructor_DesertTile_HasRobberAndDiceNumberSeven()
    {
        // Arrange & Act
        var tile = new Tile(ResourceType.Desert, 2, 0, 0);

        // Assert
        Assert.Equal(ResourceType.Desert, tile.Resource);
        Assert.Equal(7, tile.DiceNumber);
        Assert.True(tile.HasRobber);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(0)]
    [InlineData(13)]
    public void Constructor_InvalidDiceNumber_ThrowsArgumentException(int invalidDiceNumber)
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new Tile(ResourceType.Wood, invalidDiceNumber, 0, 0));
        
        Assert.Equal("Dice number must be between 2 and 12 (Parameter 'diceNumber')", exception.Message);
    }
}