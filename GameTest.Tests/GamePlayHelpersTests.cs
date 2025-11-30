using Xunit;
using GameTest.Models;
using GameTest.DTOs;
using GameTest.Services;
using GameTest.Functions;
using Microsoft.VisualStudio.TestPlatform.Common.ExtensionFramework;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace GameTest.Tests;

public class GamePlayHelpersTests
{
    [Fact]
    public void GetResourcesEarnedOnLastRoll_NoBuildingsOnMatchingTiles()
    {
        // Arrage
        var gameState = BoardCreationHelpers.CreateNewBoard(GameType.Test);
        gameState.SetDiceForTesting(new GameDice(new GameDie(3), new GameDie(5)));

        // Act
        var resources = GamePlayHelpers.GetResourcesEarnedOnLastRoll(gameState);

        // Assert
        Assert.Empty(resources);
    }

    [Fact]
    public void GetResourcesEarnedOnLastRoll_BuildingsOnMatchingTiles()
    {
        // Arrage
        var gameState = BoardCreationHelpers.CreateNewBoard(GameType.Test);

        var targetTile = BoardCreationHelpers.GetTileAt(gameState.Tiles, 0, 0);
        Assert.NotNull(targetTile);
        Assert.Equal(9, targetTile.DiceNumber);

        var bluePlayer = gameState.Players.First(p => p.Color == PlayerColor.Blue);
        Assert.NotNull(bluePlayer);
        var redPlayer = gameState.Players.First(p => p.Color == PlayerColor.Red);
        Assert.NotNull(redPlayer);

        var blueVertex = gameState.Vertices.First(v => v.Tiles.Contains(targetTile) && v.Owner == bluePlayer);
        Assert.NotNull(blueVertex);
        Assert.Equal(BuildingType.Settlement, blueVertex.Building);
        blueVertex.UpgradeToCity();

        gameState.SetDiceForTesting(new GameDice(new GameDie(4), new GameDie(5)));

        // Act
        var resources = GamePlayHelpers.GetResourcesEarnedOnLastRoll(gameState);

        // Assert
        Assert.NotEmpty(resources);
        Assert.True(resources.ContainsKey(bluePlayer));
        Assert.True(resources.ContainsKey(redPlayer));
        Assert.True(resources[redPlayer].ContainsKey(ResourceType.Grain));
        Assert.Equal(1, resources[redPlayer][ResourceType.Grain]);
        Assert.True(resources[bluePlayer].ContainsKey(ResourceType.Grain));
        Assert.Equal(2, resources[bluePlayer][ResourceType.Grain]);
    }

    [Fact]
    public void GetResourcesEarnedOnLastRoll_DontIncludeDesert()
    {
        // Arrage
        var gameState = BoardCreationHelpers.CreateNewBoard(GameType.Starter);

        var desertTile = BoardCreationHelpers.GetTileAt(gameState.Tiles, 0, 0);
        Assert.NotNull(desertTile);
        Assert.Equal(ResourceType.Desert, desertTile.Resource);

        var brickTile = BoardCreationHelpers.GetTileAt(gameState.Tiles, -1, -1);
        Assert.NotNull(brickTile);
        Assert.Equal(ResourceType.Brick, brickTile.Resource);

        var woolTile = BoardCreationHelpers.GetTileAt(gameState.Tiles, 1, -1);
        Assert.NotNull(woolTile);
        Assert.Equal(ResourceType.Wool, woolTile.Resource);

        var bluePlayer = gameState.Players.First(p => p.Color == PlayerColor.Blue);
        Assert.NotNull(bluePlayer);
        var redPlayer = gameState.Players.First(p => p.Color == PlayerColor.Red);
        Assert.NotNull(redPlayer);

        gameState.Vertices[0].BuildSettlement(bluePlayer);

        gameState.SetDiceForTesting(new GameDice(new GameDie(4), new GameDie(3)));

        // Act
        var resources = GamePlayHelpers.GetResourcesEarnedOnLastRoll(gameState);

        // Assert
        Assert.Empty(resources);
    }

    [Fact]
    public void GetVictoryPointsForBuild_ForSettlement()
    {
        // Arrange
        // Act
        // Assert
        Assert.Equal(1, GamePlayHelpers.GetVictoryPointsForBuild(BuildingType.Settlement));
    }

    [Fact]
    public void GetVictoryPointsForBuild_ForCity()
    {
        // Arrange
        // Act
        // Assert
        Assert.Equal(2, GamePlayHelpers.GetVictoryPointsForBuild(BuildingType.City));
    }

    [Fact]
    public void AssignResourcesToPlayers()
    {
        // Arrange
        GameState gameState = new GameState(new Guid());

        var player1 = new Player("Fred", PlayerColor.Blue);
        var player2 = new Player("Marge", PlayerColor.Orange);
        gameState.AddPlayer(player1);
        gameState.AddPlayer(player2);

        Dictionary<Player, Dictionary<ResourceType, int>> resources = new Dictionary<Player, Dictionary<ResourceType, int>>();
        resources.Add(player1, new Dictionary<ResourceType, int>());
        resources[player1].Add(ResourceType.Ore, 4);
        resources.Add(player2, new Dictionary<ResourceType, int>());
        resources[player2].Add(ResourceType.Wood, 1);
        resources[player2].Add(ResourceType.Brick, 2);

        // Act
        GamePlayHelpers.AssignResourcesToPlayers(gameState, resources);

        // Assert
        Assert.Equal(4, gameState.Players[0].Resources[ResourceType.Ore]);
        Assert.Equal(0, gameState.Players[0].Resources[ResourceType.Brick]);
        Assert.Equal(0, gameState.Players[0].Resources[ResourceType.Wood]);
        Assert.Equal(4, gameState.Players[0].ResourceCount);
        Assert.Equal(0, gameState.Players[1].Resources[ResourceType.Ore]);
        Assert.Equal(2, gameState.Players[1].Resources[ResourceType.Brick]);
        Assert.Equal(1, gameState.Players[1].Resources[ResourceType.Wood]);
        Assert.Equal(3, gameState.Players[1].ResourceCount);
    }

    private Player CreatePlayerWithSufficientResources()
    {
        var player = new Player("Mary", PlayerColor.Red);

        player.Resources[ResourceType.Wood] = 1;
        player.Resources[ResourceType.Brick] = 1;
        player.Resources[ResourceType.Grain] = 2;
        player.Resources[ResourceType.Wool] = 1;
        player.Resources[ResourceType.Ore] = 3;

        return player;
    }

    private Player CreatePlayerWithInsufficientResources()
    {
        var player = new Player("Mary", PlayerColor.Red);

        player.Resources[ResourceType.Wood] = 1;
        player.Resources[ResourceType.Brick] = 0;
        player.Resources[ResourceType.Grain] = 2;
        player.Resources[ResourceType.Wool] = 0;
        player.Resources[ResourceType.Ore] = 2;

        return player;
    }

