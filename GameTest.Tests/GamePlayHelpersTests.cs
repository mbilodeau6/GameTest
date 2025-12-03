using Xunit;
using GameTest.Models;
using GameTest.DTOs;
using GameTest.Services;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

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
    public void HasResourcesToBuyDevCard_SufficientResources()
    {
        var player = CreatePlayerWithSufficientResources();
        Assert.True(GamePlayHelpers.HasResourcesToBuyDevCard(player));        
    }

    [Fact]
    public void HasResourcesToBuyDevCard_InsufficientResources()
    {
        var player = CreatePlayerWithInsufficientResources();
        Assert.False(GamePlayHelpers.HasResourcesToBuyDevCard(player));        
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
        GamePlayHelpers.WithdrawResourcesToBuyDevCard(player);

        // Assert
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
        GamePlayHelpers.WithdrawResourcesToBuyDevCard(player);

        // Assert
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

    [Fact]
    public void CountVictoryPointsForPlayer_IncludeAllDevCardBuckets()
    {
        // Arrage
        var player = new Player("Allen", PlayerColor.Red);
        player.AssignDevelopmentCard(DevelopmentCardType.VictoryPoint);
        player.AssignDevelopmentCard(DevelopmentCardType.RoadBuilding);
        player.MakeNewDevelopmentCardsPlayable();
        player.AssignDevelopmentCard(DevelopmentCardType.VictoryPoint);

        // Act
        var count = GamePlayHelpers.CountVictoryPointDevCardsForPlayer(player);

        // Assert
        Assert.Equal(2, count);
    }

    [Fact]
    public void UpdatePlayerVictoryPoints_BluePlayer()
    {
        var gs = CreateTestGameWithManyRoads();
        var bluePlayer = gs.Players.First(p => p.Color == PlayerColor.Blue);
        GamePlayHelpers.UpdatePlayerVictoryPoints(gs, bluePlayer);
        Assert.Equal(5, bluePlayer.VictoryPoints);

        bluePlayer.AssignDevelopmentCard(DevelopmentCardType.VictoryPoint);
        GamePlayHelpers.UpdatePlayerVictoryPoints(gs, bluePlayer);
        Assert.Equal(6, bluePlayer.VictoryPoints);

        bluePlayer.MakeNewDevelopmentCardsPlayable();
        bluePlayer.AssignDevelopmentCard(DevelopmentCardType.VictoryPoint);
        GamePlayHelpers.UpdatePlayerVictoryPoints(gs, bluePlayer);
        Assert.Equal(7, bluePlayer.VictoryPoints);
    }

    private void BuildTestCities(TestGameBoard board, int count)
    {
        int vIndex = 0;

        for (int i = 0; i < count; i++)
        {
            while (board.GetGameState().Vertices[vIndex].Building != null)
                vIndex++;

            board.GetGameState().Vertices[vIndex].BuildSettlement(board.GetCurrentPlayer());
            board.GetGameState().Vertices[vIndex].UpgradeToCity();
        }
    }

    private void BuildTestSettlements(TestGameBoard board, int count)
    {
        int vIndex = 0;

        for (int i = 0; i < count; i++)
        {
            while (board.GetGameState().Vertices[vIndex].Building != null)
                vIndex++;

            board.GetGameState().Vertices[vIndex].BuildSettlement(board.GetCurrentPlayer());
        }
    }

    [Fact]
    public void PlayerHasWon_DefaultThreshold_Not_Met()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.RollOrUseDevCard, board.GetRedPlayer(), board.GetBluePlayer());
        Assert.True(board.InExpectedState(GameStates.RollOrUseDevCard, board.GetRedPlayer()));
        BuildTestCities(board, 4);
        BuildTestSettlements(board, 1);

        Assert.False(GamePlayHelpers.PlayerHasWon(board.GetGameState(), board.GetRedPlayer()));
    }

    [Fact]
    public void PlayerHasWon_DefaultThreshold_Met()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.RollOrUseDevCard, board.GetRedPlayer(), board.GetBluePlayer());
        Assert.True(board.InExpectedState(GameStates.RollOrUseDevCard, board.GetRedPlayer()));
        BuildTestCities(board, 4);
        BuildTestSettlements(board, 2);

        Assert.True(GamePlayHelpers.PlayerHasWon(board.GetGameState(), board.GetRedPlayer()));
    }

    // TODO: Add tests where victory points come from dev cards, longest road, and largest army

    private static bool IsNextPhaseAsExpected(GamePhase nextPhase, GameStates expectedState, Player expectedCurrent, Player? expectedEnd = null)
    {
        var result = nextPhase.PhaseState.Equals(expectedState)
            && nextPhase.CurrentPlayer != null
            && nextPhase.CurrentPlayer.Id.Equals(expectedCurrent.Id);

        if (expectedEnd != null)
            return result && nextPhase.EndPlayer != null 
                && nextPhase.EndPlayer.Id.Equals(expectedEnd.Id);
        else
            return result;
    }

    [Fact]
    public void GetNextPhase_SettingUpBoard_MoveToPlaceFirstSettlement()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.SettingUpBoard);

        var phase = GamePlayHelpers.GetNextPhase(board.GetGameState());

        Assert.NotNull(phase.CurrentPlayer);
        if (phase.CurrentPlayer.Id == board.GetRedPlayer().Id)
            Assert.True(IsNextPhaseAsExpected(phase, GameStates.PlaceFirstSettlement, board.GetRedPlayer(), board.GetBluePlayer()));
        else
            Assert.True(IsNextPhaseAsExpected(phase, GameStates.PlaceFirstSettlement, board.GetBluePlayer(), board.GetRedPlayer()));
    }

    [Fact]
    public void GetNextPhase_PlaceFirstSettlement_NoSettlementStayPut()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceFirstSettlement, board.GetRedPlayer(), board.GetBluePlayer());

        var phase = GamePlayHelpers.GetNextPhase(board.GetGameState());

        Assert.True(IsNextPhaseAsExpected(phase, GameStates.PlaceFirstSettlement, board.GetRedPlayer(), board.GetBluePlayer()));
    }

    [Fact]
    public void GetNextPhase_PlaceFirstSettlement_MoveToPlaceFirstRoad()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceFirstSettlement, board.GetRedPlayer(), board.GetBluePlayer());
        Assert.NotNull(board.GetGameState().Phase.CurrentPlayer);
        board.GetVertex(TestVertex.V2).BuildSettlement(board.GetRedPlayer());

        var phase = GamePlayHelpers.GetNextPhase(board.GetGameState());

        Assert.True(IsNextPhaseAsExpected(phase, GameStates.PlaceFirstRoad, board.GetRedPlayer(), board.GetBluePlayer()));
    }

    [Fact]
    public void GetNextPhase_PlaceFirstRoad_NoRoadStayPut()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceFirstRoad, board.GetRedPlayer(), board.GetBluePlayer());
        board.GetVertex(TestVertex.V2).BuildSettlement(board.GetRedPlayer());

        var phase = GamePlayHelpers.GetNextPhase(board.GetGameState());

        Assert.True(IsNextPhaseAsExpected(phase, GameStates.PlaceFirstRoad, board.GetRedPlayer(), board.GetBluePlayer()));
    }

    [Fact]
    public void GetNextPhase_PlaceFirstRoad_MoveToNextPlayer()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceFirstRoad, board.GetRedPlayer(), board.GetBluePlayer());
        board.GetVertex(TestVertex.V2).BuildSettlement(board.GetRedPlayer());
        board.GetEdge(TestEdge.E2).BuildRoad(board.GetRedPlayer());

        var phase = GamePlayHelpers.GetNextPhase(board.GetGameState());

        Assert.True(IsNextPhaseAsExpected(phase, GameStates.PlaceFirstSettlement, board.GetBluePlayer(), board.GetBluePlayer()));
    }

    [Fact]
    public void GetNextPhase_PlaceFirstRoad_MoveToPlaceSecondSettlement()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceFirstRoad, board.GetBluePlayer(), board.GetBluePlayer());
        board.GetVertex(TestVertex.V2).BuildSettlement(board.GetBluePlayer());
        board.GetEdge(TestEdge.E2).BuildRoad(board.GetBluePlayer());

        var phase = GamePlayHelpers.GetNextPhase(board.GetGameState());

        Assert.True(IsNextPhaseAsExpected(phase, GameStates.PlaceSecondSettlement, board.GetBluePlayer(), board.GetRedPlayer()));
    }

    [Fact]
    public void GetNextPhase_PlaceSecondSettlement_No2ndStayPut()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceSecondSettlement, board.GetBluePlayer(), board.GetRedPlayer());
        board.GetVertex(TestVertex.V2).BuildSettlement(board.GetBluePlayer());
        board.GetEdge(TestEdge.E2).BuildRoad(board.GetBluePlayer());

        var phase = GamePlayHelpers.GetNextPhase(board.GetGameState());

        Assert.True(IsNextPhaseAsExpected(phase, GameStates.PlaceSecondSettlement, board.GetBluePlayer(), board.GetRedPlayer()));
    }

    [Fact]
    public void GetNextPhase_PlaceSecondSettlement_MoveToPlaceSecondRoad()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceSecondSettlement, board.GetBluePlayer(), board.GetRedPlayer());
        board.GetVertex(TestVertex.V2).BuildSettlement(board.GetBluePlayer());
        board.GetVertex(TestVertex.V4).BuildSettlement(board.GetBluePlayer());
        board.GetEdge(TestEdge.E2).BuildRoad(board.GetBluePlayer());

        var phase = GamePlayHelpers.GetNextPhase(board.GetGameState());

        Assert.True(IsNextPhaseAsExpected(phase, GameStates.PlaceSecondRoad, board.GetBluePlayer(), board.GetRedPlayer()));
    }

    [Fact]
    public void GetNextPhase_PlaceSecondRoad_No2ndStayPut()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceSecondRoad, board.GetBluePlayer(), board.GetRedPlayer());
        board.GetVertex(TestVertex.V2).BuildSettlement(board.GetBluePlayer());
        board.GetVertex(TestVertex.V4).BuildSettlement(board.GetBluePlayer());
        board.GetEdge(TestEdge.E2).BuildRoad(board.GetBluePlayer());

        var phase = GamePlayHelpers.GetNextPhase(board.GetGameState());

        Assert.True(IsNextPhaseAsExpected(phase, GameStates.PlaceSecondRoad, board.GetBluePlayer(), board.GetRedPlayer()));
    }

    [Fact]
    public void GetNextPhase_PlaceSecondRoad_MoveToNextPlayer()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceSecondRoad, board.GetBluePlayer(), board.GetRedPlayer());
        board.GetVertex(TestVertex.V2).BuildSettlement(board.GetBluePlayer());
        board.GetVertex(TestVertex.V4).BuildSettlement(board.GetBluePlayer());
        board.GetEdge(TestEdge.E2).BuildRoad(board.GetBluePlayer());
        board.GetEdge(TestEdge.E4).BuildRoad(board.GetBluePlayer());

        var phase = GamePlayHelpers.GetNextPhase(board.GetGameState());

        Assert.True(IsNextPhaseAsExpected(phase, GameStates.PlaceSecondSettlement, board.GetRedPlayer(), board.GetRedPlayer()));
    }

    [Fact]
    public void GetNextPhase_PlaceSecondRoad_MoveToRollOrUseDevCard()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceSecondRoad, board.GetRedPlayer(), board.GetRedPlayer());
        board.GetVertex(TestVertex.V2).BuildSettlement(board.GetRedPlayer());
        board.GetVertex(TestVertex.V4).BuildSettlement(board.GetRedPlayer());
        board.GetEdge(TestEdge.E2).BuildRoad(board.GetRedPlayer());
        board.GetEdge(TestEdge.E4).BuildRoad(board.GetRedPlayer());

        var phase = GamePlayHelpers.GetNextPhase(board.GetGameState());

        Assert.True(IsNextPhaseAsExpected(phase, GameStates.RollOrUseDevCard, board.GetRedPlayer(), board.GetBluePlayer()));
    }

    [Fact]
    public void GetNextPhase_RollOrUseDevCard_NoRollStayPut()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.RollOrUseDevCard, board.GetRedPlayer(), board.GetBluePlayer());
        board.GetGameState().Phase.SetWaitingForRoll();

        var phase = GamePlayHelpers.GetNextPhase(board.GetGameState());

        Assert.True(IsNextPhaseAsExpected(phase, GameStates.RollOrUseDevCard, board.GetRedPlayer(), board.GetBluePlayer()));
    }

    [Fact]
    public void GetNextPhase_RollOrUseDevCard_MoveToBuildOrTrade()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.RollOrUseDevCard, board.GetRedPlayer(), board.GetBluePlayer());
        board.GetGameState().Phase.SetWaitingForRoll();

        do
            GamePlayHelpers.RollDice(board.GetGameState(), true);
        while (board.GetGameState().Dice.GetCombinedValue() == 7);

        var phase = GamePlayHelpers.GetNextPhase(board.GetGameState());

        Assert.True(IsNextPhaseAsExpected(phase, GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer()));
    }

    [Fact]
    public void GetNextPhase_RollOrUseDevCard_MoveToPlaceRobber()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.RollOrUseDevCard, board.GetRedPlayer(), board.GetBluePlayer());
        board.GetGameState().Phase.SetWaitingForRoll();

        do
            GamePlayHelpers.RollDice(board.GetGameState(), true);
        while (board.GetGameState().Dice.GetCombinedValue() != 7);

        var phase = GamePlayHelpers.GetNextPhase(board.GetGameState());

        Assert.True(IsNextPhaseAsExpected(phase, GameStates.PlaceRobber, board.GetRedPlayer(), board.GetBluePlayer()));
        Assert.Equal(GameStates.BuildOrTrade, phase.PreviousState);
    }

    [Fact]
    public void GetNextPhase_PlaceRobber_RobberNotMovedStayPut()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceRobber, board.GetRedPlayer(), board.GetBluePlayer());
        board.GetGameState().Phase.SetStateToReturnTo(GameStates.RollOrUseDevCard, board.GetGameState().RobberTile);

        var phase = GamePlayHelpers.GetNextPhase(board.GetGameState());

        Assert.True(IsNextPhaseAsExpected(phase, GameStates.PlaceRobber, board.GetRedPlayer(), board.GetBluePlayer()));
        Assert.Equal(GameStates.RollOrUseDevCard, board.GetGameState().Phase.PreviousState);
    }

    [Fact]
    public void GetNextPhase_PlaceRobber_MoveToRollOrUseDevCard()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceRobber, board.GetRedPlayer(), board.GetBluePlayer());
        board.GetGameState().Phase.SetStateToReturnTo(GameStates.RollOrUseDevCard, board.GetGameState().RobberTile);
        board.GetGameState().SetRobberTile(board.GetTile(TestTile.T5));

        var phase = GamePlayHelpers.GetNextPhase(board.GetGameState());
        board.GetGameState().Phase.ClearRobberState();

        Assert.True(IsNextPhaseAsExpected(phase, GameStates.RollOrUseDevCard, board.GetRedPlayer(), board.GetBluePlayer()));
        Assert.Null(board.GetGameState().Phase.PreviousState);
        Assert.Null(board.GetGameState().Phase.OriginalRobberTile);
        Assert.Equal(board.GetTile(TestTile.T5).Id, board.GetGameState().RobberTile.Id);
    }

    [Fact]
    public void GetNextPhase_PlaceRobber_MoveToBuildOrTrade()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceRobber, board.GetRedPlayer(), board.GetBluePlayer());
        board.GetGameState().Phase.SetStateToReturnTo(GameStates.BuildOrTrade, board.GetGameState().RobberTile);
        board.GetGameState().SetRobberTile(board.GetTile(TestTile.T5));

        var phase = GamePlayHelpers.GetNextPhase(board.GetGameState());
        board.GetGameState().Phase.ClearRobberState();

        Assert.True(IsNextPhaseAsExpected(phase, GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer()));
        Assert.Null(board.GetGameState().Phase.PreviousState);
        Assert.Null(board.GetGameState().Phase.OriginalRobberTile);
        Assert.Equal(board.GetTile(TestTile.T5).Id, board.GetGameState().RobberTile.Id);
    }

    private TestGameBoard CreateGameStateForRoadBuildingPhaseTesting(GameStates startingState)
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(startingState, board.GetRedPlayer(), board.GetBluePlayer());
        board.GetEdge(TestEdge.E2).BuildRoad(board.GetRedPlayer());
        board.GetEdge(TestEdge.E17).BuildRoad(board.GetRedPlayer());
        board.GetRedPlayer().AssignDevelopmentCard(DevelopmentCardType.RoadBuilding);
        board.GetRedPlayer().MakeNewDevelopmentCardsPlayable();
        GamePlayHelpers.PlayRoadBuildingDevCard(board.GetGameState(), board.GetRedPlayer());

        return board;
    }

    // TODO: This is really testing that GetNextPhase() stays as the phase transition
    // is being done by the call to GamePlayHelpers.PlayRoadBuildingDevCard().
    // Need to review and make sure this makes sense. If yes, should change to 
    // a PlayRoadBuildingDevCard test or integration test.
    [Fact]
    public void GetNextPhase_BuildOrTrade_MoveToFirstDevCardRoad()
    {
        // Arrange
        var board = CreateGameStateForRoadBuildingPhaseTesting(GameStates.BuildOrTrade);

        // Act
        var phase = GamePlayHelpers.GetNextPhase(board.GetGameState());

        // Assert
        Assert.Equal(GameStates.FirstDevCardRoad, phase.PhaseState);
        Assert.Equal(GameStates.BuildOrTrade, phase.PreviousState);
        Assert.Equal(2, phase.RoadsPreRoadBuilding);
    }

    // TODO: This is really testing that GetNextPhase() stays as the phase transition
    // is being done by the call to GamePlayHelpers.PlayRoadBuildingDevCard().
    // Need to review and make sure this makes sense. If yes, should change to 
    // a PlayRoadBuildingDevCard test or integration test.
    [Fact]
    public void GetNextPhase_RollOrUseDevCard_MoveToFirstDevCardRoad()
    {
        // Arrange
        var board = CreateGameStateForRoadBuildingPhaseTesting(GameStates.RollOrUseDevCard);

        // Act
        var phase = GamePlayHelpers.GetNextPhase(board.GetGameState());

        // Assert
        Assert.Equal(GameStates.FirstDevCardRoad, phase.PhaseState);
        Assert.Equal(GameStates.RollOrUseDevCard, phase.PreviousState);
        Assert.Equal(2, phase.RoadsPreRoadBuilding);
    }

    [Fact]
    public void GetNextPhase_FirstDevCardRoad_MoveToSecondDevCardRoad()
    {
        // Arrange
        var board = CreateGameStateForRoadBuildingPhaseTesting(GameStates.RollOrUseDevCard);
        GamePlayHelpers.BuildRoad(board.GetGameState(), board.GetRedPlayer(), board.GetEdge(TestEdge.E16));

        // Act
        var phase = GamePlayHelpers.GetNextPhase(board.GetGameState());

        Assert.Equal(GameStates.SecondDevCardRoad, phase.PhaseState);
        Assert.Equal(GameStates.RollOrUseDevCard, phase.PreviousState);
        Assert.Equal(2, phase.RoadsPreRoadBuilding);
    }

    [Fact]
    public void GetNextPhase_SecondDevCardRoad_StayBecauseConditionsNotSatisfied()
    {
        // Arrange
        var board = CreateGameStateForRoadBuildingPhaseTesting(GameStates.RollOrUseDevCard);
        GamePlayHelpers.BuildRoad(board.GetGameState(), board.GetRedPlayer(), board.GetEdge(TestEdge.E16));
        GamePlayHelpers.GameLoop(board.GetGameState());
        Assert.Equal(GameStates.SecondDevCardRoad, board.GetGameState().Phase.PhaseState);

        // Act
        var phase = GamePlayHelpers.GetNextPhase(board.GetGameState());

        Assert.Equal(GameStates.SecondDevCardRoad, phase.PhaseState);
        Assert.Equal(GameStates.RollOrUseDevCard, phase.PreviousState);
        Assert.Equal(2, phase.RoadsPreRoadBuilding);
    }

    [Fact]
    public void GetNextPhase_SecondDevCardRoad_MoveToBuildOrTrade()
    {
        var board = CreateGameStateForRoadBuildingPhaseTesting(GameStates.BuildOrTrade);
        GamePlayHelpers.BuildRoad(board.GetGameState(), board.GetRedPlayer(), board.GetEdge(TestEdge.E16));
        GamePlayHelpers.GameLoop(board.GetGameState());
        Assert.Equal(GameStates.SecondDevCardRoad, board.GetGameState().Phase.PhaseState);
        GamePlayHelpers.BuildRoad(board.GetGameState(), board.GetRedPlayer(), board.GetEdge(TestEdge.E18));

        // Act
        var phase = GamePlayHelpers.GetNextPhase(board.GetGameState());

        Assert.Equal(GameStates.BuildOrTrade, phase.PhaseState);
        Assert.Null(phase.PreviousState);
        Assert.Null(phase.RoadsPreRoadBuilding);
    }

    [Fact]
    public void GetNextPhase_SecondDevCardRoad_MoveToRollOrUseDevCard()
    {
        var board = CreateGameStateForRoadBuildingPhaseTesting(GameStates.RollOrUseDevCard);
        GamePlayHelpers.BuildRoad(board.GetGameState(), board.GetRedPlayer(), board.GetEdge(TestEdge.E16));
        GamePlayHelpers.GameLoop(board.GetGameState());
        Assert.Equal(GameStates.SecondDevCardRoad, board.GetGameState().Phase.PhaseState);
        GamePlayHelpers.BuildRoad(board.GetGameState(), board.GetRedPlayer(), board.GetEdge(TestEdge.E18));

        // Act
        var phase = GamePlayHelpers.GetNextPhase(board.GetGameState());

        Assert.Equal(GameStates.RollOrUseDevCard, phase.PhaseState);
        Assert.Null(phase.PreviousState);
        Assert.Null(phase.RoadsPreRoadBuilding);
    }

    [Fact]
    public void GetNextPhase_BuildOrTrade_MoveToGameOverWin()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());
        BuildTestCities(board, 4);
        BuildTestSettlements(board, 2);

        var phase = GamePlayHelpers.GetNextPhase(board.GetGameState());

        Assert.True(IsNextPhaseAsExpected(phase, GameStates.GameOver, board.GetRedPlayer(), board.GetBluePlayer()));
    }

    // TODO: Seems like moving from any state to GameOver due to the resignation
    // of all other players would be from explicit calls from the users vs
    // something that would happen due to a call to GetNextPhase.

    [Fact]
    public void EndTurn_MovesToNextPlayer()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());
        GamePlayHelpers.EndTurn(board.GetRedPlayer(), board.GetGameState());

        Assert.True(IsNextPhaseAsExpected(board.GetGameState().Phase, GameStates.RollOrUseDevCard, board.GetBluePlayer(), board.GetBluePlayer()));
    }

    [Fact]
    public void EndTurn_ExceptionIfNoCurrentPlayer()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        Assert.Null(board.GetGameState().Phase.CurrentPlayer);

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.EndTurn(board.GetRedPlayer(), board.GetGameState()));
    }

    [Fact]
    public void EndTurn_ExceptionIfNotPlayersTurn()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.EndTurn(board.GetBluePlayer(), board.GetGameState()));
    }

    [Fact]
    public void EndTurn_ExceptionIfNotBuildOrTradePhase()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.RollOrUseDevCard, board.GetRedPlayer(), board.GetBluePlayer());

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.EndTurn(board.GetRedPlayer(), board.GetGameState()));
    }

    [Fact]
    public void BuildRoad_MissingPlayer()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.BuildOrTrade, board.GetBluePlayer(), board.GetBluePlayer());

        var response = GamePlayHelpers.BuildRoadRequestFromUser(board.GetGameState(), "PP1", board.GetEdge(TestEdge.E2).Id);

        Assert.False(response.Success);
        Assert.Equal(1012, response.ErrorCode);
    }

    [Fact]
    public void BuildRoad_NotPlayersTurn()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.BuildOrTrade, board.GetBluePlayer(), board.GetBluePlayer());

        var response = GamePlayHelpers.BuildRoadRequestFromUser(board.GetGameState(), board.GetRedPlayer().Id, board.GetEdge(TestEdge.E2).Id);

        Assert.False(response.Success);
        Assert.Equal(1011, response.ErrorCode);
    }

    [Fact]
    public void BuildRoad_FailIfWrongPhase()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.RollOrUseDevCard, board.GetRedPlayer(), board.GetBluePlayer());

        var response = GamePlayHelpers.BuildRoadRequestFromUser(board.GetGameState(), board.GetRedPlayer().Id, board.GetEdge(TestEdge.E2).Id);

        Assert.False(response.Success);
        Assert.Equal(1003, response.ErrorCode);
    }

    [Fact]
    public void BuildRoad_BuildOrTradePhase_BuildsRoadNoPhaseChange()
    {
        // Arrange
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());
        board.GetVertex(TestVertex.V2).BuildSettlement(board.GetRedPlayer());

        board.GetRedPlayer().Resources[ResourceType.Wood] = 1;
        board.GetRedPlayer().Resources[ResourceType.Brick] = 1;

        // Act
        var response = GamePlayHelpers.BuildRoadRequestFromUser(board.GetGameState(), board.GetRedPlayer().Id, board.GetEdge(TestEdge.E2).Id);

        // Assert
        Assert.True(response.Success);
        var e2 = board.GetEdge(TestEdge.E2);
        Assert.NotNull(e2.Owner);
        Assert.Equal(board.GetRedPlayer().Id, e2.Owner.Id);
    }

    [Fact]
    public void BuildRoad_PlaceFirstRoadPhase_BuildsRoadAndPhaseChange()
    {
        // Arrange
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceFirstRoad, board.GetBluePlayer(), board.GetBluePlayer());
        board.GetVertex(TestVertex.V2).BuildSettlement(board.GetBluePlayer());

        // Act
        var response = GamePlayHelpers.BuildRoadRequestFromUser(board.GetGameState(), board.GetBluePlayer().Id, board.GetEdge(TestEdge.E2).Id);

        // Assert
        Assert.True(response.Success);
        var e2 = board.GetEdge(TestEdge.E2);
        Assert.NotNull(e2.Owner);
        Assert.Equal(board.GetBluePlayer().Id, e2.Owner.Id);
        Assert.True(IsNextPhaseAsExpected(board.GetGameState().Phase, GameStates.PlaceSecondSettlement, board.GetBluePlayer()));
    }

    [Fact]
    public void BuildRoad_NotAdjacentToBuilding()
    {
        // Arrange
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceFirstRoad, board.GetBluePlayer(), board.GetBluePlayer());
        board.GetVertex(TestVertex.V3).BuildSettlement(board.GetBluePlayer());

        // Act - Build road on non-adjacent edge.
        var response = GamePlayHelpers.BuildRoadRequestFromUser(board.GetGameState(), board.GetBluePlayer().Id, board.GetEdge(TestEdge.E1).Id);

        // Assert
        Assert.False(response.Success);
        Assert.Equal(1015, response.ErrorCode);
    }

    [Fact]
    public void BuildRoad_NotAdjacentToBuildingOfRightPlayer()
    {
        // Arrange
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceFirstRoad, board.GetBluePlayer(), board.GetBluePlayer());
        board.GetVertex(TestVertex.V3).BuildSettlement(board.GetRedPlayer());

        // Act - Try to build blue road next to red vertex
        var response = GamePlayHelpers.BuildRoadRequestFromUser(board.GetGameState(), board.GetBluePlayer().Id, board.GetEdge(TestEdge.E2).Id);

        // Assert
        Assert.False(response.Success);
        Assert.Equal(1015, response.ErrorCode);
    }

    [Fact]
    public void BuildSettlement_MissingPlayer()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.BuildOrTrade, board.GetBluePlayer(), board.GetBluePlayer());

        var response = GamePlayHelpers.BuildSettlementRequestFromUser(board.GetGameState(), "PP1", board.GetVertex(TestVertex.V2).Id);

        Assert.False(response.Success);
        Assert.Equal(1012, response.ErrorCode);
    }

    [Fact]
    public void BuildSettlement_NotPlayersTurn()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.BuildOrTrade, board.GetBluePlayer(), board.GetBluePlayer());

        var response = GamePlayHelpers.BuildSettlementRequestFromUser(board.GetGameState(), board.GetRedPlayer().Id, board.GetVertex(TestVertex.V5).Id);

        Assert.False(response.Success);
        Assert.Equal(1011, response.ErrorCode);
    }

    // TODO: Need to add additional BuildSettlement tests.

    [Fact]
    public void BuildSettlement_FailIfWrongPhase()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.RollOrUseDevCard, board.GetRedPlayer(), board.GetBluePlayer());

        var response = GamePlayHelpers.BuildSettlementRequestFromUser(board.GetGameState(), board.GetRedPlayer().Id, board.GetVertex(TestVertex.V5).Id);

        Assert.False(response.Success);
        Assert.Equal(1003, response.ErrorCode);
    }

    [Fact]
    public void BuildSettlement_BuildOrTradePhase_BuildsSettlementNoPhaseChange()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.BuildOrTrade, board.GetRedPlayer(), board.GetBluePlayer());
        board.GetEdge(TestEdge.E8).BuildRoad(board.GetRedPlayer());

        var vertex = board.GetVertex(TestVertex.V2);
        board.GetRedPlayer().Resources[ResourceType.Brick] = 1;
        board.GetRedPlayer().Resources[ResourceType.Wood] = 1;
        board.GetRedPlayer().Resources[ResourceType.Wool] = 1;
        board.GetRedPlayer().Resources[ResourceType.Grain] = 1;

        // Act
        var response = GamePlayHelpers.BuildSettlementRequestFromUser(board.GetGameState(), board.GetRedPlayer().Id, vertex.Id);

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(vertex.Owner);
        Assert.NotNull(vertex.Building);
        Assert.Equal(board.GetRedPlayer().Id, vertex.Owner.Id);
        Assert.Equal(BuildingType.Settlement, vertex.Building);
    }

    [Fact]
    public void BuildRoad_PlaceFirstSettlementPhase_BuildsSettlementAndPhaseChange()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceFirstSettlement, board.GetRedPlayer(), board.GetBluePlayer());

        var response = GamePlayHelpers.BuildSettlementRequestFromUser(gs, board.GetRedPlayer().Id, board.GetVertex(TestVertex.V5).Id);

        Assert.True(response.Success);
        Assert.NotNull(board.GetVertex(TestVertex.V5).Building);
        Assert.Equal(BuildingType.Settlement, board.GetVertex(TestVertex.V5).Building);
        Assert.NotNull(board.GetVertex(TestVertex.V5).Owner);
        Assert.Equal(board.GetRedPlayer().Id, board.GetVertex(TestVertex.V5).Owner.Id);
        Assert.Equal(GameStates.PlaceFirstRoad, gs.Phase.PhaseState);
        Assert.NotNull(gs.Phase.CurrentPlayer);
        Assert.Equal(gs.Phase.CurrentPlayer.Id, board.GetRedPlayer().Id);
    }

    [Fact]
    public void GameLoop_DropOutIfNotBotsTurn()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceFirstSettlement, board.GetRedPlayer(), board.GetBluePlayer());

        GamePlayHelpers.GameLoop(board.GetGameState());

        Assert.NotNull(board.GetGameState().Phase.CurrentPlayer);
        Assert.False(board.GetGameState().Phase.CurrentPlayer.IsBot);
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
        GamePlayHelpers.GameLoop(gs);

        // Assert
        Assert.Equal(botTile.Id, gs.RobberTile.Id);
        Assert.Null(gs.Phase.PreviousState);
        Assert.Equal(GameStates.BuildOrTrade, gs.Phase.PhaseState);
        Assert.Contains(ResourceType.Ore, human.Resources);
        Assert.Equal(1, human.Resources[ResourceType.Ore]);
        Assert.Contains(ResourceType.Ore, bot.Resources);
        Assert.Equal(1, bot.Resources[ResourceType.Ore]);
    }

    private static GameState CreateGameForBuyDevCardTesting(GameStates currentState)
    {
        var gs = new GameState(new Guid());
        var player1 = new Player("Tim", PlayerColor.Red);
        gs.Players.Add(player1);
        player1.AssignResources(ResourceType.Ore, 1);
        player1.AssignResources(ResourceType.Grain, 2);
        player1.AssignResources(ResourceType.Wool, 1);
        var player2 = new Player("Mary", PlayerColor.Blue, true);
        gs.Players.Add(player2);

        var tile1 = new Tile(ResourceType.Wood, 3, 0, 0);
        gs.Tiles.Add(tile1);
        var tile2 = new Tile(ResourceType.Brick, 9, 2, 0);
        gs.Tiles.Add(tile2);

        gs.Phase.CurrentPlayer = player1;
        gs.Phase.PhaseState = currentState;

        return gs;        
    }


    [Fact]
    public void BuyDevCardFromUser_NotCurrentPlayer()
    {
        var gs = CreateGameForBuyDevCardTesting(GameStates.BuildOrTrade);
        var bot = gs.Players.First(p => p.IsBot);
        var human = gs.Players.First(p => !p.IsBot);
        Assert.Equal(human.Id, gs.Phase.CurrentPlayer.Id);
        Assert.Empty(bot.DevCardsPurchasedThisRound);
        Assert.Equal(0, bot.DevelopmentCardCount);

        var response = GamePlayHelpers.BuyDevCardFromUser(gs, bot.Id);

        Assert.False(response.Success);
        Assert.Equal(1011, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void BuyDevCardFromUser_WrongState()
    {
        var gs = CreateGameForBuyDevCardTesting(GameStates.RollOrUseDevCard);
        var human = gs.Players.First(p => !p.IsBot);
        Assert.Empty(human.DevCardsPurchasedThisRound);
        Assert.Equal(0, human.DevelopmentCardCount);

        var response = GamePlayHelpers.BuyDevCardFromUser(gs, human.Id);

        Assert.False(response.Success);
        Assert.Equal(1003, response.ErrorCode);
        Assert.Null(response.GameState);
        Assert.Empty(human.DevCardsPurchasedThisRound);
        Assert.Equal(0, human.DevelopmentCardCount);
    }

    [Fact]
    public void BuyDevCardFromUser_InsufficientResources()
    {
        var gs = CreateGameForBuyDevCardTesting(GameStates.BuildOrTrade);
        var human = gs.Players.First(p => !p.IsBot);
        human.RemoveResources(ResourceType.Wool, 1); // Take away all wool.
        Assert.Empty(human.DevCardsPurchasedThisRound);
        Assert.Equal(0, human.DevelopmentCardCount);

        var response = GamePlayHelpers.BuyDevCardFromUser(gs, human.Id);

        Assert.False(response.Success);
        Assert.Equal(1017, response.ErrorCode);
        Assert.Null(response.GameState);
        Assert.Empty(human.DevCardsPurchasedThisRound);
        Assert.Equal(0, human.DevelopmentCardCount);
    }

    [Fact]
    public void BuyDevCardFromUser_Valid()
    {
        var gs = CreateGameForBuyDevCardTesting(GameStates.BuildOrTrade);
        var human = gs.Players.First(p => !p.IsBot);
        Assert.Empty(human.DevCardsPurchasedThisRound);
        Assert.Equal(0, human.DevelopmentCardCount);
        var grainCount = human.Resources[ResourceType.Grain];
        var woolCount = human.Resources[ResourceType.Wool];
        var oreCount = human.Resources[ResourceType.Ore];

        var response = GamePlayHelpers.BuyDevCardFromUser(gs, human.Id);

        Assert.True(response.Success);
        Assert.Equal(0, response.ErrorCode);
        Assert.NotNull(response.GameState);
        Assert.Single(human.DevCardsPurchasedThisRound);
        Assert.Equal(1, human.DevelopmentCardCount);
        Assert.Empty(human.DevCardsPlayed);
        Assert.Empty(human.DevCardsReadyToPlay);
        Assert.Equal(grainCount - 1, human.Resources[ResourceType.Grain]);
        Assert.Equal(woolCount - 1, human.Resources[ResourceType.Wool]);
        Assert.Equal(oreCount - 1, human.Resources[ResourceType.Ore]);
    }

    [Fact]
    public void BuyDevCard_InvalidState()
    {
        var gs = CreateGameForBuyDevCardTesting(GameStates.RollOrUseDevCard);
        var human = gs.Players.First(p => !p.IsBot);

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.BuyDevCard(gs, human));
    }

    [Fact]
    public void BuyDevCard_NotCurrentUser()
    {
        var gs = CreateGameForBuyDevCardTesting(GameStates.BuildOrTrade);
        var bot = gs.Players.First(p => p.IsBot);

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.BuyDevCard(gs, bot));
    }

    [Fact]
    public void BuyDevCard_InsufficientResources()
    {
        var gs = CreateGameForBuyDevCardTesting(GameStates.BuildOrTrade);
        var human = gs.Players.First(p => !p.IsBot);
        human.RemoveResources(ResourceType.Grain, 2); // remove all grain

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.BuyDevCard(gs, human));
    }

    [Fact]
    public void BuyDevCard_Valid()
    {
        var gs = CreateGameForBuyDevCardTesting(GameStates.BuildOrTrade);
        var human = gs.Players.First(p => !p.IsBot);
        var grainCount = human.Resources[ResourceType.Grain];
        var woolCount = human.Resources[ResourceType.Wool];
        var oreCount = human.Resources[ResourceType.Ore];

        GamePlayHelpers.BuyDevCard(gs, human);

        Assert.Single(human.DevCardsPurchasedThisRound);
        Assert.Equal(1, human.DevelopmentCardCount);
        Assert.Empty(human.DevCardsPlayed);
        Assert.Empty(human.DevCardsReadyToPlay);
        Assert.Equal(grainCount - 1, human.Resources[ResourceType.Grain]);
        Assert.Equal(woolCount - 1, human.Resources[ResourceType.Wool]);
        Assert.Equal(oreCount - 1, human.Resources[ResourceType.Ore]);
    }

    [Fact]
    public void EndTurn_MovesDevCardsToReadyToPlayState()
    {
        // Arrange
        var gs = CreateGameForBuyDevCardTesting(GameStates.BuildOrTrade);
        var human = gs.Players.First(p => !p.IsBot);

        GamePlayHelpers.BuyDevCard(gs, human);
        Assert.Single(human.DevCardsPurchasedThisRound);
        Assert.Equal(1, human.DevelopmentCardCount);

        // Act
        GamePlayHelpers.EndTurn(human, gs, true);

        // Assert
        Assert.Empty(human.DevCardsPurchasedThisRound);
        Assert.Single(human.DevCardsReadyToPlay);
        Assert.Equal(1, human.DevelopmentCardCount);
    }

    private static GameState CreateGameForPlayDevCardTesting(GameStates currentState, DevelopmentCardType desiredType)
    {
        var gs = new GameState(new Guid());
        var player1 = new Player("Tim", PlayerColor.Red);
        gs.Players.Add(player1);
        var player2 = new Player("Mary", PlayerColor.Blue, true);
        gs.Players.Add(player2);
        var tile1 = new Tile(ResourceType.Wood, 3, 0, 0);
        gs.Tiles.Add(tile1);
        var tile2 = new Tile(ResourceType.Brick, 9, 2, 0);
        gs.Tiles.Add(tile2);
        gs.SetRobberTile(tile1);
        gs.Phase.CurrentPlayer = player1;

        // Assign some resources so that we can test monopoly
        player1.AssignResources(ResourceType.Wood, 1);
        player1.AssignResources(ResourceType.Brick, 2);
        player2.AssignResources(ResourceType.Wood, 2);
        player2.AssignResources(ResourceType.Ore, 1);

        // Build one settlement for each player so that resources and be stolen when knight placed
        BoardCreationHelpers.CreateEdgesAndVerticesForBoard(gs);
        BoardCreationHelpers.LinkEdgesAndVertices(gs);
        var v1 = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, tile1, null, null, VertexDirection.SW);
        v1.BuildSettlement(player1);
        var v2 = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, tile2, null, null, VertexDirection.NE);
        v2.BuildSettlement(player2);
        GamePlayHelpers.MarkBlockedVertices(gs);

        // Customize test board for the test being done
        player1.AssignDevelopmentCard(desiredType);
        player1.MakeNewDevelopmentCardsPlayable();
        gs.Phase.PhaseState = currentState;

        return gs;        
    }

    [Fact]
    public void PlayMonopolyDevCardFromUser_InvalidState()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.PlaceRobber, DevelopmentCardType.Monopoly);
        var human = gs.Players.First(p => !p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.Monopoly.ToString(), new List<string>() { ResourceType.Wood.ToString()}, null);

        var response = GamePlayHelpers.PlayMonopolyDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1003, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayMonopolyDevCardFromUser_NotPlayersTurn()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.RollOrUseDevCard, DevelopmentCardType.Monopoly);
        var bot = gs.Players.First(p => p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(bot.Id, DevelopmentCardType.Monopoly.ToString(), new List<string>() { ResourceType.Wood.ToString()}, null);

        var response = GamePlayHelpers.PlayMonopolyDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1011, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayMonopolyDevCardFromUser_NoResourceRequested()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.RollOrUseDevCard, DevelopmentCardType.Monopoly);
        var human = gs.Players.First(p => !p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.Monopoly.ToString(), new List<string>(), null);

        var response = GamePlayHelpers.PlayMonopolyDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1037, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayMonopolyDevCardFromUser_NullResourceRequested()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.RollOrUseDevCard, DevelopmentCardType.Monopoly);
        var human = gs.Players.First(p => !p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.Monopoly.ToString(), null, null);

        var response = GamePlayHelpers.PlayMonopolyDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1037, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayMonopolyDevCardFromUser_MultipleResourcesRequested()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.RollOrUseDevCard, DevelopmentCardType.Monopoly);
        var human = gs.Players.First(p => !p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.Monopoly.ToString(), 
            new List<string>()  { ResourceType.Wood.ToString(), ResourceType.Brick.ToString()}, null);

        var response = GamePlayHelpers.PlayMonopolyDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1037, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayMonopolyDevCardFromUser_RequestDesert()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.RollOrUseDevCard, DevelopmentCardType.Monopoly);
        var human = gs.Players.First(p => !p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.Monopoly.ToString(), 
            new List<string>()  { ResourceType.Desert.ToString()}, null);

        var response = GamePlayHelpers.PlayMonopolyDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1038, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayMonopolyDevCardFromUser_RequestInvalidResource()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.RollOrUseDevCard, DevelopmentCardType.Monopoly);
        var human = gs.Players.First(p => !p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.Monopoly.ToString(), 
            new List<string>()  { "Apple"}, null);

        var response = GamePlayHelpers.PlayMonopolyDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1038, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayMonopolyDevCardFromUser_NoMonopolyCard()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.RollOrUseDevCard, DevelopmentCardType.YearOfPlenty);
        var human = gs.Players.First(p => !p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.Monopoly.ToString(), 
            new List<string>()  { ResourceType.Wood.ToString() }, null);

        var response = GamePlayHelpers.PlayMonopolyDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1039, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayMonopolyDevCardFromUser_Valid()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.RollOrUseDevCard, DevelopmentCardType.Monopoly);
        var human = gs.Players.First(p => !p.IsBot);
        var bot = gs.Players.First(p => p.IsBot);
        var monopolyCount = human.DevCardsReadyToPlay.Count(d => d == DevelopmentCardType.Monopoly);
        var woodCount = human.Resources[ResourceType.Wood];
        Assert.True(woodCount > 0);
        var botWoodCount = bot.Resources[ResourceType.Wood];
        Assert.True(botWoodCount > 0);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.Monopoly.ToString(), 
            new List<string>()  { ResourceType.Wood.ToString() }, null);

        var response = GamePlayHelpers.PlayMonopolyDevCardFromUser(gs, request);

        Assert.True(response.Success);
        Assert.NotNull(response.GameState);
        Assert.Equal(monopolyCount - 1, human.DevCardsReadyToPlay.Count(d => d == DevelopmentCardType.Monopoly));
        Assert.Empty(human.DevCardsPurchasedThisRound);
        Assert.Empty(human.DevCardsPlayed);
        Assert.Equal(woodCount + botWoodCount, human.Resources[ResourceType.Wood]);
        Assert.Equal(0, bot.Resources[ResourceType.Wood]);
    }

    [Fact]
    public void PlayMonopolyDevCard_InvalidState()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.SettingUpBoard, DevelopmentCardType.Monopoly);
        var human = gs.Players.First(p => !p.IsBot);

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.PlayMonopolyDevCard(gs, human, ResourceType.Wood));
    }

    [Fact]
    public void PlayMonopolyDevCard_NotPlayersTurn()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.RollOrUseDevCard, DevelopmentCardType.Monopoly);
        var bot = gs.Players.First(p => p.IsBot);

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.PlayMonopolyDevCard(gs, bot, ResourceType.Wood));
    }

    [Fact]
    public void PlayMonopolyDevCard_PlayerDoesntHaveDevCard()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.RollOrUseDevCard, DevelopmentCardType.RoadBuilding);
        var human = gs.Players.First(p => !p.IsBot);

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.PlayMonopolyDevCard(gs, human, ResourceType.Wood));
    }

    [Fact]
    public void PlayMonopolyDevCard_RequestDesert()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.RollOrUseDevCard, DevelopmentCardType.Monopoly);
        var human = gs.Players.First(p => !p.IsBot);

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.PlayMonopolyDevCard(gs, human, ResourceType.Desert));
    }

    [Fact]
    public void PlayMonopolyDevCard_Valid()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.Monopoly);
        var human = gs.Players.First(p => !p.IsBot);
        var bot = gs.Players.First(p => p.IsBot);
        var monopolyCount = human.DevCardsReadyToPlay.Count(d => d == DevelopmentCardType.Monopoly);
        var woodCount = human.Resources[ResourceType.Wood];
        Assert.True(woodCount > 0);
        var botWoodCount = bot.Resources[ResourceType.Wood];
        Assert.True(botWoodCount > 0);

        GamePlayHelpers.PlayMonopolyDevCard(gs, human, ResourceType.Wood);

        Assert.Equal(monopolyCount - 1, human.DevCardsReadyToPlay.Count(d => d == DevelopmentCardType.Monopoly));
        Assert.Empty(human.DevCardsPurchasedThisRound);
        Assert.Empty(human.DevCardsPlayed);
        Assert.Equal(woodCount + botWoodCount, human.Resources[ResourceType.Wood]);
        Assert.Equal(0, bot.Resources[ResourceType.Wood]);
    }

    [Fact]
    public void PlayYearOfPlentyDevCardFromUser_InvalidState()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.PlaceSecondSettlement, DevelopmentCardType.YearOfPlenty);
        var human = gs.Players.First(p => !p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.YearOfPlenty.ToString(), 
            new List<string>()  { ResourceType.Wood.ToString(), ResourceType.Brick.ToString() }, null);

        var response = GamePlayHelpers.PlayYearOfPlentyDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1003, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayYearOfPlentyDevCardFromUser_NotPlayersTurn()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.YearOfPlenty);
        var bot = gs.Players.First(p => p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(bot.Id, DevelopmentCardType.YearOfPlenty.ToString(), 
            new List<string>()  { ResourceType.Wood.ToString(), ResourceType.Brick.ToString() }, null);

        var response = GamePlayHelpers.PlayYearOfPlentyDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1011, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayYearOfPlentyDevCardFromUser_TooFewResourcesSelected()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.YearOfPlenty);
        var human = gs.Players.First(p => !p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.YearOfPlenty.ToString(), 
            new List<string>()  { ResourceType.Wood.ToString() }, null);

        var response = GamePlayHelpers.PlayYearOfPlentyDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1040, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayYearOfPlentyDevCardFromUser_NoResourcesSelected()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.YearOfPlenty);
        var human = gs.Players.First(p => !p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.YearOfPlenty.ToString(), 
            new List<string>(), null);

        var response = GamePlayHelpers.PlayYearOfPlentyDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1040, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayYearOfPlentyDevCardFromUser_ResourcesSelectedNull()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.YearOfPlenty);
        var human = gs.Players.First(p => !p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.YearOfPlenty.ToString(), 
            null, null);

        var response = GamePlayHelpers.PlayYearOfPlentyDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1040, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayYearOfPlentyDevCardFromUser_TooManyResourcesSelected()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.YearOfPlenty);
        var human = gs.Players.First(p => !p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.YearOfPlenty.ToString(), 
            new List<string>()  { ResourceType.Wood.ToString(), ResourceType.Brick.ToString(), ResourceType.Brick.ToString() }, null);

        var response = GamePlayHelpers.PlayYearOfPlentyDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1040, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayYearOfPlentyDevCardFromUser_InvalidFirstResourcesSelected()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.YearOfPlenty);
        var human = gs.Players.First(p => !p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.YearOfPlenty.ToString(), 
            new List<string>()  { ResourceType.Desert.ToString(), ResourceType.Wood.ToString() }, null);

        var response = GamePlayHelpers.PlayYearOfPlentyDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1038, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayYearOfPlentyDevCardFromUser_InvalidSecondResourcesSelected()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.YearOfPlenty);
        var human = gs.Players.First(p => !p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.YearOfPlenty.ToString(), 
            new List<string>()  { ResourceType.Wood.ToString(), "Pear" }, null);

        var response = GamePlayHelpers.PlayYearOfPlentyDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1038, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayYearOfPlentyDevCardFromUser_WrongDevCardPlayed()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.YearOfPlenty);
        var human = gs.Players.First(p => !p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.Monopoly.ToString(), 
            new List<string>()  { ResourceType.Wood.ToString(), ResourceType.Brick.ToString() }, null);

        var response = GamePlayHelpers.PlayYearOfPlentyDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(9999, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayYearOfPlentyDevCardFromUser_PlayerDoesntHaveDevCard()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.Monopoly);
        var human = gs.Players.First(p => !p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.YearOfPlenty.ToString(), 
            new List<string>()  { ResourceType.Wood.ToString(), ResourceType.Brick.ToString() }, null);

        var response = GamePlayHelpers.PlayYearOfPlentyDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1039, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayYearOfPlentyDevCardFromUser_Valid()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.YearOfPlenty);
        var human = gs.Players.First(p => !p.IsBot);
        var countWood = human.Resources[ResourceType.Wood];
        var countBrick = human.Resources[ResourceType.Brick];
        var countYearOfPlenty = human.DevCardsReadyToPlay.Count(d => d == DevelopmentCardType.YearOfPlenty);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.YearOfPlenty.ToString(), 
            new List<string>()  { ResourceType.Wood.ToString(), ResourceType.Brick.ToString() }, null);

        var response = GamePlayHelpers.PlayYearOfPlentyDevCardFromUser(gs, request);

        Assert.True(response.Success);
        Assert.Equal(0, response.ErrorCode);
        Assert.NotNull(response.GameState);
        Assert.Equal(countWood + 1, human.Resources[ResourceType.Wood]);
        Assert.Equal(countBrick + 1, human.Resources[ResourceType.Brick]);
        Assert.Equal(countYearOfPlenty - 1, human.DevCardsReadyToPlay.Count(d => d == DevelopmentCardType.YearOfPlenty));
    }

    [Fact]
    public void PlayYearOfPlentyDevCard_TooFewResourcesSelected()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.YearOfPlenty);
        var human = gs.Players.First(p => !p.IsBot);

        var request = new List<ResourceType>() { ResourceType.Wood };

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.PlayYearOfPlentyDevCard(gs, human, request));
    }

    [Fact]
    public void PlayYearOfPlentyDevCard_TooManyResourcesSelected()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.YearOfPlenty);
        var human = gs.Players.First(p => !p.IsBot);

        var request = new List<ResourceType>() { ResourceType.Wood, ResourceType.Wood, ResourceType.Ore };

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.PlayYearOfPlentyDevCard(gs, human, request));
    }

    [Fact]
    public void PlayYearOfPlentyDevCard_NotPlayersTurn()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.YearOfPlenty);
        var bot = gs.Players.First(p => p.IsBot);

        var request = new List<ResourceType>()  { ResourceType.Wood, ResourceType.Brick };

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.PlayYearOfPlentyDevCard(gs, bot, request));
    }

    [Fact]
    public void PlayYearOfPlentyDevCard_Valid()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.YearOfPlenty);
        var human = gs.Players.First(p => !p.IsBot);
        var countWood = human.Resources[ResourceType.Wood];
        var countBrick = human.Resources[ResourceType.Brick];
        var countYearOfPlenty = human.DevCardsReadyToPlay.Count(d => d == DevelopmentCardType.YearOfPlenty);

        var request = new List<ResourceType>()  { ResourceType.Wood, ResourceType.Brick };

        GamePlayHelpers.PlayYearOfPlentyDevCard(gs, human, request);

        Assert.Equal(countWood + 1, human.Resources[ResourceType.Wood]);
        Assert.Equal(countBrick + 1, human.Resources[ResourceType.Brick]);
        Assert.Equal(countYearOfPlenty - 1, human.DevCardsReadyToPlay.Count(d => d == DevelopmentCardType.YearOfPlenty));
    }

    [Fact]
    public void PlayRoadBuildingDevCardFromUser_InvalidState()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.SettingUpBoard, DevelopmentCardType.RoadBuilding);
        var human = gs.Players.First(p => !p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.RoadBuilding.ToString(), null, null);

        var response = GamePlayHelpers.PlayRoadBuildingDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1003, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayRoadBuildingDevCardFromUser_NotPlayersTurn()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.RoadBuilding);
        var bot = gs.Players.First(p => p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(bot.Id, DevelopmentCardType.RoadBuilding.ToString(), null, null);

        var response = GamePlayHelpers.PlayRoadBuildingDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1011, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayRoadBuildingDevCardFromUser_WrongDevCardPlayed()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.RoadBuilding);
        var human = gs.Players.First(p => !p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.YearOfPlenty.ToString(), null, null);

        var response = GamePlayHelpers.PlayRoadBuildingDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(9999, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayRoadBuildingDevCardFromUser_PlayDoesntHaveDevCard()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.Monopoly);
        var human = gs.Players.First(p => !p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.RoadBuilding.ToString(), null, null);

        var response = GamePlayHelpers.PlayRoadBuildingDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1039, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayRoadBuildingDevCardFromUser_Valid()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.RoadBuilding);
        var human = gs.Players.First(p => !p.IsBot);
        var roadBuildingCount = human.DevCardsReadyToPlay.Count(d => d == DevelopmentCardType.RoadBuilding);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.RoadBuilding.ToString(), null, null);

        var response = GamePlayHelpers.PlayRoadBuildingDevCardFromUser(gs, request);

        Assert.True(response.Success);
        Assert.Equal(roadBuildingCount - 1, human.DevCardsReadyToPlay.Count(d => d == DevelopmentCardType.RoadBuilding));
        Assert.Equal(GameStates.FirstDevCardRoad ,gs.Phase.PhaseState);
    }

    [Fact]
    public void PlayRoadBuildingDevCard_InvalidState()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.SettingUpBoard, DevelopmentCardType.RoadBuilding);
        var human = gs.Players.First(p => !p.IsBot);

        Assert.Throws<InvalidOperationException>( () => GamePlayHelpers.PlayRoadBuildingDevCard(gs, human));
    }

    [Fact]
    public void PlayRoadBuildingDevCard_Valid()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.RoadBuilding);
        var human = gs.Players.First(p => !p.IsBot);
        var roadBuildingCount = human.DevCardsReadyToPlay.Count(d => d == DevelopmentCardType.RoadBuilding);

        GamePlayHelpers.PlayRoadBuildingDevCard(gs, human);

        Assert.Equal(roadBuildingCount - 1, human.DevCardsReadyToPlay.Count(d => d == DevelopmentCardType.RoadBuilding));
        Assert.Equal(GameStates.FirstDevCardRoad ,gs.Phase.PhaseState);
    }

    [Fact]
    public void PlayKnightDevCardFromUser_InvalidState()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.PlaceRobber, DevelopmentCardType.Knight);
        var human = gs.Players.First(p => !p.IsBot);
        var t1 = BoardCreationHelpers.GetTileAt(gs.Tiles, 2, 0);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.Knight.ToString(), null, t1.Id);

        var response = GamePlayHelpers.PlayKnightDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1003, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayKnightDevCardFromUser_NotPlayersTurn()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.Knight);
        var bot = gs.Players.First(p => p.IsBot);
        var t1 = BoardCreationHelpers.GetTileAt(gs.Tiles, 2, 0);

        PlayDevCardRequest request = new PlayDevCardRequest(bot.Id, DevelopmentCardType.Knight.ToString(), null, t1.Id);

        var response = GamePlayHelpers.PlayKnightDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1011, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayKnightDevCardFromUser_TileIdMissing()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.Knight);
        var human = gs.Players.First(p => !p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.Knight.ToString(), null, null);

        var response = GamePlayHelpers.PlayKnightDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1041, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayKnightDevCardFromUser_TileIdInvalid()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.Knight);
        var human = gs.Players.First(p => !p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.Knight.ToString(), null, "TT1");

        var response = GamePlayHelpers.PlayKnightDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1032, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayKnightDevCardFromUser_RobberAlreadyOnTile()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.Knight);
        var human = gs.Players.First(p => !p.IsBot);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.Knight.ToString(), null, gs.RobberTile.Id);

        var response = GamePlayHelpers.PlayKnightDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1033, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayKnightDevCardFromUser_PlayerDoesNotHaveKnight()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.Monopoly);
        var human = gs.Players.First(p => !p.IsBot);
        var t1 = BoardCreationHelpers.GetTileAt(gs.Tiles, 2, 0);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.Knight.ToString(), null, t1.Id);

        var response = GamePlayHelpers.PlayKnightDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(1039, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayKnightDevCardFromUser_WrongDevCardPlayed()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.Knight);
        var human = gs.Players.First(p => !p.IsBot);
        var t1 = BoardCreationHelpers.GetTileAt(gs.Tiles, 2, 0);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.YearOfPlenty.ToString(), null, t1.Id);

        var response = GamePlayHelpers.PlayKnightDevCardFromUser(gs, request);

        Assert.False(response.Success);
        Assert.Equal(9999, response.ErrorCode);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void PlayKnightDevCardFromUser_Valid()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.Knight);
        var human = gs.Players.First(p => !p.IsBot);
        var readyKnightCount = human.DevCardsReadyToPlay.Count(d => d == DevelopmentCardType.Knight);
        var playedKnightCount = human.DevCardsPlayed.Count(d => d == DevelopmentCardType.Knight);
        var t1 = BoardCreationHelpers.GetTileAt(gs.Tiles, 2, 0);

        PlayDevCardRequest request = new PlayDevCardRequest(human.Id, DevelopmentCardType.Knight.ToString(), null, t1.Id);

        var response = GamePlayHelpers.PlayKnightDevCardFromUser(gs, request);

        Assert.True(response.Success);
        Assert.Equal(readyKnightCount - 1, human.DevCardsReadyToPlay.Count(d => d == DevelopmentCardType.RoadBuilding));
        Assert.Equal(playedKnightCount + 1, human.DevCardsPlayed.Count(d => d == DevelopmentCardType.Knight));
        Assert.Equal(GameStates.BuildOrTrade ,gs.Phase.PhaseState);
    }

    [Fact]
    public void PlayKnightDevCard_NotPlayersTurn()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.Knight);
        var bot = gs.Players.First(p => p.IsBot);
        var t1 = BoardCreationHelpers.GetTileAt(gs.Tiles, 2, 0);

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.PlayKnightDevCard(gs, bot, t1));
    }

    [Fact]
    public void PlayKnightDevCard_RobberAlreadyOnTile()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.Knight);
        var human = gs.Players.First(p => !p.IsBot);
        var t1 = BoardCreationHelpers.GetTileAt(gs.Tiles, 2, 0);

        Assert.Throws<InvalidOperationException>(() => GamePlayHelpers.PlayKnightDevCard(gs, human, gs.RobberTile));
    }

    [Fact]
    public void PlayKnightDevCard_Valid()
    {
        var gs = CreateGameForPlayDevCardTesting(GameStates.BuildOrTrade, DevelopmentCardType.Knight);
        var human = gs.Players.First(p => !p.IsBot);
        var bot = gs.Players.First(p => p.IsBot);
        var readyKnightCount = human.DevCardsReadyToPlay.Count(d => d == DevelopmentCardType.Knight);
        var playedKnightCount = human.DevCardsPlayed.Count(d => d == DevelopmentCardType.Knight);
        var t1 = BoardCreationHelpers.GetTileAt(gs.Tiles, 2, 0);
        var humanResourceCount = human.ResourceCount;
        var botResourceCount = human.ResourceCount;

        GamePlayHelpers.PlayKnightDevCard(gs, human, t1);

        Assert.Equal(readyKnightCount - 1, human.DevCardsReadyToPlay.Count(d => d == DevelopmentCardType.RoadBuilding));
        Assert.Equal(playedKnightCount + 1, human.DevCardsPlayed.Count(d => d == DevelopmentCardType.Knight));
        Assert.Equal(GameStates.BuildOrTrade ,gs.Phase.PhaseState);
        Assert.Equal(humanResourceCount + 1, human.ResourceCount);
        Assert.Equal(botResourceCount - 1, bot.ResourceCount);
    }
}
