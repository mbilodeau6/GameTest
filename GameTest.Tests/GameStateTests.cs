using Xunit;
using GameTest.Models;

namespace GameTest.Tests;

public class GameStateTests
{
    [Fact]
    public void Constructor_Default_EmptyState()
    {
        // Act
        var game = new GameState(Guid.NewGuid());

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
        var game = new GameState(Guid.NewGuid());
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
        var game = new GameState(Guid.NewGuid());
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
        var edge = new Edge(tile, HexDirection.NE);
        var expectedEdgeId = edge.Id;

        // Act
        var game = new GameState(Guid.NewGuid());
        game.AddEdge(edge);

        // Assert
        Assert.NotEmpty(game.Edges);
        Assert.Equal(expectedEdgeId, game.Edges[0].Id);
    }


    [Fact]
    public void AddVertex_AddSingleVertex()
    {
        // Arrange
        var player = new Player("PlayerA", PlayerColor.Red);
        var tile = new Tile(ResourceType.Brick, 8, 0, 0);
        var vertex = new Vertex(player, tile);
        var expectedVertexId = vertex.Id;

        // Act
        var game = new GameState(Guid.NewGuid());
        game.AddVertex(vertex);

        // Assert
        Assert.NotEmpty(game.Vertices);
        Assert.Equal(expectedVertexId, game.Vertices[0].Id);
    }

    [Fact]
    public void SetRobberTileId_ValidLocation()
    {
        // Arrange
        var tile = new Tile(ResourceType.Brick, 8, 0, 0);
        var tileId = tile.Id;
        var game = new GameState(Guid.NewGuid());
        game.AddTile(tile);

        // Act
        game.SetRobberTile(tileId);

        // Assert
        Assert.Equal(tileId, game.RobberTileId);
    } 

    [Fact]
    public void SetRobberTileId_NonexistentTile()
    {
        // Arrange
        var tile = new Tile(ResourceType.Brick, 8, 0, 0);
        var tileId = tile.Id;
        var game = new GameState(Guid.NewGuid());
        game.AddTile(tile);

        // Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            game.SetRobberTile("TX"));

        Assert.Equal("The specified tile does not exist in the game.", exception.Message);
    } 

    [Fact]
    public void SetRobberTileId_AlreadySet()
    {
        // Arrange
        var tile = new Tile(ResourceType.Brick, 8, 0, 0);
        var tileId = tile.Id;
        var game = new GameState(Guid.NewGuid());
        game.AddTile(tile);
        game.SetRobberTile(tileId);

        // Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            game.SetRobberTile(tileId));

        Assert.Equal("Robber is already on the specified tile.", exception.Message);
    } 
}