    [Fact]
    public void HasResourcesToBuildRoad_SufficientResources()
    {
        // Arrange
        var player = CreatePlayerWithSufficientResources();

        // Act
        var result = GamePlayHelpers.HasResourcesToBuildRoad(player);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasResourcesToBuildRoad_InsufficientResources()
    {
        // Arrange
        var player = CreatePlayerWithInsufficientResources();

        // Act
        var result = GamePlayHelpers.HasResourcesToBuildRoad(player);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void WithdrawResourcesToBuildRoad_SufficientResources()
    {
        // Arrange
        var player = CreatePlayerWithSufficientResources();
        var woodCount = player.Resources[ResourceType.Wood];
        var brickCount = player.Resources[ResourceType.Brick];

        // Act
        GamePlayHelpers.WithdrawResourcesToBuildRoad(player);

        // Assert
        Assert.Equal(woodCount - 1, player.Resources[ResourceType.Wood]);
        Assert.Equal(brickCount - 1, player.Resources[ResourceType.Brick]);
    }

    [Fact]
    public void WithdrawResourcesToBuildRoad_InsufficientResources()
    {
        // Arrange
        var player = CreatePlayerWithInsufficientResources();
        var woodCount = player.Resources[ResourceType.Wood];
        var brickCount = player.Resources[ResourceType.Brick];

        // Act
        var exception = Assert.Throws<InvalidOperationException>(() =>
           GamePlayHelpers.WithdrawResourcesToBuildRoad(player));

        // Assert
        Assert.Equal("Player does not have required resources to build road.", exception.Message);
        Assert.Equal(woodCount, player.Resources[ResourceType.Wood]);
        Assert.Equal(brickCount, player.Resources[ResourceType.Brick]);
    }

    [Fact]
    void HasResourcesToBuildSettlment_SufficientResources()
    {
        // Arrange
        var player = CreatePlayerWithSufficientResources();

        // Act
        var result = GamePlayHelpers.HasResourcesToBuildSettlement(player);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HsResourcesToBuildSettlement_InufficientResources()
    {
        // Arrange
        var player = CreatePlayerWithInsufficientResources();

        // Act
        var result = GamePlayHelpers.HasResourcesToBuildSettlement(player);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void WithdrawResourcesToBuildSettlement_SufficientResources()
    {
        // Arrange
        var player = CreatePlayerWithSufficientResources();
        var woodCount = player.Resources[ResourceType.Wood];
        var brickCount = player.Resources[ResourceType.Brick];
        var woolCount = player.Resources[ResourceType.Wool];
        var grainCount = player.Resources[ResourceType.Grain];

        // Act
        GamePlayHelpers.WithdrawResourcesToBuildSettlement(player);

        // Assert
        Assert.Equal(woodCount - 1, player.Resources[ResourceType.Wood]);
        Assert.Equal(brickCount - 1, player.Resources[ResourceType.Brick]);
        Assert.Equal(woolCount - 1, player.Resources[ResourceType.Wool]);
        Assert.Equal(grainCount - 1, player.Resources[ResourceType.Grain]);
    }

    [Fact]
    public void WithdrawResourcesToBuildSettlement_InufficientResources()
    {
        // Arrange
        var player = CreatePlayerWithInsufficientResources();
        var woodCount = player.Resources[ResourceType.Wood];
        var brickCount = player.Resources[ResourceType.Brick];
        var woolCount = player.Resources[ResourceType.Wool];
        var grainCount = player.Resources[ResourceType.Grain];

        // Act
        var exception = Assert.Throws<InvalidOperationException>(() =>
           GamePlayHelpers.WithdrawResourcesToBuildSettlement(player));

        // Assert
        Assert.Equal("Player does not have required resources to build settlement.", exception.Message);
        Assert.Equal(woodCount, player.Resources[ResourceType.Wood]);
        Assert.Equal(brickCount, player.Resources[ResourceType.Brick]);
        Assert.Equal(woolCount, player.Resources[ResourceType.Wool]);
        Assert.Equal(grainCount, player.Resources[ResourceType.Grain]);
    }

    [Fact]
    public void HasResourcesToBuildCity_SufficientResources()
    {
        // Arrange
        var player = CreatePlayerWithSufficientResources();

        // Act
        var result = GamePlayHelpers.HasResourcesToBuildCity(player);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasResourcesToBuildCity_InsufficientResources()
    {
        // Arrange
        var player = CreatePlayerWithInsufficientResources();

        // Act
        var result = GamePlayHelpers.HasResourcesToBuildCity(player);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void WithdrawResourcesToBuildCity_SufficientResources()
    {
        // Arrange
        var player = CreatePlayerWithSufficientResources();
        var oreCount = player.Resources[ResourceType.Ore];
        var grainCount = player.Resources[ResourceType.Grain];

        // Act
        GamePlayHelpers.WithdrawResourcesToBuildCity(player);

        // Assert
        Assert.Equal(oreCount - 3, player.Resources[ResourceType.Ore]);
        Assert.Equal(grainCount - 2, player.Resources[ResourceType.Grain]);
    }

    [Fact]
    public void WithdrawResourcesToBuildCity_InsufficientResources()
    {
        // Arrange
        var player = CreatePlayerWithInsufficientResources();
        var oreCount = player.Resources[ResourceType.Ore];
        var grainCount = player.Resources[ResourceType.Grain];

        // Act
        var exception = Assert.Throws<InvalidOperationException>(() =>
           GamePlayHelpers.WithdrawResourcesToBuildCity(player));

        // Assert
        Assert.Equal("Player does not have required resources to build city.", exception.Message);
        Assert.Equal(oreCount, player.Resources[ResourceType.Ore]);
        Assert.Equal(grainCount, player.Resources[ResourceType.Grain]);
    }

    [Fact]
    public void WithdrawResourcesToBuyDevCard_SufficientResources()
    {
        // Arrange
        var player = CreatePlayerWithSufficientResources();
        var oreCount = player.Resources[ResourceType.Ore];
        var woolCount = player.Resources[ResourceType.Wool];
        var grainCount = player.Resources[ResourceType.Grain];

        // Act
        var result = GamePlayHelpers.WithdrawResourcesToBuyDevCard(player);

        // Assert
        Assert.True(result);
        Assert.Equal(oreCount - 1, player.Resources[ResourceType.Ore]);
        Assert.Equal(woolCount - 1, player.Resources[ResourceType.Wool]);
        Assert.Equal(grainCount - 1, player.Resources[ResourceType.Grain]);
    }

    [Fact]
    public void WithdrawResourcesToBuyDevCard_InsufficientResources()
    {
        // Arrange
        var player = CreatePlayerWithInsufficientResources();
        var oreCount = player.Resources[ResourceType.Ore];
        var woolCount = player.Resources[ResourceType.Wool];
        var grainCount = player.Resources[ResourceType.Grain];

        // Act
        var result = GamePlayHelpers.WithdrawResourcesToBuyDevCard(player);

        // Assert
        Assert.False(result);
        Assert.Equal(oreCount, player.Resources[ResourceType.Ore]);
        Assert.Equal(woolCount, player.Resources[ResourceType.Wool]);
        Assert.Equal(grainCount, player.Resources[ResourceType.Grain]);
    }

    private static List<Player> CreateListOfPlayersForGetNextPlayerTests()
    {
        List<Player> players = new List<Player>();
        players.Add(new Player("Michael", PlayerColor.Red));
        players.Add(new Player("Jason", PlayerColor.Blue));
        players.Add(new Player("Jeff", PlayerColor.White));

        return players;
    }

    [Fact]
    public void GetNextPlayer_FirstOfThree()
    {
        var players = CreateListOfPlayersForGetNextPlayerTests();
        var nextPlayer = GamePlayHelpers.GetNextPlayer(players[0], players);

        Assert.Equal(players[1], nextPlayer);
    }

    [Fact]
    public void GetNextPlayer_SecondOfThree()
    {
        var players = CreateListOfPlayersForGetNextPlayerTests();
        var nextPlayer = GamePlayHelpers.GetNextPlayer(players[1], players);

        Assert.Equal(players[2], nextPlayer);
    }

    [Fact]
    public void GetNextPlayer_ThreeOfThree()
    {
        var players = CreateListOfPlayersForGetNextPlayerTests();
        var nextPlayer = GamePlayHelpers.GetNextPlayer(players[2], players);

        Assert.Equal(players[0], nextPlayer);
    }

    [Fact]
    public void GetPreviousPlayer_FirstOfThree()
    {
        var players = CreateListOfPlayersForGetNextPlayerTests();
        var nextPlayer = GamePlayHelpers.GetPreviousPlayer(players[0], players);

        Assert.Equal(players[2], nextPlayer);
    }

    [Fact]
    public void GetPreviousPlayer_SecondOfThree()
    {
        var players = CreateListOfPlayersForGetNextPlayerTests();
        var nextPlayer = GamePlayHelpers.GetPreviousPlayer(players[1], players);

        Assert.Equal(players[0], nextPlayer);
    }

    [Fact]
    public void GetPreviousPlayer_ThreeOfThree()
    {
        var players = CreateListOfPlayersForGetNextPlayerTests();
        var nextPlayer = GamePlayHelpers.GetPreviousPlayer(players[2], players);

        Assert.Equal(players[1], nextPlayer);
    }

    private static GameState CreateTestGameWithManyRoads()
    {
        GameState gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter);
        var player3 = new Player("Alex", PlayerColor.Orange);
        gs.AddPlayer(player3);
        gs.Edges[0].BuildRoad(gs.Players[0]);
        gs.Vertices[0].BuildSettlement(gs.Players[0]);
        gs.Vertices[1].BuildSettlement(gs.Players[1]);
        gs.Vertices[2].BuildSettlement(gs.Players[1]);
        gs.Vertices[3].BuildSettlement(gs.Players[1]);
        gs.Vertices[4].BuildSettlement(gs.Players[1]);
        gs.Vertices[4].UpgradeToCity();
        gs.Edges[1].BuildRoad(player3);
        gs.Edges[2].BuildRoad(player3);
        gs.Vertices[5].BuildSettlement(player3);
        gs.Vertices[5].UpgradeToCity();
        gs.Vertices[6].BuildSettlement(player3);
        gs.Vertices[6].UpgradeToCity();

        return gs;
    }

    [Fact]
    public void CountSettlementsForPlayer_CountRedPlayer_1()
    {
        // Arrange
        var gs = CreateTestGameWithManyRoads();

        // Act
        var count = GamePlayHelpers.CountSettlementsForPlayer(gs, gs.Players[0]);

        // Assert
        Assert.Equal(PlayerColor.Red, gs.Players[0].Color);
        Assert.Equal(1, count);
    }

    [Fact]
    public void CountSettlementsForPlayer_CountBluePlayer_3()
    {
        // Arrange
        var gs = CreateTestGameWithManyRoads();

        // Act
        var count = GamePlayHelpers.CountSettlementsForPlayer(gs, gs.Players[1]);

        // Assert
        Assert.Equal(PlayerColor.Blue, gs.Players[1].Color);
        Assert.Equal(3, count);
    }

    [Fact]
    public void CountSettlementsForPlayer_CountOrangePlayer_0()
    {
        // Arrange
        var gs = CreateTestGameWithManyRoads();

        // Act
        var count = GamePlayHelpers.CountSettlementsForPlayer(gs, gs.Players[2]);

        // Assert
        Assert.Equal(PlayerColor.Orange, gs.Players[2].Color);
        Assert.Equal(0, count);
    }

    [Fact]
    public void CountRoadsForPlayer_CountRedPlayer_1()
    {
        // Arrange
        var gs = CreateTestGameWithManyRoads();

        // Act
        var count = GamePlayHelpers.CountRoadsForPlayer(gs, gs.Players[0]);

        // Assert
        Assert.Equal(PlayerColor.Red, gs.Players[0].Color);
        Assert.Equal(1, count);
    }

    [Fact]
    public void CountRoadsForPlayer_CountBluePlayer_3()
    {
        // Arrange
        var gs = CreateTestGameWithManyRoads();

        // Act
        var count = GamePlayHelpers.CountRoadsForPlayer(gs, gs.Players[1]);

        // Assert
        Assert.Equal(PlayerColor.Blue, gs.Players[1].Color);
        Assert.Equal(0, count);
    }

    [Fact]
    public void CountRoadsForPlayer_CountOrangePlayer_0()
    {
        // Arrange
        var gs = CreateTestGameWithManyRoads();

        // Act
        var count = GamePlayHelpers.CountRoadsForPlayer(gs, gs.Players[2]);

        // Assert
        Assert.Equal(PlayerColor.Orange, gs.Players[2].Color);
        Assert.Equal(2, count);
    }

    private GameState CreateGameStateForPhaseTesting()
    {
        var gs = new GameState(new Guid());
        gs.Tiles.AddRange(BoardCreationHelpers.CreateTilesForTestBoard());
        gs.SetRobberTile(BoardCreationHelpers.GetTileAt(gs.Tiles, -1, -1));
        BoardCreationHelpers.CreateEdgesAndVerticesForBoard(gs);
        BoardCreationHelpers.LinkEdgesAndVertices(gs);
        gs.AddPlayer(new Player("George", PlayerColor.White));
        gs.AddPlayer(new Player("Elaine", PlayerColor.Green));

        return gs;
    }


    private void BuildTestCities(GameState gs, Player player, int count)
    {
        int vIndex = 0;

        for (int i = 0; i < count; i++)
        {
            while (gs.Vertices[vIndex].Building != null)
                vIndex++;

            gs.Vertices[vIndex].BuildSettlement(player);
            gs.Vertices[vIndex].UpgradeToCity();
        }
    }

    private void BuildTestSettlements(GameState gs, Player player, int count)
    {
        int vIndex = 0;

        for (int i = 0; i < count; i++)
        {
            while (gs.Vertices[vIndex].Building != null)
                vIndex++;

            gs.Vertices[vIndex].BuildSettlement(player);
        }
    }

    [Fact]
    public void PlayerHasWon_DefaultThreshold_Not_Met()
    {
        var gs = CreateGameStateForPhaseTesting();
        gs.Phase = new GamePhase(GameStates.RollOrUseDevCard, gs.Players[0], gs.Players[1]);
        Assert.NotNull(gs.Phase.CurrentPlayer);
        BuildTestCities(gs, gs.Phase.CurrentPlayer, 4);
        BuildTestSettlements(gs, gs.Phase.CurrentPlayer, 1);

        Assert.False(GamePlayHelpers.PlayerHasWon(gs, gs.Phase.CurrentPlayer));
    }

    [Fact]
    public void PlayerHasWon_DefaultThreshold_Met()
    {
        var gs = CreateGameStateForPhaseTesting();
        gs.Phase = new GamePhase(GameStates.RollOrUseDevCard, gs.Players[0], gs.Players[1]);
        Assert.NotNull(gs.Phase.CurrentPlayer);
        BuildTestCities(gs, gs.Phase.CurrentPlayer, 4);
        BuildTestSettlements(gs, gs.Phase.CurrentPlayer, 2);

        Assert.True(GamePlayHelpers.PlayerHasWon(gs, gs.Phase.CurrentPlayer));
    }

    // TODO: Add tests where victory points come from dev cards, longest road, and largest army


    [Fact]
    public void GetNextPhase_SettingUpBoard_MoveToPlaceFirstSettlement()
    {
        var gs = CreateGameStateForPhaseTesting();
        gs.Phase = new GamePhase(GameStates.SettingUpBoard);

        var phase = GamePlayHelpers.GetNextPhase(gs);

        Assert.Equal(GameStates.PlaceFirstSettlement, phase.PhaseState);

        Assert.NotNull(phase.CurrentPlayer);
        Assert.NotNull(phase.EndPlayer);

        if (phase.CurrentPlayer.Id == gs.Players[0].Id)
        {
            Assert.Equal(gs.Players[0].Id, phase.CurrentPlayer.Id);
            Assert.Equal(gs.Players[1].Id, phase.EndPlayer.Id);
        }
        else
        {
            Assert.Equal(gs.Players[1].Id, phase.CurrentPlayer.Id);
            Assert.Equal(gs.Players[0].Id, phase.EndPlayer.Id);
        }
    }

    [Fact]
    public void GetNextPhase_PlaceFirstSettlement_NoSettlementStayPut()
    {
        var gs = CreateGameStateForPhaseTesting();
        gs.Phase = new GamePhase(GameStates.PlaceFirstSettlement, gs.Players[0], gs.Players[1]);

        var phase = GamePlayHelpers.GetNextPhase(gs);

        Assert.Equal(GameStates.PlaceFirstSettlement, phase.PhaseState);
        Assert.Equal(gs.Players[0], phase.CurrentPlayer);
        Assert.Equal(gs.Players[1], phase.EndPlayer);
    }

    [Fact]
    public void GetNextPhase_PlaceFirstSettlement_MoveToPlaceFirstRoad()
    {
        var gs = CreateGameStateForPhaseTesting();
        gs.Phase = new GamePhase(GameStates.PlaceFirstSettlement, gs.Players[0], gs.Players[1]);
        Assert.NotNull(gs.Phase.CurrentPlayer);
        gs.Vertices[0].BuildSettlement(gs.Phase.CurrentPlayer);

        var phase = GamePlayHelpers.GetNextPhase(gs);

        Assert.Equal(GameStates.PlaceFirstRoad, phase.PhaseState);
        Assert.Equal(gs.Players[0], phase.CurrentPlayer);
        Assert.Equal(gs.Players[1], phase.EndPlayer);
    }

    [Fact]
    public void GetNextPhase_PlaceFirstRoad_NoRoadStayPut()
    {
        var gs = CreateGameStateForPhaseTesting();
        gs.Phase = new GamePhase(GameStates.PlaceFirstRoad, gs.Players[0], gs.Players[1]);
        Assert.NotNull(gs.Phase.CurrentPlayer);
        gs.Vertices[0].BuildSettlement(gs.Phase.CurrentPlayer);

        var phase = GamePlayHelpers.GetNextPhase(gs);

        Assert.Equal(GameStates.PlaceFirstRoad, phase.PhaseState);
        Assert.Equal(gs.Players[0], phase.CurrentPlayer);
        Assert.Equal(gs.Players[1], phase.EndPlayer);
    }

    [Fact]
    public void GetNextPhase_PlaceFirstRoad_MoveToNextPlayer()
    {
        var gs = CreateGameStateForPhaseTesting();
        gs.Phase = new GamePhase(GameStates.PlaceFirstRoad, gs.Players[0], gs.Players[1]);
        Assert.NotNull(gs.Phase.CurrentPlayer);
        gs.Vertices[0].BuildSettlement(gs.Phase.CurrentPlayer);
        gs.Edges[0].BuildRoad(gs.Players[0]);

        var phase = GamePlayHelpers.GetNextPhase(gs);

        Assert.Equal(GameStates.PlaceFirstSettlement, phase.PhaseState);
        Assert.Equal(gs.Players[1], phase.CurrentPlayer);
        Assert.Equal(gs.Players[1], phase.EndPlayer);
    }

    [Fact]
    public void GetNextPhase_PlaceFirstRoad_MoveToPlaceSecondSettlement()
    {
        var gs = CreateGameStateForPhaseTesting();
        gs.Phase = new GamePhase(GameStates.PlaceFirstRoad, gs.Players[1], gs.Players[1]);
        Assert.NotNull(gs.Phase.CurrentPlayer);
        gs.Vertices[0].BuildSettlement(gs.Phase.CurrentPlayer);
        gs.Edges[0].BuildRoad(gs.Phase.CurrentPlayer);

        var phase = GamePlayHelpers.GetNextPhase(gs);

        Assert.Equal(GameStates.PlaceSecondSettlement, phase.PhaseState);
        Assert.Equal(gs.Players[1], phase.CurrentPlayer);
        Assert.Equal(gs.Players[0], phase.EndPlayer);
    }

    [Fact]
    public void GetNextPhase_PlaceSecondSettlement_No2ndStayPut()
    {
        var gs = CreateGameStateForPhaseTesting();
        gs.Phase = new GamePhase(GameStates.PlaceSecondSettlement, gs.Players[1], gs.Players[0]);
        Assert.NotNull(gs.Phase.CurrentPlayer);
        gs.Vertices[0].BuildSettlement(gs.Phase.CurrentPlayer);
        gs.Edges[0].BuildRoad(gs.Phase.CurrentPlayer);

        var phase = GamePlayHelpers.GetNextPhase(gs);

        Assert.Equal(GameStates.PlaceSecondSettlement, phase.PhaseState);
        Assert.Equal(gs.Players[1], phase.CurrentPlayer);
        Assert.Equal(gs.Players[0], phase.EndPlayer);
    }

    [Fact]
    public void GetNextPhase_PlaceSecondSettlement_MoveToPlaceSecondRoad()
    {
        var gs = CreateGameStateForPhaseTesting();
        gs.Phase = new GamePhase(GameStates.PlaceSecondSettlement, gs.Players[1], gs.Players[0]);
        Assert.NotNull(gs.Phase.CurrentPlayer);
        gs.Vertices[0].BuildSettlement(gs.Phase.CurrentPlayer);
        gs.Vertices[1].BuildSettlement(gs.Phase.CurrentPlayer);
        gs.Edges[0].BuildRoad(gs.Phase.CurrentPlayer);

        var phase = GamePlayHelpers.GetNextPhase(gs);

        Assert.Equal(GameStates.PlaceSecondRoad, phase.PhaseState);
        Assert.Equal(gs.Players[1], phase.CurrentPlayer);
        Assert.Equal(gs.Players[0], phase.EndPlayer);
    }

    [Fact]
    public void GetNextPhase_PlaceSecondRoad_No2ndStayPut()
    {
        var gs = CreateGameStateForPhaseTesting();
        gs.Phase = new GamePhase(GameStates.PlaceSecondRoad, gs.Players[1], gs.Players[0]);
        Assert.NotNull(gs.Phase.CurrentPlayer);
        gs.Vertices[0].BuildSettlement(gs.Phase.CurrentPlayer);
        gs.Vertices[1].BuildSettlement(gs.Phase.CurrentPlayer);
        gs.Edges[0].BuildRoad(gs.Phase.CurrentPlayer);

        var phase = GamePlayHelpers.GetNextPhase(gs);

        Assert.Equal(GameStates.PlaceSecondRoad, phase.PhaseState);
        Assert.Equal(gs.Players[1], phase.CurrentPlayer);
        Assert.Equal(gs.Players[0], phase.EndPlayer);
    }

    [Fact]
    public void GetNextPhase_PlaceSecondRoad_MoveToNextPlayer()
    {
        var gs = CreateGameStateForPhaseTesting();
        gs.Phase = new GamePhase(GameStates.PlaceSecondRoad, gs.Players[1], gs.Players[0]);
        Assert.NotNull(gs.Phase.CurrentPlayer);
        gs.Vertices[0].BuildSettlement(gs.Phase.CurrentPlayer);
        gs.Vertices[1].BuildSettlement(gs.Phase.CurrentPlayer);
        gs.Edges[0].BuildRoad(gs.Phase.CurrentPlayer);
        gs.Edges[1].BuildRoad(gs.Phase.CurrentPlayer);

        var phase = GamePlayHelpers.GetNextPhase(gs);

        Assert.Equal(GameStates.PlaceSecondSettlement, phase.PhaseState);
        Assert.Equal(gs.Players[0], phase.CurrentPlayer);
        Assert.Equal(gs.Players[0], phase.EndPlayer);
    }

    [Fact]
    public void GetNextPhase_PlaceSecondRoad_MoveToRollOrUseDevCard()
    {
        var gs = CreateGameStateForPhaseTesting();
        gs.Phase = new GamePhase(GameStates.PlaceSecondRoad, gs.Players[0], gs.Players[0]);
        Assert.NotNull(gs.Phase.CurrentPlayer);
        gs.Vertices[0].BuildSettlement(gs.Phase.CurrentPlayer);
        gs.Vertices[1].BuildSettlement(gs.Phase.CurrentPlayer);
        gs.Edges[0].BuildRoad(gs.Phase.CurrentPlayer);
        gs.Edges[1].BuildRoad(gs.Phase.CurrentPlayer);

        var phase = GamePlayHelpers.GetNextPhase(gs);

        Assert.Equal(GameStates.RollOrUseDevCard, phase.PhaseState);
        Assert.Equal(gs.Players[0], phase.CurrentPlayer);
        Assert.Equal(gs.Players[1], phase.EndPlayer);
    }

    [Fact]
    public void GetNextPhase_RollOrUseDevCard_NoRollStayPut()
    {
        var gs = CreateGameStateForPhaseTesting();
        gs.Phase = new GamePhase(GameStates.RollOrUseDevCard, gs.Players[0], gs.Players[1]);
        gs.Dice.SetWaiting();
        Assert.True(gs.Dice.WaitingForRoll);

        var phase = GamePlayHelpers.GetNextPhase(gs);

        Assert.Equal(GameStates.RollOrUseDevCard, phase.PhaseState);
        Assert.Equal(gs.Players[0], phase.CurrentPlayer);
        Assert.Equal(gs.Players[1], phase.EndPlayer);
    }

    [Fact]
    public void GetNextPhase_RollOrUseDevCard_MoveToBuildOrTrade()
    {
        var gs = CreateGameStateForPhaseTesting();
        gs.Phase = new GamePhase(GameStates.RollOrUseDevCard, gs.Players[0], gs.Players[1]);
        gs.Dice.SetWaiting();
        Assert.True(gs.Dice.WaitingForRoll);

        do
            gs.Dice.Roll();
        while (gs.Dice.Die1.Value + gs.Dice.Die2.Value == 7);

        var phase = GamePlayHelpers.GetNextPhase(gs);

        Assert.Equal(GameStates.BuildOrTrade, phase.PhaseState);
        Assert.Equal(gs.Players[0], phase.CurrentPlayer);
        Assert.Equal(gs.Players[1], phase.EndPlayer);
    }

    [Fact]
    public void GetNextPhase_RollOrUseDevCard_MoveToPlaceRobber()
    {
        var gs = CreateGameStateForPhaseTesting();
        gs.Phase = new GamePhase(GameStates.RollOrUseDevCard, gs.Players[0], gs.Players[1]);
        gs.Dice.SetWaiting();
        Assert.True(gs.Dice.WaitingForRoll);

        do
            gs.Dice.Roll();
        while (gs.Dice.Die1.Value + gs.Dice.Die2.Value != 7);

        var phase = GamePlayHelpers.GetNextPhase(gs);

        Assert.Equal(GameStates.PlaceRobber, phase.PhaseState);
        Assert.Equal(gs.Players[0], phase.CurrentPlayer);
        Assert.Equal(gs.Players[1], phase.EndPlayer);
        Assert.Equal(GameStates.BuildOrTrade, gs.Phase.PreviousState);
    }

    [Fact]
    public void GetNextPhase_PlaceRobber_RobberNotMovedStayPut()
    {
        var gs = CreateGameStateForPhaseTesting();
        gs.Phase = new GamePhase(GameStates.PlaceRobber, gs.Players[0], gs.Players[1]);

        gs.Phase.SetStateToReturnTo(GameStates.RollOrUseDevCard, gs.RobberTile);

        var phase = GamePlayHelpers.GetNextPhase(gs);

        Assert.Equal(GameStates.PlaceRobber, phase.PhaseState);
        Assert.Equal(GameStates.RollOrUseDevCard, gs.Phase.PreviousState);
        Assert.Equal(gs.Players[0], phase.CurrentPlayer);
        Assert.Equal(gs.Players[1], phase.EndPlayer);
    }

    [Fact]
    public void GetNextPhase_PlaceRobber_MoveToRollOrUseDevCard()
    {
        var gs = CreateGameStateForPhaseTesting();
        gs.Phase = new GamePhase(GameStates.PlaceRobber, gs.Players[0], gs.Players[1]);
        gs.Phase.SetStateToReturnTo(GameStates.RollOrUseDevCard, gs.RobberTile);

        var newRobberTile = BoardCreationHelpers.GetTileAt(gs.Tiles, -2, 0);
        gs.SetRobberTile(newRobberTile);

        var phase = GamePlayHelpers.GetNextPhase(gs);
        gs.Phase.ClearRobberState();

        Assert.Equal(GameStates.RollOrUseDevCard, phase.PhaseState);
        Assert.Equal(gs.Players[0], phase.CurrentPlayer);
        Assert.Equal(gs.Players[1], phase.EndPlayer);
        Assert.Null(gs.Phase.PreviousState);
        Assert.Null(gs.Phase.OriginalRobberTile);
        Assert.Equal(newRobberTile.Id, gs.RobberTile.Id);
    }

    // TODO: Can't implement this phase transition until support for playing Knights
    // [Fact]
    // public void GetNextPhase_BuildOrTrade_MoveToPlaceRobber()
    // {
    //     var gs = CreateGameStateForPhaseTesting();
    //     gs.Phase = new GamePhase(GameStates.BuildOrTrade, gs.Players[0], gs.Players[1]);

    //     // TODO: Add code to set up state to match what will happen if Players[0]
    //     // plays a Knight Dev Card.

    //     var phase = GamePlayHelpers.GetNextPhase(gs);

    //     Assert.Equal(GameStates.PlaceRobber, phase.PhaseState);
    //     Assert.Equal(gs.Players[0], phase.CurrentPlayer);
    //     Assert.Equal(gs.Players[1], phase.EndPlayer);
    //     Assert.Equal(GameStates.BuildOrTrade, gs.Settings.PreRobberState);
    // }

    [Fact]
    public void GetNextPhase_PlaceRobber_MoveToBuildOrTrade()
    {
        var gs = CreateGameStateForPhaseTesting();
        gs.Phase = new GamePhase(GameStates.PlaceRobber, gs.Players[0], gs.Players[1]);
        gs.Phase.SetStateToReturnTo(GameStates.BuildOrTrade, gs.RobberTile);
        var newRobberTile = BoardCreationHelpers.GetTileAt(gs.Tiles, -2, 0);
        gs.SetRobberTile(newRobberTile);

        var phase = GamePlayHelpers.GetNextPhase(gs);

        gs.Phase.ClearRobberState();

        Assert.Equal(GameStates.BuildOrTrade, phase.PhaseState);
        Assert.Equal(gs.Players[0], phase.CurrentPlayer);
        Assert.Equal(gs.Players[1], phase.EndPlayer);
        Assert.Null(gs.Phase.PreviousState);
        Assert.Null(gs.Phase.OriginalRobberTile);
        Assert.Equal(newRobberTile.Id, gs.RobberTile.Id);
    }

    [Fact]
    public void GetNextPhase_BuildOrTrade_MoveToGameOverWin()
    {
        var gs = CreateGameStateForPhaseTesting();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, gs.Players[0], gs.Players[1]);
        Assert.NotNull(gs.Phase.CurrentPlayer);
        BuildTestCities(gs, gs.Phase.CurrentPlayer, 4);
        BuildTestSettlements(gs, gs.Phase.CurrentPlayer, 2);

        var phase = GamePlayHelpers.GetNextPhase(gs);

        Assert.Equal(GameStates.GameOver, phase.PhaseState);
        Assert.Equal(gs.Players[0], phase.CurrentPlayer);
        Assert.Equal(gs.Players[1], phase.EndPlayer);
    }

    // TODO: Seems like moving from any state to GameOver due to the resignation
    // of all other players would be from explicit calls from the users vs
    // something that would happen due to a call to GetNextPhase.

    [Fact]
    public void EndTurn_MovesToNextPlayer()
    {
        var gs = CreateGameStateForPhaseTesting();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, gs.Players[0], gs.Players[1]);

        Assert.NotNull(gs.Phase.CurrentPlayer);
        GamePlayHelpers.EndTurn(gs.Phase.CurrentPlayer, gs);

        Assert.Equal(GameStates.RollOrUseDevCard, gs.Phase.PhaseState);
        Assert.Equal(gs.Players[1], gs.Phase.CurrentPlayer);
        Assert.Equal(gs.Players[1], gs.Phase.EndPlayer);
    }

    [Fact]
    public void EndTurn_ExceptionIfNoCurrentPlayer()
    {
        var gs = CreateGameStateForPhaseTesting();
        Assert.Null(gs.Phase.CurrentPlayer);

        var exception = Assert.Throws<InvalidOperationException>(() =>
           GamePlayHelpers.EndTurn(gs.Players[0], gs));

        Assert.Equal("Can not end turn without a CurrentPlayer.", exception.Message);
    }

    [Fact]
    public void EndTurn_ExceptionIfNotPlayersTurn()
    {
        var gs = CreateGameStateForPhaseTesting();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, gs.Players[0], gs.Players[1]);

        var exception = Assert.Throws<InvalidOperationException>(() =>
           GamePlayHelpers.EndTurn(gs.Players[1], gs));

        Assert.Equal("Can not end the turn for another player.", exception.Message);
    }

    [Fact]
    public void EndTurn_ExceptionIfNotBuildOrTradePhase()
    {
        var gs = CreateGameStateForPhaseTesting();
        gs.Phase = new GamePhase(GameStates.RollOrUseDevCard, gs.Players[0], gs.Players[1]);

        var exception = Assert.Throws<InvalidOperationException>(() =>
           GamePlayHelpers.EndTurn(gs.Players[0], gs));

        Assert.Equal("Can not end turn on any phase but BuildOrTrade.", exception.Message);
    }

    [Fact]
    public void BuildRoad_MissingPlayer()
    {
        var gs = CreateGameStateForPhaseTesting();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, gs.Players[1], gs.Players[1]);

        var response = GamePlayHelpers.BuildRoadRequestFromUser(gs, "PP1", gs.Edges[0].Id);

        Assert.False(response.Success);
        Assert.Equal(1012, response.ErrorCode);
    }

    [Fact]
    public void BuildRoad_NotPlayersTurn()
    {
        var gs = CreateGameStateForPhaseTesting();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, gs.Players[1], gs.Players[1]);

        var response = GamePlayHelpers.BuildRoadRequestFromUser(gs, gs.Players[0].Id, gs.Edges[0].Id);

        Assert.False(response.Success);
        Assert.Equal(1011, response.ErrorCode);
    }

