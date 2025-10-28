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
        Assert.Equal((1, -1), HexProximity.GetDirectionOffset(HexDirection.NE));
    }

    [Fact]
    public void GetDirectionOffset_E()
    {
        // Assert
        Assert.Equal((2, 0), HexProximity.GetDirectionOffset(HexDirection.E));
    }

    [Fact]
    public void GetDirectionOffset_SE()
    {
        // Assert
        Assert.Equal((1, 1), HexProximity.GetDirectionOffset(HexDirection.SE));
    }

    [Fact]
    public void GetDirectionOffset_SW()
    {
        // Assert
        Assert.Equal((-1, 1), HexProximity.GetDirectionOffset(HexDirection.SW));
    }

    [Fact]
    public void GetDirectionOffset_W()
    {
        // Assert
        Assert.Equal((-2, 0), HexProximity.GetDirectionOffset(HexDirection.W));
    }

    [Fact]
    public void GetDirectionOffset_NW()
    {
        // Assert
        Assert.Equal((-1, -1), HexProximity.GetDirectionOffset(HexDirection.NW));
    }

    [Fact]
    public void GetCoordinates_ToOrigin_FromNW()
    {
        // Arrange
        var origin = (-1, -1);

        // Act
        var result = HexProximity.GetCoordinates(origin, HexDirection.SE);

        // Assert
        Assert.Equal((0, 0), result);
    }

    [Fact]
    public void GetCoordinates_WestOfEastEdge()
    {
        // Arrange
        var origin = (4, 0);

        // Act
        var result = HexProximity.GetCoordinates(origin, HexDirection.W);

        // Assert
        Assert.Equal((2, 0), result);
    }

    [Fact]
    public void GetPrecedingDirection_FromNE()
    {
        // Act
        var result = HexProximity.getPrecedingDirection(HexDirection.NE);

        // Assert
        Assert.Equal(HexDirection.NW, result);
    }

    [Fact]
    public void GetPrecedingDirection_FromE()
    {
        // Act
        var result = HexProximity.getPrecedingDirection(HexDirection.E);

        // Assert
        Assert.Equal(HexDirection.NE, result);
    }

    [Fact]
    public void GetVertexDirectionForEdgeDirection_FromNE()
    {
        // Act
        var result = HexProximity.GetVertexDirectionForEdgeDirection(HexDirection.NE);

        // Assert
        Assert.Equal(VertexDirection.N, result);
    }

    [Fact] 
    public void GetVertexDirectionForEdgeDirection_FromE()
    {
        // Act
        var result = HexProximity.GetVertexDirectionForEdgeDirection(HexDirection.E);

        // Assert
        Assert.Equal(VertexDirection.NE, result);
    }

    [Fact] 
    public void GetVertexDirectionForEdgeDirection_FromSE()
    {
        // Act
        var result = HexProximity.GetVertexDirectionForEdgeDirection(HexDirection.SE);

        // Assert
        Assert.Equal(VertexDirection.SE, result);
    }

    [Fact] 
    public void GetVertexDirectionForEdgeDirection_FromSW()
    {
        // Act
        var result = HexProximity.GetVertexDirectionForEdgeDirection(HexDirection.SW);

        // Assert
        Assert.Equal(VertexDirection.S, result);
    }

    [Fact] 
    public void GetVertexDirectionForEdgeDirection_FromW()
    {
        // Act
        var result = HexProximity.GetVertexDirectionForEdgeDirection(HexDirection.W);

        // Assert
        Assert.Equal(VertexDirection.SW, result);
    }

    [Fact]
    public void GetVertexDirectionForEdgeDirection_FromNW()
    {
        // Act
        var result = HexProximity.GetVertexDirectionForEdgeDirection(HexDirection.NW);

        // Assert
        Assert.Equal(VertexDirection.NW, result);
    }

    [Fact]
    public void GetHexDirectionForVertexDirection_FromN()
    {
        // Act
        var result = HexProximity.GetHexDirectionForVertexDirection(VertexDirection.N);

        // Assert
        Assert.Equal(HexDirection.NE, result);
    }
    
    [Fact]
    public void GetHexDirectionForVertexDirection_FromNE()
    {
        // Act
        var result = HexProximity.GetHexDirectionForVertexDirection(VertexDirection.NE);

        // Assert
        Assert.Equal(HexDirection.E, result);
    }

    [Fact]
    public void GetHexDirectionForVertexDirection_FromNW()
    {
        // Act
        var result = HexProximity.GetHexDirectionForVertexDirection(VertexDirection.NW);

        // Assert
        Assert.Equal(HexDirection.NW, result);
    }
}