using Xunit;
using GameTest.Models;
using GameTest.DTOs;
using Grpc.Core;

namespace GameTest.Tests;

public class GameStateDTOTests
{
    private GameState CreateTestGameState()
    {
        Guid guid = Guid.NewGuid();
        var gameState = new GameState(guid);
        var player1 = new Player("Alice", PlayerColor.Blue);
        var player2 = new Player("Bob", PlayerColor.Red, true);
        gameState.Players.Add(player1);
        gameState.Players.Add(player2);

        var tile1 = new Tile(ResourceType.Brick, 8, 0, 0);
        var tile2 = new Tile(ResourceType.Wood, 5, -1, -1);
        gameState.Tiles.Add(tile1);
        gameState.Tiles.Add(tile2);

        gameState.Edges.Add(new Edge(tile1, tile2));
        gameState.Edges.Add(new Edge(tile1, HexDirection.NE));
        gameState.Edges.Add(new Edge(tile2, HexDirection.SW));

        gameState.Vertices.Add(new Vertex(tile1, tile2));
        gameState.Vertices[0].BuildSettlement(player1);
        gameState.SetRobberTile(tile2);
        gameState.Vertices.Add(new Vertex(tile1, VertexDirection.SW));

        gameState.Ports.Add(new Port(gameState.Vertices[0], gameState.Vertices[1], PortType.Ore));

        gameState.Players[0].AssignDevelopmentCard(DevelopmentCardType.RoadBuilding);
        gameState.Players[0].AssignResources(ResourceType.Ore, 2);
        gameState.Players[1].AssignDevelopmentCard(DevelopmentCardType.Knight);
        gameState.Players[1].AssignDevelopmentCard(DevelopmentCardType.Knight);
        gameState.Players[1].AssignResources(ResourceType.Brick, 1);

        gameState.Phase = new GamePhase(GameStates.SettingUpBoard, player1);

        return gameState;
    }
    
    [Fact]
    public void GameStateDTO_ValidGameState_CreatesDTO()
    {
        // Arrange
        var gameState = CreateTestGameState();

        // Act
        var gameStateDTO = new GameStateDTO(gameState);

        // Assert
        // TODO: Should add tests to check specific instances of players, tiles, edges, vertices
        Assert.Equal(2, gameStateDTO.Players.Count);
        Assert.Equal("Red", gameStateDTO.Players[1].Color);
        Assert.Equal(2, gameStateDTO.Tiles.Count);
        Assert.Equal("Brick", gameStateDTO.Tiles[0].Resource);
        Assert.Equal(3, gameStateDTO.Edges.Count);
        Assert.Equal(2, gameStateDTO.Vertices.Count);
        Assert.Equal("Settlement", gameStateDTO.Vertices[0].Building);
        Assert.Equal(gameState.Id.ToString(), gameStateDTO.Id);
        Assert.Equal("Default", gameStateDTO.Settings.Type.ToString());
        Assert.Equal(gameState.Tiles[1].Id, gameStateDTO.RobberTileId);
        Assert.Equal(gameState.Settings.Type.ToString(), gameStateDTO.Settings.Type);
        Assert.NotNull(gameState.Phase);
        if (gameState.Phase.CurrentPlayer != null) 
            Assert.Equal(gameState.Phase.CurrentPlayer.Id, gameStateDTO.Phase.CurrentPlayerId);
        Assert.Equal(gameState.Phase.PhaseState.ToString(), gameStateDTO.Phase.PhaseState);
        Assert.True(gameStateDTO.Dice.Die1.Random);
        Assert.Single(gameStateDTO.Ports);
        Assert.Equal(PortType.Ore.ToString(), gameStateDTO.Ports[0].Type);
    }

    [Fact]
    public void GameStateDTO_RobberTileSet()
    {
        // Arrange
        Guid guid = Guid.NewGuid();
        var gameState = new GameState(guid);
        var tile1 = new Tile(ResourceType.Brick, 8, 0, 0);
        var tile2 = new Tile(ResourceType.Wood, 5, -1, 0);
        gameState.Tiles.Add(tile1);
        gameState.Tiles.Add(tile2);
        gameState.SetRobberTile(tile1);

        // Act
        var gameStateDTO = new GameStateDTO(gameState);

        // Assert
        Assert.Equal(2, gameStateDTO.Tiles.Count);
        Assert.Equal(tile1.Id, gameStateDTO.RobberTileId);
    }

    [Fact]
    public void GetStateForPlayer_HideBotValues()
    {
        // Arrange
        var gameState = CreateTestGameState();
        var gsFullDTO = new GameStateDTO(gameState);

        // Act
        var gsTransformed = gsFullDTO.GetStateForPlayer(gameState.Players[0]);

        // Assert
        Assert.False(gsTransformed.Players[0].IsBot);
        Assert.True(gsTransformed.Players[1].IsBot);
        Assert.Equal(2, gsTransformed.Players[0].Resources[ResourceType.Ore]);
        Assert.Equal(1, gsTransformed.Players[0].DevelopmentCards[DevelopmentCardType.RoadBuilding]);
        Assert.Equal(1, gsTransformed.Players[0].DevelopmentCardCount);
        Assert.Equal(2, gsTransformed.Players[0].ResourceCount);
        Assert.Equal(2, gsTransformed.Players[1].DevelopmentCardCount);
        Assert.Equal(1, gsTransformed.Players[1].ResourceCount);
        Assert.True(gsTransformed.Players[1].DevelopmentCards.Values.All(v => v == 0));
        Assert.True(gsTransformed.Players[1].Resources.Values.All(v => v == 0));
    }
}
