using Xunit;
using GameTest.Models;
using GameTest.DTOs;

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
    public void Constructor_FromDTO_CreatesValidObject()
    {
        var tiles = new List<Tile>();
        var t1 = new Tile(ResourceType.Brick, 3, -3, -1);
        tiles.Add(t1);
        var t2 = new Tile(ResourceType.Wool, 4, -2, 0);
        tiles.Add(t2); 

        var expectedEdge = new Edge(t1, t2);

        var dto = new EdgeDTO(expectedEdge);

        // TODO: Add owner and direction to test all edge properties

        // Act
        var edge = new Edge(dto, new List<Player>(), tiles);

        // Assert
        Assert.Equal(expectedEdge.Id, edge.Id);
        Assert.True(edge.Tiles.Count == expectedEdge.Tiles.Count);
        Assert.True(edge.Tiles.Any(t => t.Id == t1.Id));
        Assert.True(edge.Tiles.Any(t => t.Id == t2.Id));
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

        Assert.NotNull(edge.Owner);
        string expectedResult = $"Edge {edge.Id} (Owner: {edge.Owner.Name})";
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void BuildRoad_SetsOwnerWhenNone()
    {
        // Arrange
        var player = new Player("Alice", PlayerColor.Red);
        var tile1 = new Tile(ResourceType.Brick, 8, 0, 0);
        var tile2 = new Tile(ResourceType.Wool, 5, 1, -1);
        var edge = new Edge(tile1, tile2);

        // Act
        bool result = edge.BuildRoad(player);

        // Assert
        Assert.True(result);
        Assert.NotNull(edge.Owner);
        Assert.Equal(player.Id, edge.Owner.Id);
    }

    [Fact]
    public void BuildRoad_FailsWhenOwnerAlreadySet()
    {
        // Arrange
        var player1 = new Player("Alice", PlayerColor.Red);
        var player2 = new Player("Bob", PlayerColor.Blue);
        var tile1 = new Tile(ResourceType.Brick, 8, 0, 0);
        var tile2 = new Tile(ResourceType.Wool, 5, 1, -1);
        var edge = new Edge(tile1, tile2);
        bool initialResult = edge.BuildRoad(player1);

        // Act
        bool result = edge.BuildRoad(player2);

        // Assert
        Assert.False(result);
        Assert.NotNull(edge.Owner);
        Assert.Equal(player1.Id, edge.Owner.Id);
    }
}