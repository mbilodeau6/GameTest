using Xunit;
using GameTest.Models;

namespace GameTest.Tests;

public class EdgeTests
{
    [Fact]
    public void Constructor_WithOwner_CreatesExpectedEdge()
    {
        // Arrange
        var owner = new Player("Alice", PlayerColor.Red);
        var tile1 = new Tile(ResourceType.Brick, 8, 0, 0);
        var tile2 = new Tile(ResourceType.Wool, 5, 1, -1);

        // Act
        var edge = new Edge(owner, tile1, tile2);

        // Assert
        Assert.True(TestHelpers.ValidateId(edge.Id, 'E'));
        Assert.Equal(2, edge.Vertices.Length);
        Assert.All(edge.Vertices, e => Assert.Null(e));
        Assert.Equal(2, edge.Tiles.Count);
        Assert.All(edge.Tiles, t => Assert.NotNull(t));
    }

    [Fact]
    public void Constructor_TwoTiles_CreatesExpectedEdge()
    {
        // Arrange
        var tile1 = new Tile(ResourceType.Brick, 8, 0, 0);
        var tile2 = new Tile(ResourceType.Wool, 5, 1, -1);

        // Act
        var edge = new Edge(tile1, tile2);

        // Assert
        Assert.True(TestHelpers.ValidateId(edge.Id, 'E'));
        Assert.Equal(2, edge.Vertices.Length);
        Assert.All(edge.Vertices, e => Assert.Null(e));
        Assert.Equal(2, edge.Tiles.Count);
        Assert.All(edge.Tiles, t => Assert.NotNull(t));
    }

    [Fact]
    public void Constructor_OneTile_CreatesExpectedEdge()
    {
        // Arrange
        var tile1 = new Tile(ResourceType.Brick, 8, 0, 0);

        // Act
        var edge = new Edge(tile1, HexDirection.NE);

        // Assert
        Assert.True(TestHelpers.ValidateId(edge.Id, 'E'));
        Assert.Single(edge.Tiles);
        Assert.Equal(HexDirection.NE, edge.Direction);
        Assert.All(edge.Tiles, t => Assert.NotNull(t));
    }

    
    [Fact]
    public void ToString_ReturnsNonEmptyString()
    {
        // Arrange
        var owner = new Player("Alice", PlayerColor.Red);
        var tile1 = new Tile(ResourceType.Brick, 8, 0, 0);
        var tile2 = new Tile(ResourceType.Wool, 5, 1, -1);
        var edge = new Edge(owner, tile1, tile2);

        // Act
        var result = edge.ToString();

        // Assert
        Assert.NotNull(result);

        string expectedResult = $"Edge {edge.Id} (Owner: {edge.Owner.Name})";
        Assert.Equal(expectedResult, result);
    }
}