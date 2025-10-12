using Xunit;
using GameTest.Models;
using GameTest.DTOs;

namespace GameTest.Tests;

public class GameStateTests
{
    [Fact]
    public void Constructor_DefaultNoGameType_EmptyState()
    {
        // Act
        var game = new GameState(Guid.NewGuid());

        // Assert
        Assert.Empty(game.Players);
        Assert.Empty(game.Tiles);
        Assert.Equal(GameType.Default, game.Type);
    }

    [Fact]
    public void Constructor_DefaultWithGameType_EmptyState()
    {
        // Act
        var game = new GameState(Guid.NewGuid(), GameType.Starter);

        // Assert
        Assert.Empty(game.Players);
        Assert.Empty(game.Tiles);
        Assert.Equal(GameType.Starter, game.Type);
    }

    [Fact]
    public void Constructor_FromDTO_CreatesValidObject()
    {
        // Arrange
        var dto = new GameStateDTO(Guid.NewGuid().ToString(), GameType.Default.ToString());

        // Act
        var game = new GameState(dto);

        // Assert
        Assert.Equal(dto.Id, game.Id.ToString());
        Assert.Equal(dto.Type, game.Type.ToString());

        // TODO: Add tests to verify players, tiles, edges, vertices once those DTOs are implemented
        Assert.Empty(game.Players);
        Assert.Empty(game.Tiles);
        Assert.Empty(game.Edges);
        Assert.Empty(game.Vertices);
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

    [Fact]
    public void PlaceRobberOnDesert_OneDesert()
    {
        // Arrange
        var desertTile = new Tile(ResourceType.Desert, 0, 0, 0);
        var game = new GameState(Guid.NewGuid());
        game.AddTile(desertTile);

        // Act
        game.PlaceRobberOnDesert();

        // Assert
        Assert.Equal(desertTile.Id, game.RobberTileId);
    }

    [Fact]
    public void PlaceRobberOnDesert_TwoDeserts()
    {
        // Arrange
        var desertTile1 = new Tile(ResourceType.Desert, 0, 0, 0);
        var desertTile2 = new Tile(ResourceType.Desert, 0, 1, -1);
        var game = new GameState(Guid.NewGuid());
        game.AddTile(desertTile1);
        game.AddTile(desertTile2);

        // Assert
        game.PlaceRobberOnDesert();

        Assert.True(game.RobberTileId == desertTile1.Id || game.RobberTileId == desertTile2.Id);
    }

    [Fact]
    public void PlaceRobberOnDesert_NoDesert_ThrowsInvalidOperationException()
    {
        // Arrange
        var tile = new Tile(ResourceType.Brick, 8, 0, 0);
        var game = new GameState(Guid.NewGuid());
        game.AddTile(tile);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => game.PlaceRobberOnDesert());
        Assert.Equal("No desert tile found in the game.", exception.Message);
    }
}