using Xunit;
using GameTest.Models;

namespace GameTest.Tests;

public class EdgeTests
{
    [Fact]
    public void Constructor_ValidParameters_CreatesExpectedEdge()
    {
        // Arrange

        // Act
        var edge = new Edge();

        // Assert
        Assert.True(edge.Id >= 1);
        Assert.False(edge.HasRoad);
        Assert.Null(edge.Owner);
        Assert.Equal(2, edge.Vertices.Length);
        Assert.All(edge.Vertices, e => Assert.Null(e));
        Assert.Equal(2, edge.Tiles.Length);
        Assert.All(edge.Tiles, t => Assert.Null(t));
    }

    [Fact]
    public void BuildRoad_SetsHasRoadToTrue()
    {
        // Arrange
        var edge = new Edge();

        // Act
        edge.BuildRoad();

        // Assert
        Assert.True(edge.HasRoad);
    }

    [Fact]
    public void BuildRoad_AlreadyHasRoad_DoesNotChangeState()
    {
        // Arrange
        var edge = new Edge();
        edge.BuildRoad(); // First build a road

        // Act
        edge.BuildRoad(); // Try to build again

        // Assert
        Assert.True(edge.HasRoad); // Should still be true
    }

    [Fact]
    public void RemoveRoad_SetsHasRoadToFalse()
    {
        // Arrange
        var edge = new Edge();
        edge.BuildRoad(); // First build a road

        // Act
        edge.RemoveRoad();

        // Assert
        Assert.False(edge.HasRoad);
    }

    [Fact]
    public void RemoveRoad_NoExistingRoad_DoesNotChangeState()
    {
        // Arrange
        var edge = new Edge();

        // Act
        edge.RemoveRoad(); // Try to remove when no road exists

        // Assert
        Assert.False(edge.HasRoad); // Should still be false
    }

    [Fact]
    public void ToString_ReturnsNonEmptyString()
    {
        // Arrange
        var edge = new Edge();

        // Act
        var result = edge.ToString();

        // Assert
        Assert.NotNull(result);

        string expectedResult = $"Edge {edge.Id} (";

        if (edge.HasRoad)
            if (edge.Owner != null)
                expectedResult += $"HasRoad: T; Owner: {edge.Owner.Name})";
            else
                expectedResult += "HasRoad: T)";
        else
            expectedResult += "HasRoad: F)";

        Assert.Equal(expectedResult, result);
    }
}