using Xunit;
using GameTest.Models;
using GameTest.DTOs;
using GameTest.Services;
using GameTest.Functions;

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

        var targetTile = BoardCreationHelpers.GetRequiredTileAt(gameState.Tiles, 0, 0);
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
    public void WithdrawResourcesToBuildRoad_SufficientResources()
    {
        // Arrange
        var player = CreatePlayerWithSufficientResources();
        var woodCount = player.Resources[ResourceType.Wood];
        var brickCount = player.Resources[ResourceType.Brick];

        // Act
        var result = GamePlayHelpers.WithdrawResourcesToBuildRoad(player);

        // Assert
        Assert.True(result);
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
        var result = GamePlayHelpers.WithdrawResourcesToBuildRoad(player);

        // Assert
        Assert.False(result);
        Assert.Equal(woodCount, player.Resources[ResourceType.Wood]);
        Assert.Equal(brickCount, player.Resources[ResourceType.Brick]);
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
        var result = GamePlayHelpers.WithdrawResourcesToBuildSettlement(player);

        // Assert
        Assert.True(result);
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
        var result = GamePlayHelpers.WithdrawResourcesToBuildSettlement(player);

        // Assert
        Assert.False(result);
        Assert.Equal(woodCount, player.Resources[ResourceType.Wood]);
        Assert.Equal(brickCount, player.Resources[ResourceType.Brick]);
        Assert.Equal(woolCount, player.Resources[ResourceType.Wool]);
        Assert.Equal(grainCount, player.Resources[ResourceType.Grain]);
    }

    [Fact]
    public void WithdrawResourcesToBuildCity_SufficientResources()
    {
        // Arrange
        var player = CreatePlayerWithSufficientResources();
        var oreCount = player.Resources[ResourceType.Ore];
        var grainCount = player.Resources[ResourceType.Grain];

        // Act
        var result = GamePlayHelpers.WithdrawResourcesToBuildCity(player);

        // Assert
        Assert.True(result);
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
        var result = GamePlayHelpers.WithdrawResourcesToBuildCity(player);

        // Assert
        Assert.False(result);
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

    private GameState CreateGameStateForPhaseTesting()
    {
        var gs = new GameState(new Guid());
        gs.Tiles.AddRange(BoardCreationHelpers.CreateTilesForTestBoard());
        BoardCreationHelpers.CreateEdgesAndVerticesForBoard(gs);
        gs.AddPlayer(new Player("George", PlayerColor.White));
        gs.AddPlayer(new Player("Elaine", PlayerColor.Green));

        return gs;
    }

    [Fact]
    public void GetNextPhase_PrePlay_MoveToSetupSettlementAsc()
    {
        var gs = CreateGameStateForPhaseTesting();
        gs.Phase = new GamePhase(GameStates.PrePlay);

        var phase = GamePlayHelpers.GetNextPhase(gs);

        Assert.Equal(GameStates.SetUpSettlementAsc, phase.PhaseState);

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
    public void GetNextPhase_SetupSettlementAsc_NoSettlementStayPut()
    {
        var gs = CreateGameStateForPhaseTesting();
        gs.Phase = new GamePhase(GameStates.SetUpSettlementAsc, gs.Players[0], gs.Players[1]);

        var phase = GamePlayHelpers.GetNextPhase(gs);

        Assert.Equal(GameStates.SetUpSettlementAsc, phase.PhaseState);
        Assert.Equal(gs.Players[0], phase.CurrentPlayer);
        Assert.Equal(gs.Players[1], phase.EndPlayer);
    }

    [Fact]
    public void GetNextPhase_SetupSettlementAsc_MoveToSetupRoadAsc()
    {
        var gs = CreateGameStateForPhaseTesting();
        gs.Phase = new GamePhase(GameStates.SetUpSettlementAsc, gs.Players[0], gs.Players[1]);
        Assert.NotNull(gs.Phase.CurrentPlayer);
        gs.Vertices[0].BuildSettlement(gs.Phase.CurrentPlayer);

        var phase = GamePlayHelpers.GetNextPhase(gs);

        Assert.Equal(GameStates.SetUpRoadAsc, phase.PhaseState);
        Assert.Equal(gs.Players[0], phase.CurrentPlayer);
        Assert.Equal(gs.Players[1], phase.EndPlayer);
    }

    [Fact]
    public void GetNextPhase_SetupRoadAsc_NoRoadStayPut()
    {
        var gs = CreateGameStateForPhaseTesting();
        gs.Phase = new GamePhase(GameStates.SetUpRoadAsc, gs.Players[0], gs.Players[1]);
        Assert.NotNull(gs.Phase.CurrentPlayer);
        gs.Vertices[0].BuildSettlement(gs.Phase.CurrentPlayer);

        var phase = GamePlayHelpers.GetNextPhase(gs);

        Assert.Equal(GameStates.SetUpRoadAsc, phase.PhaseState);
        Assert.Equal(gs.Players[0], phase.CurrentPlayer);
        Assert.Equal(gs.Players[1], phase.EndPlayer);
    }

    [Fact]
    public void GetNextPhase_SetupRoadAsc_MoveToNextPlayer()
    {
        Assert.False(true);
    }

    [Fact]
    public void GetNextPhase_SetupRoadAsc_MoveToSetupSettlementDesc()
    {

    }

    [Fact]
    public void GetNextPhase_SetupSettlement_MoveToSetupRoadDesc()
    {

    }

    [Fact]
    public void GetNextPhase_SetupRoadDesc_MoveToNextPlayer()
    {

    }

    [Fact]
    public void GetNextPhase_SetupRoadDesc_MoveToPreRoll()
    {

    }

    [Fact]
    public void GetNextPhase_PreRoll_MoveToPostRoll()
    {

    }

    [Fact]
    public void GetNextPhase_PostRoll_MoveToNextPlayer()
    {

    }

    [Fact]
    public void GetNextPhase_PostRoll_MoveToGameOver()
    {

    }


}
