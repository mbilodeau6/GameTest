using Xunit;
using GameTest.Models;
using GameTest.DTOs;

namespace GameTest.Tests;

public class GameStateDTOTests
{
    [Fact]
    public void GameStateDTO_ValidGameState_CreatesDTO()
    {
        // Arrange
        var gameState = new GameState();
        var player1 = new Player("Alice", PlayerColor.Blue);
        var player2 = new Player("Bob", PlayerColor.Red);
        gameState.Players.Add(player1);
        gameState.Players.Add(player2);

        var tile1 = new Tile(ResourceType.Brick, 8, 0, 0);
        var tile2 = new Tile(ResourceType.Wood, 5, -1, 0);
        gameState.Tiles.Add(tile1);
        gameState.Tiles.Add(tile2);

        gameState.Edges.Add(new Edge(player1, tile1, tile2));
        gameState.Edges.Add(new Edge(player2, tile1));
        gameState.Edges.Add(new Edge(player1, tile2));

        gameState.Vertices.Add(new Vertex(player2, tile1, tile2));

        // Act
        var gameStateDTO = new GameStateDTO(gameState);

        // Assert
        // TODO: Should add tests to check specific instances of players, tiles, edges, vertices
        Assert.Equal(2, gameStateDTO.Players.Count);
        Assert.Equal("Red", gameStateDTO.Players[1].Color);
        Assert.Equal(2, gameStateDTO.Tiles.Count);
        Assert.Equal("Brick", gameStateDTO.Tiles[0].Resource);
        Assert.Equal(3, gameStateDTO.Edges.Count);
        Assert.Single(gameStateDTO.Vertices);
        Assert.Equal("Settlement", gameStateDTO.Vertices[0].Building);
    }
}
