using Xunit;
using GameTest.Models;
using GameTest.DTOs;

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
        Assert.True(TestHelpers.ValidateId(tile.Id, 'T'));
        Assert.Equal(resource, tile.Resource);
        Assert.Equal(diceNumber, tile.DiceNumber);
        Assert.Equal(x, tile.X);
        Assert.Equal(y, tile.Y);
    }

    [Fact]
    public void Constructor_DesertTile_HasRobberAndDiceNumberSeven()
    {
        // Arrange & Act
        var tile = new Tile(ResourceType.Desert, 2, 0, 0);

        // Assert
        Assert.True(TestHelpers.ValidateId(tile.Id, 'T'));
        Assert.Equal(ResourceType.Desert, tile.Resource);
        Assert.Equal(7, tile.DiceNumber);
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

    [Fact]
    public void Constructor_DiceNumberIgnoredForDesertTile()
    {
        // Arrange & Act
        var tile = new Tile(ResourceType.Desert, 0, 0, 0);

        // Assert
        Assert.True(TestHelpers.ValidateId(tile.Id, 'T'));
        Assert.Equal(ResourceType.Desert, tile.Resource);
        Assert.Equal(7, tile.DiceNumber);
    }

    [Fact]
    public void ToString_ReturnsExpectedFormat()
    {
        // Arrange
        var tile = new Tile(ResourceType.Ore, 10, -1, -1);
        var desertTile = new Tile(ResourceType.Desert, 2, 1, 0);

        // Act
        var tileString = tile.ToString();
        var desertTileString = desertTile.ToString();

        // Assert
        Assert.Equal("Ore (-1,-1)(10)", tileString);
        Assert.Equal("Desert (1,0)(7)", desertTileString);
    }

    [Fact]
    public void Constructor_FromDTO_CreatesValidObject()
    {
        var expectedTile = new Tile(ResourceType.Brick, 3, -3, -1);

        var dto = new TileDTO(expectedTile);

        // Act
        var tile = new Tile(dto);

        // Assert
        Assert.Equal(expectedTile.Id, tile.Id);
        Assert.Equal(expectedTile.Resource, tile.Resource);
        Assert.Equal(expectedTile.DiceNumber, tile.DiceNumber);
        Assert.Equal(expectedTile.X, tile.X);
        Assert.Equal(expectedTile.Y, tile.Y);
    }
}