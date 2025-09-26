using Xunit;
using GameTest.Models;

namespace GameTest.Tests;

public class GameStateTests
{
    [Fact]
    public void Constructor_Default_EmptyState()
    {
        // Act
        var game = new GameState();

        // Assert
        Assert.Empty(game.Players);
        Assert.Empty(game.Tiles);
    }

    [Fact]
    public void AddPlayer_AddSinglePlayer()
    {
        // Arrange
        string expectedName = "Alice";

        // Act
        var game = new GameState();
        game.AddPlayer(new Player(expectedName, PlayerColor.Blue));

        // Assert
        Assert.NotEmpty(game.Players);
        Assert.Equal(expectedName, game.Players[0].Name);
    }

    [Fact]
    public void AddTile_AddSingleTile()
    {
        // Arrange
        ResourceType expectedResourceType = ResourceType.Wool;

        // Act
        var game = new GameState();
        game.AddTile(new Tile(expectedResourceType, 5, 0, 0));

        // Assert
        Assert.NotEmpty(game.Tiles);
        Assert.Equal(expectedResourceType, game.Tiles[0].Resource);
    }

    [Fact]
    public void AddEdge_AddSingleEdge()
    {
        // Arrange
        var tile = new Tile(ResourceType.Brick, 8, 0, 0);
        var edge = new Edge(new Player("PlayerA", PlayerColor.Red), tile);
        var expectedEdgeId = edge.Id;

        // Act
        var game = new GameState();
        game.AddEdge(edge);

        // Assert
        Assert.NotEmpty(game.Edges);
        Assert.Equal(expectedEdgeId, game.Edges[0].Id);
    }


    [Fact]
    public void AddVertex_AddSingleVertex()
    {
        // Arrange
        var vertex = new Vertex();
        var expectedVertexId = vertex.Id;

        // Act
        var game = new GameState();
        game.AddVertex(vertex);

        // Assert
        Assert.NotEmpty(game.Vertices);
        Assert.Equal(expectedVertexId, game.Vertices[0].Id);
    }    
}