    // TODO: Need to add additional BuildSRoad tests.

    [Fact]
    public void BuildRoad_FailIfWrongPhase()
    {
        var gs = CreateGameStateForPhaseTesting();
        gs.Phase = new GamePhase(GameStates.RollOrUseDevCard, gs.Players[0], gs.Players[1]);

        var response = GamePlayHelpers.BuildRoadRequestFromUser(gs, gs.Players[0].Id, gs.Edges[0].Id);

        Assert.False(response.Success);
        Assert.Equal(1003, response.ErrorCode);
    }

    [Fact]
    public void BuildRoad_BuildOrTradePhase_BuildsRoadNoPhaseChange()
    {
        // Arrange
        var gs = CreateGameStateForPhaseTesting();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, gs.Players[0], gs.Players[1]);

        Assert.NotEmpty(gs.Edges[0].Vertices);
        var adjacentVertex = gs.Edges[0].Vertices[0];
        adjacentVertex.BuildSettlement(gs.Players[0]);

        gs.Players[0].Resources[ResourceType.Wood] = 1;
        gs.Players[0].Resources[ResourceType.Brick] = 1;

        // Act
        var response = GamePlayHelpers.BuildRoadRequestFromUser(gs, gs.Players[0].Id, gs.Edges[0].Id);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(gs.Edges[0].Owner);
        Assert.Equal(gs.Players[0].Id, gs.Edges[0].Owner.Id);
    }

    [Fact]
    public void BuildRoad_PlaceFirstRoadPhase_BuildsRoadAndPhaseChange()
    {
        // Arrange
        var gs = CreateGameStateForPhaseTesting();
        gs.Phase = new GamePhase(GameStates.PlaceFirstRoad, gs.Players[1], gs.Players[1]);

        Assert.NotEmpty(gs.Edges[0].Vertices);
        var adjacentVertex = gs.Edges[0].Vertices[1];
        adjacentVertex.BuildSettlement(gs.Players[1]);

        // Act
        var response = GamePlayHelpers.BuildRoadRequestFromUser(gs, gs.Players[1].Id, gs.Edges[0].Id);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(gs.Edges[0].Owner);
        Assert.Equal(gs.Players[1].Id, gs.Edges[0].Owner.Id);
        Assert.Equal(GameStates.PlaceSecondSettlement, gs.Phase.PhaseState);
        Assert.NotNull(gs.Phase.CurrentPlayer);
        Assert.Equal(gs.Phase.CurrentPlayer.Id, gs.Players[1].Id);
    }

    [Fact]
    public void BuildRoad_NotAdjacentToBuilding()
    {
        // Arrange
        var gs = CreateGameStateForPhaseTesting();
        gs.Phase = new GamePhase(GameStates.PlaceFirstRoad, gs.Players[1], gs.Players[1]);

        var grainTile = BoardCreationHelpers.GetTileAt(gs.Tiles, 0, 0);
        var woolTile = BoardCreationHelpers.GetTileAt(gs.Tiles, 1, -1);
        var oreTile = BoardCreationHelpers.GetTileAt(gs.Tiles, 2, 0);
        var woodTile = BoardCreationHelpers.GetTileAt(gs.Tiles, 1, 1);

        // Retrieve non-adjacent vertex and edge.
        var vertex = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, grainTile, oreTile, woodTile, null);
        var edge = BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, grainTile, woolTile, null);

        vertex.BuildSettlement(gs.Players[1]);

        // Act
        var response = GamePlayHelpers.BuildRoadRequestFromUser(gs, gs.Players[1].Id, edge.Id);

        // Assert
        Assert.False(response.Success);
        Assert.Equal(1015, response.ErrorCode);
        Assert.Equal(BuildingType.Settlement, vertex.Building);
    }

    [Fact]
    public void BuildRoad_NotAdjacentToBuildingOfRightPlayer()
    {
        // Arrange
        var gs = CreateGameStateForPhaseTesting();
        gs.Phase = new GamePhase(GameStates.PlaceFirstRoad, gs.Players[1], gs.Players[1]);

        var grainTile = BoardCreationHelpers.GetTileAt(gs.Tiles, 0, 0);
        var oreTile = BoardCreationHelpers.GetTileAt(gs.Tiles, 2, 0);
        var woodTile = BoardCreationHelpers.GetTileAt(gs.Tiles, 1, 1);

        // Retrieve non-adjacent vertex and edge.
        var vertex = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, grainTile, oreTile, woodTile, null);
        var edge = BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, grainTile, oreTile, null);

        vertex.BuildSettlement(gs.Players[0]);

        // Act
        var response = GamePlayHelpers.BuildRoadRequestFromUser(gs, gs.Players[1].Id, edge.Id);

        // Assert
        Assert.Equal(BuildingType.Settlement, vertex.Building);
        Assert.False(response.Success);
        Assert.Equal(1015, response.ErrorCode);
    }

    [Fact]
    public void BuildSettlement_MissingPlayer()
    {
        var gs = CreateGameStateForPhaseTesting();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, gs.Players[1], gs.Players[1]);

        var response = GamePlayHelpers.BuildSettlementRequestFromUser(gs, "PP1", gs.Vertices[0].Id);

        Assert.False(response.Success);
        Assert.Equal(1012, response.ErrorCode);
    }

    [Fact]
    public void BuildSettlement_NotPlayersTurn()
    {
        var gs = CreateGameStateForPhaseTesting();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, gs.Players[1], gs.Players[1]);

        var response = GamePlayHelpers.BuildSettlementRequestFromUser(gs, gs.Players[0].Id, gs.Vertices[0].Id);

        Assert.False(response.Success);
        Assert.Equal(1011, response.ErrorCode);
    }

    // TODO: Need to add additional BuildSettlement tests.

    [Fact]
    public void BuildSettlement_FailIfWrongPhase()
    {
        var gs = CreateGameStateForPhaseTesting();
        gs.Phase = new GamePhase(GameStates.RollOrUseDevCard, gs.Players[0], gs.Players[1]);

        var response = GamePlayHelpers.BuildSettlementRequestFromUser(gs, gs.Players[0].Id, gs.Vertices[0].Id);

        Assert.False(response.Success);
        Assert.Equal(1003, response.ErrorCode);
    }

    [Fact]
    public void BuildSettlement_BuildOrTradePhase_BuildsSettlementNoPhaseChange()
    {
        var gs = CreateGameStateForPhaseTesting();
        gs.Phase = new GamePhase(GameStates.BuildOrTrade, gs.Players[0], gs.Players[1]);

        var grainTile = BoardCreationHelpers.GetTileAt(gs.Tiles, 0, 0);
        var oreTile = BoardCreationHelpers.GetTileAt(gs.Tiles, 2, 0);
        var woolTile = BoardCreationHelpers.GetTileAt(gs.Tiles, 1, -1);

        var edge = BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, woolTile, oreTile, null);
        edge.BuildRoad(gs.Players[0]);

        var vertex = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, grainTile, oreTile, woolTile, null);
        gs.Players[0].Resources[ResourceType.Brick] = 1;
        gs.Players[0].Resources[ResourceType.Wood] = 1;
        gs.Players[0].Resources[ResourceType.Wool] = 1;
        gs.Players[0].Resources[ResourceType.Grain] = 1;

        // Act
        var response = GamePlayHelpers.BuildSettlementRequestFromUser(gs, gs.Players[0].Id, vertex.Id);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(vertex.Owner);
        Assert.NotNull(vertex.Building);
        Assert.Equal(gs.Players[0].Id, vertex.Owner.Id);
        Assert.Equal(BuildingType.Settlement, vertex.Building);
    }

    [Fact]
    public void BuildRoad_PlaceFirstSettlementPhase_BuildsSettlementAndPhaseChange()
    {
        var gs = CreateGameStateForPhaseTesting();
        gs.Phase = new GamePhase(GameStates.PlaceFirstSettlement, gs.Players[0], gs.Players[1]);

        var response = GamePlayHelpers.BuildSettlementRequestFromUser(gs, gs.Players[0].Id, gs.Vertices[0].Id);

        Assert.True(response.Success);
        Assert.NotNull(gs.Vertices[0].Building);
        Assert.Equal(BuildingType.Settlement, gs.Vertices[0].Building);
        Assert.NotNull(gs.Vertices[0].Owner);
        Assert.Equal(gs.Players[0].Id, gs.Vertices[0].Owner.Id);
        Assert.Equal(GameStates.PlaceFirstRoad, gs.Phase.PhaseState);
        Assert.NotNull(gs.Phase.CurrentPlayer);
        Assert.Equal(gs.Phase.CurrentPlayer.Id, gs.Players[0].Id);
    }

    [Fact]
    public void GameLoop_DropOutIfNotBotsTurn()
    {
        var gs = CreateGameStateForPhaseTesting();
        gs.Phase = new GamePhase(GameStates.PlaceFirstSettlement, gs.Players[0], gs.Players[1]);

        GamePlayHelpers.GameLoop(gs);

        Assert.NotNull(gs.Phase.CurrentPlayer);
        Assert.False(gs.Phase.CurrentPlayer.IsBot);
    }

    [Fact]
    public void LinkEdgesAndVertices_AroundCenter()
    {
        // Arrange
        var gs = new GameState(new Guid());

        var t1 = new Tile(ResourceType.Desert, 0, 0, 0);
        gs.Tiles.Add(t1);
        var t2 = new Tile(ResourceType.Wool, 4, 1, -1);
        gs.Tiles.Add(t2);
        var t3 = new Tile(ResourceType.Wood, 3, 2, 0);
        gs.Tiles.Add(t3);
        var t4 = new Tile(ResourceType.Ore, 8, 4, 0);
        gs.Tiles.Add(t4);
        var t5 = new Tile(ResourceType.Brick, 10, 3, -1);
        gs.Tiles.Add(t5);

        BoardCreationHelpers.CreateEdgesAndVerticesForBoard(gs);

        var edge12 = gs.Edges.First(e => e.Tiles.Contains(t1) && e.Tiles.Contains(t2));
        Assert.NotNull(edge12);
        var edge1NW = gs.Edges.First(e => e.Tiles.Count() == 1 && e.Tiles.Contains(t1) && e.Direction == HexDirection.NW);
        Assert.NotNull(edge1NW);
        var edge2W = gs.Edges.First(e => e.Tiles.Count() == 1 && e.Tiles.Contains(t2) && e.Direction == HexDirection.W);
        Assert.NotNull(edge2W);
        var edge13 = gs.Edges.First(e => e.Tiles.Contains(t1) && e.Tiles.Contains(t3));
        Assert.NotNull(edge13);
        var edge23 = gs.Edges.First(e => e.Tiles.Contains(t2) && e.Tiles.Contains(t3));
        Assert.NotNull(edge23);
        var edge34 = gs.Edges.First(e => e.Tiles.Contains(t3) && e.Tiles.Contains(t4));
        Assert.NotNull(edge34);
        var edge35 = gs.Edges.First(e => e.Tiles.Contains(t3) && e.Tiles.Contains(t5));
        Assert.NotNull(edge35);
        var edge45 = gs.Edges.First(e => e.Tiles.Contains(t4) && e.Tiles.Contains(t5));
        Assert.NotNull(edge45);
        var edge4NE = gs.Edges.First(e => e.Tiles.Count() == 1 && e.Tiles.Contains(t4) && e.Direction == HexDirection.NE);
        Assert.NotNull(edge4NE);
        var edge4E = gs.Edges.First(e => e.Tiles.Count() == 1 && e.Tiles.Contains(t4) && e.Direction == HexDirection.E);
        Assert.NotNull(edge4E);

        var vertex123 = gs.Vertices.First(v => v.Tiles.Contains(t1) && v.Tiles.Contains(t2) && v.Tiles.Contains(t3));
        Assert.NotNull(vertex123);
        var vertex12 = gs.Vertices.First(v => v.Tiles.Count == 2 && v.Tiles.Contains(t1) && v.Tiles.Contains(t2));
        Assert.NotNull(vertex12);
        var vertex34 = gs.Vertices.First(v => v.Tiles.Count == 2 && v.Tiles.Contains(t3) && v.Tiles.Contains(t4));
        Assert.NotNull(vertex34);
        var vertex345 = gs.Vertices.First(v => v.Tiles.Contains(t3) && v.Tiles.Contains(t4) && v.Tiles.Contains(t5));
        Assert.NotNull(vertex345);
        var vertex4SE = gs.Vertices.First(v => v.Tiles.Count == 1 && v.Tiles.Contains(t4) && v.Direction == VertexDirection.SE);
        Assert.NotNull(vertex4SE);
        var vertex4NE = gs.Vertices.First(v => v.Tiles.Count == 1 && v.Tiles.Contains(t4) && v.Direction == VertexDirection.NE);

        // Act
        BoardCreationHelpers.LinkEdgesAndVertices(gs);

        // Assert
        Assert.Equal(2, edge12.Vertices.Count());
        Assert.Contains(vertex12, edge12.Vertices);
        Assert.Contains(vertex123, edge12.Vertices);

        Assert.Equal(3, vertex12.Edges.Count());
        Assert.Contains(edge1NW, vertex12.Edges);
        Assert.Contains(edge2W, vertex12.Edges);
        Assert.Contains(edge12, vertex12.Edges);

        Assert.Equal(3, vertex123.Edges.Count());
        Assert.Contains(edge12, vertex123.Edges);
        Assert.Contains(edge23, vertex123.Edges);
        Assert.Contains(edge13, vertex123.Edges);

        Assert.Equal(3, vertex345.Edges.Count());
        Assert.Contains(edge34, vertex345.Edges);
        Assert.Contains(edge35, vertex345.Edges);
        Assert.Contains(edge45, vertex345.Edges);

        Assert.Equal(2, edge4E.Vertices.Count());
        Assert.Contains(vertex4NE, edge4E.Vertices);
        Assert.Contains(vertex4SE, edge4E.Vertices);

        Assert.Equal(2, vertex4NE.Edges.Count());
        Assert.Contains(edge4NE, vertex4NE.Edges);
        Assert.Contains(edge4E, vertex4NE.Edges);
    }

    [Fact]
    public void GetEdgeFromTileInfo_2Tile()
    {
        // Arrange
        GameState gs = new GameState(new Guid());
        var t1 = new Tile(ResourceType.Desert, 0, 0, 0);
        var t2 = new Tile(ResourceType.Wool, 4, 1, -1);
        var e1 = new Edge(t1, HexDirection.NW);
        gs.Edges.Add(e1);
        var e2 = new Edge(t1, HexDirection.E);
        gs.Edges.Add(e2);
        var e3 = new Edge(t1, t2);
        gs.Edges.Add(e3);
        var e4 = new Edge(t2, HexDirection.W);
        gs.Edges.Add(e4);

        // Act
        var edge = BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, t1, t2, null);

        // Assert
        Assert.Equal(e3.Id, edge.Id);
    }

    [Fact]
    public void GetEdgeFromTileInfo_1Tile()
    {
        // Arrange
        GameState gs = new GameState(new Guid());
        var t1 = new Tile(ResourceType.Desert, 0, 0, 0);
        var t2 = new Tile(ResourceType.Wool, 4, 1, -1);
        var e1 = new Edge(t1, HexDirection.NW);
        gs.Edges.Add(e1);
        var e2 = new Edge(t1, HexDirection.E);
        gs.Edges.Add(e2);
        var e3 = new Edge(t1, t2);
        gs.Edges.Add(e3);
        var e4 = new Edge(t2, HexDirection.W);
        gs.Edges.Add(e4);

        // Act
        var edge = BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, t1, null, HexDirection.E);

        // Assert
        Assert.Equal(e2.Id, edge.Id);
    }

    [Fact]
    public void GetEdgeFromTileInfo_IgnoreDirIfMoreThan1Tile()
    {
        // Arrange
        GameState gs = new GameState(new Guid());
        var t1 = new Tile(ResourceType.Desert, 0, 0, 0);
        var t2 = new Tile(ResourceType.Wool, 4, 1, -1);

        var e1 = new Edge(t1, HexDirection.W);
        gs.Edges.Add(e1);
        var e2 = new Edge(t1, t2);
        gs.Edges.Add(e2);

        // Act & Assert
        var edge = BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, t1, t2, HexDirection.E);

        Assert.Equal(e2.Id, edge.Id);
    }

    [Fact]
    public void GetEdgeFromTileInfo_EdgeNotFound()
    {
        // Arrange
        GameState gs = new GameState(new Guid());
        var t1 = new Tile(ResourceType.Desert, 0, 0, 0);
        var t2 = new Tile(ResourceType.Wool, 4, 1, -1);
        var t3 = new Tile(ResourceType.Wood, 11, -2, 0);

        var e1 = new Edge(t1, t2);
        gs.Edges.Add(e1);
        var e2 = new Edge(t1, t3);
        gs.Edges.Add(e2);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
                BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, t2, t3, null));

        Assert.Equal("Sequence contains no matching element", exception.Message);
    }

    [Fact]
    public void GetVertexFromTileInfo_IgnoreDirIfMoreThan1Tile()
    {
        // Arrange
        GameState gs = new GameState(new Guid());
        var t1 = new Tile(ResourceType.Desert, 0, 0, 0);
        var t2 = new Tile(ResourceType.Wool, 4, 1, -1);
        var t3 = new Tile(ResourceType.Brick, 6, -1, -1);

        var v1 = new Vertex(t1, t2, t3);
        gs.Vertices.Add(v1);
        var v2 = new Vertex(t1, VertexDirection.SW);

        var vertex = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, t1, t2, t3, VertexDirection.N);

        Assert.Equal(v1.Id, vertex.Id);
    }

    [Fact]
    public void GetVertexFromTileInfo_VertexNotFound()
    {
        // Arrange
        GameState gs = new GameState(new Guid());
        var t1 = new Tile(ResourceType.Desert, 0, 0, 0);
        var t2 = new Tile(ResourceType.Wool, 4, 1, -1);
        var t3 = new Tile(ResourceType.Brick, 6, -1, -1);

        var v1 = new Vertex(t1, t2);
        gs.Vertices.Add(v1);
        var v2 = new Vertex(t1, t3);
        gs.Vertices.Add(v2);

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
                BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, t1, t2, t3, null));

        Assert.Equal("Sequence contains no matching element", exception.Message);
    }

    [Fact]
    public void GetVertexFromTileInfo_3Tiles()
    {
        // Arrange
        GameState gs = new GameState(new Guid());
        var t1 = new Tile(ResourceType.Desert, 0, 0, 0);
        var t2 = new Tile(ResourceType.Wool, 4, 1, -1);
        var t3 = new Tile(ResourceType.Brick, 6, -1, -1);

        var v1 = new Vertex(t1, t2);
        gs.Vertices.Add(v1);
        var v2 = new Vertex(t1, t3);
        gs.Vertices.Add(v2);
        var v3 = new Vertex(t1, t2, t3);
        gs.Vertices.Add(v3);

        // Act & Assert
        var vertex = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, t1, t2, t3, null);

        Assert.Equal(vertex.Id, v3.Id);
    }

    [Fact]
    public void GetVertexFromTileInfo_2Tiles3rdMissing()
    {
        // Arrange
        GameState gs = new GameState(new Guid());
        var t1 = new Tile(ResourceType.Desert, 0, 0, 0);
        var t2 = new Tile(ResourceType.Wool, 4, 1, -1);
        var t3 = new Tile(ResourceType.Brick, 6, -1, -1);

        var v1 = new Vertex(t1, t2);
        gs.Vertices.Add(v1);
        var v2 = new Vertex(t1, t3);
        gs.Vertices.Add(v2);
        var v3 = new Vertex(t1, t2, t3);
        gs.Vertices.Add(v3);

        // Act & Assert
        var vertex = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, t1, t3, null, null);

        Assert.Equal(vertex.Id, v2.Id);
    }

    [Fact]
    public void GetVertexFromTileInfo_2Tiles2ndMissing()
    {
        // Arrange
        GameState gs = new GameState(new Guid());
        var t1 = new Tile(ResourceType.Desert, 0, 0, 0);
        var t2 = new Tile(ResourceType.Wool, 4, 1, -1);
        var t3 = new Tile(ResourceType.Brick, 6, -1, -1);

        var v1 = new Vertex(t1, t2);
        gs.Vertices.Add(v1);
        var v2 = new Vertex(t1, t3);
        gs.Vertices.Add(v2);
        var v3 = new Vertex(t1, t2, t3);
        gs.Vertices.Add(v3);

        // Act & Assert
        var vertex = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, t1, null, t3, null);

        Assert.Equal(vertex.Id, v2.Id);
    }

    [Fact]
    public void GetVertexFromTileInfo_1Tiles()
    {
        // Arrange
        GameState gs = new GameState(new Guid());
        var t1 = new Tile(ResourceType.Desert, 0, 0, 0);
        var t2 = new Tile(ResourceType.Wool, 4, 1, -1);
        var t3 = new Tile(ResourceType.Brick, 6, -1, -1);

        var v1 = new Vertex(t1, t2);
        gs.Vertices.Add(v1);
        var v2 = new Vertex(t1, t3);
        gs.Vertices.Add(v2);
        var v3 = new Vertex(t1, VertexDirection.S);
        gs.Vertices.Add(v3);

        // Act & Assert
        var vertex = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, t1, null, null, VertexDirection.S);

        Assert.Equal(vertex.Id, v3.Id);
    }

    [Fact]
    public void MarkBlockedVertices_CitiesAndSettlements()
    {
        // Find tiles on vertices I will build or test
        GameState gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter);
        var t9 = BoardCreationHelpers.GetTileAt(gs.Tiles, 4, 0);
        var t10 = BoardCreationHelpers.GetTileAt(gs.Tiles, 3, -1);
        var t17 = BoardCreationHelpers.GetTileAt(gs.Tiles, 2, 0);
        var t3 = BoardCreationHelpers.GetTileAt(gs.Tiles, -4, 0);
        var t4 = BoardCreationHelpers.GetTileAt(gs.Tiles, -3, 1);
        var t14 = BoardCreationHelpers.GetTileAt(gs.Tiles, -2, 0);
        var t15 = BoardCreationHelpers.GetTileAt(gs.Tiles, -1, 1);
        var t18 = BoardCreationHelpers.GetTileAt(gs.Tiles, 1, -1);
        var t8 = BoardCreationHelpers.GetTileAt(gs.Tiles, 3, 1);
        var t16 = BoardCreationHelpers.GetTileAt(gs.Tiles, 1, 1);
        var t5 = BoardCreationHelpers.GetTileAt(gs.Tiles, -2, 2);
        var t2 = BoardCreationHelpers.GetTileAt(gs.Tiles, -3, -1);

        // build on vertices
        var v53 = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, t9, null, null, VertexDirection.NE);
        v53.BuildSettlement(gs.Players[1]);
        v53.UpgradeToCity();

        var v43 = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, t9, t10, t17, null);
        v43.BuildSettlement(gs.Players[0]);

        var v19 = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, t3, t4, null, null);
        v19.BuildSettlement(gs.Players[1]);

        // Act
        GamePlayHelpers.MarkBlockedVertices(gs);

        // Assert
        // Verify that vertices that should be blocked by above buildings is blocked
        var v52 = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, t9, t10, null, null);
        Assert.Equal(BuildingType.Blocked, v52.Building);
        var v54 = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, t9, null, null, VertexDirection.SE);
        Assert.Equal(BuildingType.Blocked, v54.Building);
        var v42 = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, t10, t17, t18, null);
        Assert.Equal(BuildingType.Blocked, v42.Building);
        var v39 = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, t8, t9, t17, null);
        Assert.Equal(BuildingType.Blocked, v39.Building);
        var v18 = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, t3, t4, t14, null);
        Assert.Equal(BuildingType.Blocked, v18.Building);
        var v20 = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, t3, null, null, VertexDirection.SW);
        Assert.Equal(BuildingType.Blocked, v20.Building);
        var v25 = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, t4, null, null, VertexDirection.SW);
        Assert.Equal(BuildingType.Blocked, v25.Building);

        // Verify that vertices not blocked by above buildings is free (i.e. null)
        var v51 = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, t10, null, null, VertexDirection.NE);
        Assert.Null(v51.Building);
        var v40 = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, t8, t9, null, null);
        Assert.Null(v40.Building);
        var v34 = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, t8, t16, t17, null);
        Assert.Null(v34.Building);
        var v21 = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, t3, null, null, VertexDirection.NW);
        Assert.Null(v21.Building);
        var v24 = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, t4, t5, null, null);
        Assert.Null(v24.Building);
        var v15 = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, t2, t3, t14, null);
        Assert.Null(v15.Building);
        var v22 = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, t4, t14, t15, null);
        Assert.Null(v22.Building);

    }

    [Fact]
    public void IsEdgeAdjacentToPlayerBuild_AdjacentToRoad()
    {
        GameState gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter);

        var woodTile = BoardCreationHelpers.GetTileAt(gs.Tiles, 2, 0);
        var oreTile = BoardCreationHelpers.GetTileAt(gs.Tiles, 4, 0);
        var woolTile = BoardCreationHelpers.GetTileAt(gs.Tiles, 3, 1);

        var testEdge = BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, woodTile, woolTile, null);
        var road = BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, oreTile, woolTile, null);
        road.BuildRoad(gs.Players[0]);

        Assert.True(GamePlayHelpers.IsEdgeAdjacentToPlayerBuild(gs, testEdge, gs.Players[0]));
    }

    [Fact]
    public void IsEdgeAdjacentToPlayerBuild_AdjacentToSettlement()
    {
        GameState gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter);

        var woodTile = BoardCreationHelpers.GetTileAt(gs.Tiles, 2, 0);
        var oreTile = BoardCreationHelpers.GetTileAt(gs.Tiles, 4, 0);
        var woolTile = BoardCreationHelpers.GetTileAt(gs.Tiles, 3, 1);

        var testEdge = BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, woodTile, woolTile, null);
        var settlement = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, oreTile, woolTile, woodTile, null);
        settlement.BuildSettlement(gs.Players[0]);

        Assert.True(GamePlayHelpers.IsEdgeAdjacentToPlayerBuild(gs, testEdge, gs.Players[0]));
    }

    [Fact]
    public void IsEdgeAdjacentToPlayerBuild_AdjacentToCity()
    {
        GameState gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter);

        var woodTile = BoardCreationHelpers.GetTileAt(gs.Tiles, 2, 0);
        var oreTile = BoardCreationHelpers.GetTileAt(gs.Tiles, 4, 0);
        var woolTile = BoardCreationHelpers.GetTileAt(gs.Tiles, 3, 1);

        var testEdge = BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, woodTile, woolTile, null);
        var city = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, oreTile, woolTile, woodTile, null);
        city.BuildSettlement(gs.Players[0]);
        city.UpgradeToCity();

        Assert.True(GamePlayHelpers.IsEdgeAdjacentToPlayerBuild(gs, testEdge, gs.Players[0]));
    }

    [Fact]
    public void IsEdgeAdjacentToPlayerBuild_NotAdjacentToAnything()
    {
        GameState gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter);

        var woodTile = BoardCreationHelpers.GetTileAt(gs.Tiles, 2, 0);
        var oreTile = BoardCreationHelpers.GetTileAt(gs.Tiles, 4, 0);
        var woolTile = BoardCreationHelpers.GetTileAt(gs.Tiles, 3, 1);

        var testEdge = BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, woodTile, woolTile, null);
        var road = BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, woolTile, null, HexDirection.E);
        road.BuildRoad(gs.Players[0]);

        Assert.False(GamePlayHelpers.IsEdgeAdjacentToPlayerBuild(gs, testEdge, gs.Players[0]));
    }

    [Fact]
    public void IsEdgeAdjacentToPlayerBuild_NotAdjacentToRightPlayer()
    {
        GameState gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter);

        var woodTile = BoardCreationHelpers.GetTileAt(gs.Tiles, 2, 0);
        var oreTile = BoardCreationHelpers.GetTileAt(gs.Tiles, 4, 0);
        var woolTile = BoardCreationHelpers.GetTileAt(gs.Tiles, 3, 1);

        var testEdge = BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, woodTile, woolTile, null);
        var road = BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, oreTile, woolTile, null);
        road.BuildRoad(gs.Players[1]);

        Assert.False(GamePlayHelpers.IsEdgeAdjacentToPlayerBuild(gs, testEdge, gs.Players[0]));
    }

    [Fact]
    public void IsEdgeAdjacentToPlayerBuild_NotAdjacentIfRoadSplitByOtherPlayer()
    {
        GameState gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter);

        var woodTile = BoardCreationHelpers.GetTileAt(gs.Tiles, 2, 0);
        var oreTile = BoardCreationHelpers.GetTileAt(gs.Tiles, 4, 0);
        var woolTile = BoardCreationHelpers.GetTileAt(gs.Tiles, 3, 1);

        var testEdge = BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, woodTile, woolTile, null);
        var road = BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, oreTile, woolTile, null);
        road.BuildRoad(gs.Players[0]);

        var settlement = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, oreTile, woolTile, woodTile, null);
        settlement.BuildSettlement(gs.Players[1]);

        Assert.False(GamePlayHelpers.IsEdgeAdjacentToPlayerBuild(gs, testEdge, gs.Players[0]));
    }

    [Fact]
    public void IsVertexAdjacentToPlayerRoad_NotAdjacentToAnything()
    {
        GameState gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter);

        var woodTile = BoardCreationHelpers.GetTileAt(gs.Tiles, 2, 0);
        var oreTile = BoardCreationHelpers.GetTileAt(gs.Tiles, 4, 0);
        var woolTile = BoardCreationHelpers.GetTileAt(gs.Tiles, 3, 1);

        var testVertex = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, woodTile, oreTile, woolTile, null);
        var road = BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, oreTile, null, HexDirection.NE);
        road.BuildRoad(gs.Players[0]);

        Assert.False(GamePlayHelpers.IsVertexAdjacentToPlayerRoad(gs, testVertex, gs.Players[0]));
    }

    [Fact]
    public void IsVertexAdjacentToPlayerRoad_AdjacentToRoad()
    {
        GameState gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter);

        var woodTile = BoardCreationHelpers.GetTileAt(gs.Tiles, 2, 0);
        var oreTile = BoardCreationHelpers.GetTileAt(gs.Tiles, 4, 0);
        var woolTile = BoardCreationHelpers.GetTileAt(gs.Tiles, 3, 1);

        var testVertex = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, woodTile, oreTile, woolTile, null);
        var road = BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, oreTile, woodTile, null);
        road.BuildRoad(gs.Players[0]);

        Assert.True(GamePlayHelpers.IsVertexAdjacentToPlayerRoad(gs, testVertex, gs.Players[0]));
    }

    [Fact]
    public void BankTradeFromUser_Accepted()
    {
        GameState gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter);
        gs.Phase.PhaseState = GameStates.BuildOrTrade;
        gs.Phase.CurrentPlayer = gs.Players[0];

        gs.Players[0].AssignResources(ResourceType.Wood, 5);
        var tradeDTO = new TradeRequestDTO(gs.Players[0].Id,
                new Dictionary<string, int>() { { ResourceType.Wood.ToString(), 4 } },
                new Dictionary<string, int>() { { ResourceType.Brick.ToString(), 1 } });

        var response = GamePlayHelpers.BankTradeFromUser(gs, tradeDTO);

        Assert.True(response.Success);
        Assert.Equal(1, gs.Players[0].Resources[ResourceType.Wood]);
        Assert.Equal(1, gs.Players[0].Resources[ResourceType.Brick]);
    }

    [Fact]
    public void BankTradeFromUser_Rejected_WrongState()
    {
        GameState gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter);
        gs.Phase.PhaseState = GameStates.RollOrUseDevCard;

        gs.Players[0].AssignResources(ResourceType.Wood, 5);
        var tradeDTO = new TradeRequestDTO(gs.Players[0].Id,
                new Dictionary<string, int>() { { ResourceType.Wood.ToString(), 4 } },
                new Dictionary<string, int>() { { ResourceType.Brick.ToString(), 1 } });

        var response = GamePlayHelpers.BankTradeFromUser(gs, tradeDTO);

        Assert.False(response.Success);
        Assert.Equal(1003, response.ErrorCode);
        Assert.Equal(5, gs.Players[0].Resources[ResourceType.Wood]);
        Assert.Equal(0, gs.Players[0].Resources[ResourceType.Brick]);
    }

    [Fact]
    public void BankTradeFromUser_Rejected_DoesNotHaveEnoughResources()
    {
        GameState gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter);
        gs.Phase.PhaseState = GameStates.BuildOrTrade;
        gs.Phase.CurrentPlayer = gs.Players[0];


        gs.Players[0].AssignResources(ResourceType.Wood, 2);
        var tradeDTO = new TradeRequestDTO(gs.Players[0].Id,
                new Dictionary<string, int>() { { ResourceType.Wood.ToString(), 4 } },
                new Dictionary<string, int>() { { ResourceType.Brick.ToString(), 1 } });

        var response = GamePlayHelpers.BankTradeFromUser(gs, tradeDTO);

        Assert.False(response.Success);
        Assert.Equal(1006, response.ErrorCode);
        Assert.Equal(2, gs.Players[0].Resources[ResourceType.Wood]);
        Assert.Equal(0, gs.Players[0].Resources[ResourceType.Brick]);
    }

    [Fact]
    public void BankTrade_Accepted()
    {
        GameState gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter);

        gs.Players[0].AssignResources(ResourceType.Wood, 5);
        var tradeRequest = new TradeRequest(gs.Players[0],
                new Dictionary<ResourceType, int>() { { ResourceType.Wood, 4 } },
                new Dictionary<ResourceType, int>() { { ResourceType.Brick, 1 } });

        var response = GamePlayHelpers.BankTrade(new GameState(new Guid()), tradeRequest);

        Assert.True(response.Success);
        Assert.Equal(1, gs.Players[0].Resources[ResourceType.Wood]);
        Assert.Equal(1, gs.Players[0].Resources[ResourceType.Brick]);
    }

    [Fact]
    public void BankTrade_Rejected()
    {
        GameState gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter);

        gs.Players[0].AssignResources(ResourceType.Wood, 3);
        var tradeRequest = new TradeRequest(gs.Players[0],
                new Dictionary<ResourceType, int>() { { ResourceType.Wood, 2 } },
                new Dictionary<ResourceType, int>() { { ResourceType.Brick, 1 } });

        var response = GamePlayHelpers.BankTrade(new GameState(new Guid()), tradeRequest);

        Assert.False(response.Success);
        Assert.Equal(1008, response.ErrorCode);
        Assert.Equal(3, gs.Players[0].Resources[ResourceType.Wood]);
        Assert.Equal(0, gs.Players[0].Resources[ResourceType.Brick]);
    }

    private static GameState CreateBoardWithOnlyOneOfEachBuildAvailable()
    {
        var gs = BoardCreationHelpers.CreateNewBoard(GameType.Test);
        var player = gs.Players.First(p => !p.IsBot);

        var desertTile = BoardCreationHelpers.GetTileAt(gs.Tiles, -1, -1);
        var brickTile = BoardCreationHelpers.GetTileAt(gs.Tiles, -2, 0);
        var wool2Tile = BoardCreationHelpers.GetTileAt(gs.Tiles, -1, 1);
        var grainTile = BoardCreationHelpers.GetTileAt(gs.Tiles, 0, 0);

        // build all but one road
        BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, brickTile, null, HexDirection.SW).BuildRoad(player);
        BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, brickTile, grainTile, null).BuildRoad(player);
        BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, brickTile, desertTile, null).BuildRoad(player);
        BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, grainTile, desertTile, null).BuildRoad(player);

        // build all but one city
        gs.Vertices.First(v => v.Building == BuildingType.Settlement && v.Owner != null && v.Owner.Id == player.Id).UpgradeToCity();

        // build all but one settlement
        BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, brickTile, null, null, VertexDirection.SW).BuildSettlement(player);
        BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, brickTile, desertTile, null, null).BuildSettlement(player);

        return gs;
    }

    private static GameState CreateBoardWithAllBuildingsInUse()
    {
        var gs = CreateBoardWithOnlyOneOfEachBuildAvailable();
        var player = gs.Players.First(p => !p.IsBot);

        // Build remaining buildings to hit max
        var wool2Tile = BoardCreationHelpers.GetTileAt(gs.Tiles, -1, 1);
        var desertTile = BoardCreationHelpers.GetTileAt(gs.Tiles, -1, -1);
        var wool11Tile = BoardCreationHelpers.GetTileAt(gs.Tiles, 1, -1);
        var grainTile = BoardCreationHelpers.GetTileAt(gs.Tiles, 0, 0);

        var v1 = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, desertTile, grainTile, wool11Tile, null);
        v1.BuildSettlement(player);
        v1.UpgradeToCity();

        BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, wool2Tile, null, HexDirection.W).BuildRoad(player);
        BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, wool2Tile, null, null, VertexDirection.SW).BuildSettlement(player);

        return gs;
    }

    [Fact]
    public void UnusedRoadAvailable_Yes()
    {
        // Arrange
        var gs = CreateBoardWithOnlyOneOfEachBuildAvailable();
        var player = gs.Players.First(p => !p.IsBot);

        // Act & Assert
        Assert.True(GamePlayHelpers.UnusedRoadAvailable(gs, player)); 
    }

    [Fact]
    public void UnusedRoadAvailable_No()
    {
        // Arrange
        var gs = CreateBoardWithAllBuildingsInUse();
        var player = gs.Players.First(p => !p.IsBot);

        // Act & Assert
        Assert.False(GamePlayHelpers.UnusedRoadAvailable(gs, player)); 
    }

    [Fact]
    public void UnusedSettlementAvailable_Yes()
    {
        // Arrange
        var gs = CreateBoardWithOnlyOneOfEachBuildAvailable();
        var player = gs.Players.First(p => !p.IsBot);

        // Act & Assert
        Assert.True(GamePlayHelpers.UnusedSettlementAvailable(gs, player)); 
    }

    [Fact]
    public void UnusedSettlementAvailable_No()
    {
        // Arrange
        var gs = CreateBoardWithAllBuildingsInUse();
        var player = gs.Players.First(p => !p.IsBot);

        // Act & Assert
        Assert.False(GamePlayHelpers.UnusedSettlementAvailable(gs, player)); 
    }

    [Fact]
    public void UnusedCityAvailable_Yes()
    {
        // Arrange
        var gs = CreateBoardWithOnlyOneOfEachBuildAvailable();
        var player = gs.Players.First(p => !p.IsBot);

        // Act & Assert
        Assert.True(GamePlayHelpers.UnusedCityAvailable(gs, player)); 
    }

    [Fact]
    public void UnusedCityAvailable_No()
    {
        // Arrange
        var gs = CreateBoardWithAllBuildingsInUse();
        var player = gs.Players.First(p => !p.IsBot);

        // Act & Assert
        Assert.False(GamePlayHelpers.UnusedCityAvailable(gs, player)); 
    }

    [Fact]
    public void PopulatePlayerPorts_Valid()
    {
        // Arrange
        var gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter);
        var bot = gs.Players.First(p => p.IsBot);
        var human = gs.Players.First(p => !p.IsBot);

        var grain12Tile = BoardCreationHelpers.GetTileAt(gs.Tiles, -3, -1);
        var grain9Tile = BoardCreationHelpers.GetTileAt(gs.Tiles, -4, 0);
        var brick8Tile = BoardCreationHelpers.GetTileAt(gs.Tiles, -3, 1);
        var ore8Tile = BoardCreationHelpers.GetTileAt(gs.Tiles, 4, 0);


        BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, grain12Tile, grain9Tile, null, null).BuildSettlement(human);
        var v1 = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, grain9Tile, brick8Tile, null, null);
        v1.BuildSettlement(human);
        v1.UpgradeToCity();

        BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, ore8Tile, null, null, VertexDirection.NE).BuildSettlement(bot);

        // Act
        GamePlayHelpers.PopulatePlayerPorts(gs);

        // Assert
        Assert.Equal(2, human.Ports.Count);
        Assert.Contains(PortType.Wood, human.Ports);
        Assert.Contains(PortType.Brick, human.Ports);
        Assert.Single(bot.Ports);
        Assert.Contains(PortType.ThreeToOne, bot.Ports);
    }

    [Fact]
    public void RollDice_InvalidState_Exception()
    {
        // Arrange
        var gs = BoardCreationHelpers.CreateNewBoard(GameType.Test);
        gs.Phase.PhaseState = GameStates.BuildOrTrade;
        gs.Phase.CurrentPlayer = gs.Players[0];

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.RollDice(gs));
    }

    private static GameState CreateGameForRobberTesting(GameStates previousState)
    {
        var gs = new GameState(new Guid());
        var player1 = new Player("Time", PlayerColor.Red);
        gs.Players.Add(player1);
        var player2 = new Player("Mary", PlayerColor.Blue);
        gs.Players.Add(player2);

        var tile1 = new Tile(ResourceType.Wood, 3, 0, 0);
        gs.Tiles.Add(tile1);
        var tile2 = new Tile(ResourceType.Brick, 9, 2, 0);
        gs.Tiles.Add(tile2);

        gs.Phase.CurrentPlayer = player1;
        gs.Phase.PhaseState = GameStates.PlaceRobber;
        gs.Phase.SetStateToReturnTo(previousState, tile1);

        return gs;        
    }

    [Fact]
    public void PlaceRobberForUser_InvalidState()
    {
        var gs = CreateGameForRobberTesting(GameStates.RollOrUseDevCard);
        gs.Phase.PhaseState = GameStates.RollOrUseDevCard;

        var response = GamePlayHelpers.PlaceRobberForUser(gs, gs.Players[0].Id, gs.Tiles[0].Id);

        Assert.False(response.Success);
        Assert.Equal(1003, response.ErrorCode);
    }

    [Fact]
    public void PlaceRobberForUser_NotPlayersTurn()
    {
        var gs = CreateGameForRobberTesting(GameStates.RollOrUseDevCard);

        var response = GamePlayHelpers.PlaceRobberForUser(gs, gs.Players[1].Id, gs.Tiles[0].Id);

        Assert.False(response.Success);
        Assert.Equal(1011, response.ErrorCode);
    }

    [Fact]
    public void PlaceRobberForUser_UnknownTile()
    {
        var gs = CreateGameForRobberTesting(GameStates.RollOrUseDevCard);
        var tile = new Tile(ResourceType.Ore, 5, -2, 0);

        var response = GamePlayHelpers.PlaceRobberForUser(gs, gs.Players[0].Id, tile.Id);

        Assert.False(response.Success);
        Assert.Equal(1032, response.ErrorCode);
    }

    [Fact]
    public void PlaceRobberForUser_RobberNotMoved()
    {
        var gs = CreateGameForRobberTesting(GameStates.RollOrUseDevCard);

        var response = GamePlayHelpers.PlaceRobberForUser(gs, gs.Players[0].Id, gs.Phase.OriginalRobberTile.Id);

        Assert.False(response.Success);
        Assert.Equal(1033, response.ErrorCode);
    }

    [Fact]
    public void PlaceRobberForUser_ValidMove()
    {
        var gs = CreateGameForRobberTesting(GameStates.BuildOrTrade);

        var response = GamePlayHelpers.PlaceRobberForUser(gs, gs.Players[0].Id, gs.Tiles[1].Id);

        Assert.True(response.Success);
        Assert.Equal(gs.Tiles[1].Id, gs.RobberTile.Id);
        Assert.Null(gs.Phase.PreviousState);
    }

    [Fact]
    public void PlaceRobber_ValidMove()
    {
        // Arrange
        var gs = BoardCreationHelpers.CreateNewBoard(GameType.Test);
        var human = gs.Players.First(p => !p.IsBot);
        var bot = gs.Players.First(p => p.IsBot);
        bot.AssignResources(ResourceType.Ore, 2);
        var botTile = gs.Vertices.First(v => v.Owner != null && v.Owner.Id == bot.Id).Tiles.First(t => t.Resource != ResourceType.Desert);

        gs.Phase.CurrentPlayer = human;
        gs.Phase.PhaseState = GameStates.PlaceRobber;
        gs.Phase.SetStateToReturnTo(GameStates.BuildOrTrade, gs.RobberTile);

        Assert.Contains(ResourceType.Ore, human.Resources);
        Assert.Equal(0, human.Resources[ResourceType.Ore]);
        Assert.Contains(ResourceType.Ore, bot.Resources);
        Assert.Equal(2, bot.Resources[ResourceType.Ore]);

        // Act
        GamePlayHelpers.PlaceRobber(gs, human, botTile);

        // Assert
        Assert.Equal(botTile.Id, gs.RobberTile.Id);
        Assert.Null(gs.Phase.PreviousState);
        Assert.Equal(GameStates.BuildOrTrade, gs.Phase.PhaseState);
        Assert.Contains(ResourceType.Ore, human.Resources);
        Assert.Equal(1, human.Resources[ResourceType.Ore]);
        Assert.Contains(ResourceType.Ore, bot.Resources);
        Assert.Equal(1, bot.Resources[ResourceType.Ore]);
    }

    [Fact]
    public void BuyDevCard_NotCurrentPlayer()
    {
        Assert.False(true);
    }

    [Fact]
    public void BuyDevCard_WrongState()
    {
        Assert.False(true);
    }

    [Fact]
    public void BuyDevCard_Valid()
    {
        Assert.False(true);
    }

    // TODO: Add all the PlayDevCard tests
    [Fact]
    public void PlayDevCard_ALLTYPES()
    {
        Assert.False(true);
    }

}
