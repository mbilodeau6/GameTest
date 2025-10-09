using Xunit;
using GameTest.Services;
using GameTest.Models;

namespace GameTest.Tests;

public class HexProximityTests
{
    [Fact]
    public void GetDirectionOffset_NE()
    {
        // Assert
        Assert.Equal((1, -1), HexProximity.GetDirectionOffset(HexProximity.Direction.NE));
    }

    [Fact]
    public void GetDirectionOffset_E()
    {
        // Assert
        Assert.Equal((2, 0), HexProximity.GetDirectionOffset(HexProximity.Direction.E));
    }

    [Fact]
    public void GetDirectionOffset_SE()
    {
        // Assert
        Assert.Equal((1, 1), HexProximity.GetDirectionOffset(HexProximity.Direction.SE));
    }

    [Fact]
    public void GetDirectionOffset_SW()
    {
        // Assert
        Assert.Equal((-1, 1), HexProximity.GetDirectionOffset(HexProximity.Direction.SW));
    }

    [Fact]
    public void GetDirectionOffset_W()
    {
        // Assert
        Assert.Equal((-2, 0), HexProximity.GetDirectionOffset(HexProximity.Direction.W));
    }

    [Fact]
    public void GetDirectionOffset_NW()
    {
        // Assert
        Assert.Equal((-1, -1), HexProximity.GetDirectionOffset(HexProximity.Direction.NW));
    }

    [Fact]
    public void GetCoordinates_ToOrigin_FromNW()
    {
        // Arrange
        var origin = (-1, -1);

        // Act
        var result = HexProximity.GetCoordinates(origin, HexProximity.Direction.SE);

        // Assert
        Assert.Equal((0, 0), result);
    }

    [Fact]
    public void GetCoordinates_WestOfEastEdge()
    {
        // Arrange
        var origin = (4, 0);

        // Act
        var result = HexProximity.GetCoordinates(origin, HexProximity.Direction.W);

        // Assert
        Assert.Equal((2, 0), result);
    }

    [Fact]
    public void GetPrecedingDirection_FromNE()
    {
        // Act
        var result = HexProximity.getPrecedingDirection(HexProximity.Direction.NE);

        // Assert
        Assert.Equal(HexProximity.Direction.NW, result);
    }

    [Fact]
    public void GetPrecedingDirection_FromE()
    {
        // Act
        var result = HexProximity.getPrecedingDirection(HexProximity.Direction.E);

        // Assert
        Assert.Equal(HexProximity.Direction.NE, result);
    }
}