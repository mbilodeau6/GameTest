using Xunit;
using GameTest.Models;
using GameTest.DTOs;

namespace GameTest.Tests;

public class EdgeTests
{
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
        Assert.NotNull(edge.Vertices);
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

        var expectedEdge = new Edge(t1, HexDirection.W);

        var p1 = new Player("Alice", PlayerColor.Red);
        var p2 = new Player("Bob", PlayerColor.Blue);
        expectedEdge.BuildRoad(p1);

        var dto = new EdgeDTO(expectedEdge);

        // Act
        var edge = new Edge(dto, new List<Player>() {p1, p2}, tiles);

        // Assert
        Assert.Equal(expectedEdge.Id, edge.Id);
        Assert.True(edge.Tiles.Count == expectedEdge.Tiles.Count);
        Assert.Contains(edge.Tiles, t => t.Id == t1.Id);
        Assert.Equal(expectedEdge.Direction, edge.Direction);
        Assert.NotNull(edge.Owner);
        Assert.Equal(p1.Id, edge.Owner.Id);
    }


    [Fact]
    public void ToString_ReturnsNonEmptyString()
    {
        // Arrange
        var owner = new Player("Alice", PlayerColor.Red);
        var tile1 = new Tile(ResourceType.Brick, 8, 0, 0);
        var tile2 = new Tile(ResourceType.Wool, 5, 1, -1);
        var edge = new Edge(tile1, tile2);
        edge.BuildRoad(owner);

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

    [Fact]
    public void AddVertexReference_Valid()
    {
        var gs = TestHelpers.CreateEdgesAndVertexForRefTests();

        var result = gs.Edges[0].AddVertexReference(gs.Vertices[0]);
        Assert.True(result);

        result = gs.Edges[0].AddVertexReference(gs.Vertices[1]);
        Assert.True(result);

        Assert.Contains(gs.Vertices[0], gs.Edges[0].Vertices);
        Assert.Contains(gs.Vertices[1], gs.Edges[0].Vertices);
    }

    [Fact]
    public void AddVertexReference_TooMany()
    {
        var gs = TestHelpers.CreateEdgesAndVertexForRefTests();

        var result = gs.Edges[0].AddVertexReference(gs.Vertices[0]);
        Assert.True(result);

        result = gs.Edges[0].AddVertexReference(gs.Vertices[1]);
        Assert.True(result);

        result = gs.Edges[0].AddVertexReference(gs.Vertices[2]);
        Assert.False(result);

        Assert.Contains(gs.Vertices[0], gs.Edges[0].Vertices);
        Assert.Contains(gs.Vertices[1], gs.Edges[0].Vertices);
        Assert.DoesNotContain(gs.Vertices[2], gs.Edges[0].Vertices);
    }

    [Fact]
    public void AddVertexReference_Duplicate()
    {
        var gs = TestHelpers.CreateEdgesAndVertexForRefTests();

        var result = gs.Edges[0].AddVertexReference(gs.Vertices[0]);
        Assert.True(result);

        result = gs.Edges[0].AddVertexReference(gs.Vertices[0]);
        Assert.False(result);

        Assert.Contains(gs.Vertices[0], gs.Edges[0].Vertices);
    }
}