using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;
using GameTest.Models;
using GameTest.DTOs;
using GameTest.Services;
using GameTest.Functions;
using Microsoft.VisualStudio.TestPlatform.Common.ExtensionFramework;
using System.Drawing.Printing;
using System.Runtime.CompilerServices;

namespace GameTest.Tests;

public class IntegrationTests
{
    private bool IsTooCloseToAnotherBuilding(Vertex vertex)
    {
        foreach (var edge in vertex.Edges)
            foreach (var linkedVertex in edge.Vertices)
                if (linkedVertex.Id != vertex.Id && GamePlayHelpers.HasBuilding(linkedVertex))
                    return true; 

        return false;
    }

    [Fact]
    public void BotBuildAtEndOfSetupPhase_TriggeredByBuildRoadByUser()
    {
        var board = TestHelpers.CreateOriginalTestBoard(true);
        board.GetGameState().Phase = new GamePhase(GameStates.PlaceFirstSettlement, board.GetBluePlayer(), board.GetRedPlayer());

        // Build Bot's first settlement (triggers bot to build road)
        Assert.True(
            GamePlayHelpers.BuildSettlementRequestFromUser(
                board.GetGameState(), 
                board.GetBluePlayer().Id, 
                board.GetVertex(TestVertex.V5).Id
            ).Success);

        // Build User's settlements and first road
        Assert.True(
            GamePlayHelpers.BuildSettlementRequestFromUser(
                board.GetGameState(), 
                board.GetRedPlayer().Id, 
                board.GetVertex(TestVertex.V3).Id
            ).Success);

        Assert.True(
            GamePlayHelpers.BuildRoadRequestFromUser(
                board.GetGameState(), 
                board.GetRedPlayer().Id, 
                board.GetEdge(TestEdge.E2).Id
            ).Success);

        Assert.True(
            GamePlayHelpers.BuildSettlementRequestFromUser(
                board.GetGameState(), 
                board.GetRedPlayer().Id, 
                board.GetVertex(TestVertex.V14).Id
            ).Success);

        // Act - Build user's second road - this should trigger the bot to build its second settlement and road
        Assert.True(
            GamePlayHelpers.BuildRoadRequestFromUser(
                board.GetGameState(), 
                board.GetRedPlayer().Id, 
                board.GetEdge(TestEdge.E20).Id
            ).Success);

        // Assert
        Assert.True(board.InExpectedState(GameStates.RollOrUseDevCard, board.GetRedPlayer()));

        // TODO: Figure out a way to know the type/count of vertices/edges. Will likely need
        // mock dice implemented.
        Assert.True(board.GetGameState().CountSettlementsForPlayer(board.GetBluePlayer()) >= 2);
        Assert.True(board.GetGameState().CountRoadsForPlayer(board.GetBluePlayer()) >= 2);

        foreach(var vertex in board.GetGameState().Vertices)
            if (GamePlayHelpers.HasBuilding(vertex))
                Assert.False(IsTooCloseToAnotherBuilding(vertex));
        
        board.GetRedPlayer().AssignResources(ResourceType.Wood, 1);
        board.GetRedPlayer().AssignResources(ResourceType.Brick, 1);

        GamePlayHelpers.RollDice(board.GetGameState());
        var possibleActions = GamePlayHelpers.GetPossiblePlayerActions(board.GetGameState(), board.GetGameState().Phase.CurrentPlayer);
        Assert.NotNull(possibleActions);
        Assert.NotEmpty(possibleActions);
        Assert.Contains(possibleActions, a => a.Action == PlayerAction.PlaceRoad);
    }

    [Fact]
    public void BotRollsSeven_HumanHasToDisardBeforeBeforeBotPlacesRober()
    {
        // Build 2 settlements and roads for each player
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements();
        board.GetVertex(TestVertex.V10).BuildSettlement(board.GetRedPlayer());
        board.GetEdge(TestEdge.E15).BuildRoad(board.GetRedPlayer());
        board.GetVertex(TestVertex.V20).BuildSettlement(board.GetBluePlayer());
        board.GetEdge(TestEdge.E25).BuildRoad(board.GetBluePlayer());

        // Give human player enough cards to force discard
        board.GetRedPlayer().AssignResources(ResourceType.Brick, 4);
        board.GetRedPlayer().AssignResources(ResourceType.Wood, 4);

        // Set phase to bot's turn to roll
        board.GetGameState().Phase = new GamePhase(GameStates.RollOrUseDevCard, board.GetBluePlayer(), board.GetRedPlayer());
        board.GetGameState().SetDiceForTesting(new GameDice(3, 4)); // Force a 7
        GamePlayHelpers.GameLoop(board.GetGameState());

        Assert.Equal(GameStates.DiscardCards, board.GetGameState().Phase.PhaseState);
        Assert.Equal(board.GetRedPlayer().Id, board.GetGameState().Phase.CurrentPlayer.Id);

        GamePlayHelpers.DiscardCardRequestFromUser(board.GetGameState(), 
            new DiscardRequest(board.GetRedPlayer().Id, 
            new List<ResourceType>() {ResourceType.Brick, ResourceType.Brick, ResourceType.Wood, ResourceType.Wood}));

        Assert.Equal(GameStates.PlaceRobber, board.GetGameState().Phase.PhaseState);
        Assert.Equal(board.GetBluePlayer().Id, board.GetGameState().Phase.CurrentPlayer.Id);
    }

    [Fact]
    public void HumanRollsSeven_HumanHasToDisardBeforeBeforePlacingRober()
    {
        // Build 2 settlements and roads for each player
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements();
        board.GetVertex(TestVertex.V10).BuildSettlement(board.GetRedPlayer());
        board.GetEdge(TestEdge.E15).BuildRoad(board.GetRedPlayer());
        board.GetVertex(TestVertex.V20).BuildSettlement(board.GetBluePlayer());
        board.GetEdge(TestEdge.E25).BuildRoad(board.GetBluePlayer());

        // Give human player enough cards to force discard
        board.GetRedPlayer().AssignResources(ResourceType.Brick, 4);
        board.GetRedPlayer().AssignResources(ResourceType.Wood, 4);

        // Set phase to human's turn to roll
        board.GetGameState().Phase = new GamePhase(GameStates.RollOrUseDevCard, board.GetRedPlayer(), board.GetBluePlayer());
        board.GetGameState().SetDiceForTesting(new GameDice(3, 4)); // Force a 7
        GamePlayHelpers.GameLoop(board.GetGameState());

        Assert.Equal(GameStates.DiscardCards, board.GetGameState().Phase.PhaseState);
        Assert.Equal(board.GetRedPlayer().Id, board.GetGameState().Phase.CurrentPlayer.Id);

        GamePlayHelpers.DiscardCardRequestFromUser(board.GetGameState(), 
            new DiscardRequest(board.GetRedPlayer().Id, 
            new List<ResourceType>() {ResourceType.Brick, ResourceType.Brick, ResourceType.Wood, ResourceType.Wood}));

        Assert.Equal(GameStates.PlaceRobber, board.GetGameState().Phase.PhaseState);
        Assert.Equal(board.GetRedPlayer().Id, board.GetGameState().Phase.CurrentPlayer.Id);
    }

    [Fact]
    public void SelectTargetAfterPlaceRobber()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var orangePlayer = new Player("Tim", PlayerColor.Orange);
        var gs = board.GetGameState();
        gs.Players.Add(orangePlayer);
        board.GetVertex(TestVertex.V1).BuildSettlement(orangePlayer);
        gs.Phase = new GamePhase(GameStates.PlaceRobber, board.GetRedPlayer(), orangePlayer);
        gs.Phase.SetStateToReturnTo(GameStates.BuildOrTrade, gs.RobberTile);

        GamePlayHelpers.PlaceRobberForUser(gs, gs.Phase.CurrentPlayer.Id, board.GetTile(TestTile.T0).Id);
        Assert.NotNull(gs.Phase.TargetPlayers);
        Assert.Equal(2, gs.Phase.TargetPlayers.Count);
        Assert.Equal(GameStates.SelectTarget, gs.Phase.PhaseState);
    }


    [Fact]
    public async Task FullGameThroughInterfacesExposedToUser()
    {
        // var gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter);
        // GamePlayHelpers.StartGame(gs);

        // while (gs.Phase.PhaseState != GameStates.GameOver)
        // {
        //     var currentPlayer = gs.Phase.CurrentPlayer;
        //     if (!currentPlayer.IsBot)
        //     {
        //         if (gs.Phase.PhaseState == GameStates.PlaceFirstSettlement)
        //         {
        //             var v = gs.GetVertexFromTileInfo(gs.GetTileAt(2, 0), gs.GetTileAt(3, -1), gs.GetTileAt(4, 0), null);
        //             GamePlayHelpers.BuildSettlementRequestFromUser(gs, currentPlayer.Id, v.Id);
        //             var e = gs.GetEdgeFromTileInfo(gs.GetTileAt(2, 0), gs.GetTileAt(4, 0), null);
        //             GamePlayHelpers.BuildRoadRequestFromUser(gs, currentPlayer.Id, e.Id);
        //         }
        //         else if (gs.Phase.PhaseState == GameStates.PlaceSecondSettlement)
        //         {
        //             var v = gs.GetVertexFromTileInfo(gs.GetTileAt(1, 1), gs.GetTileAt(0, 2), gs.GetTileAt(2, 2), null);
        //             GamePlayHelpers.BuildSettlementRequestFromUser(gs, currentPlayer.Id, v.Id);
        //             var e = gs.GetEdgeFromTileInfo(gs.GetTileAt(1, 1), gs.GetTileAt(2, 2), null);
        //             GamePlayHelpers.BuildSettlementRequestFromUser(gs, currentPlayer.Id, v.Id);
        //         }
        //         else if (gs.Phase.PhaseState == GameStates.RollOrUseDevCard)
        //         {
        //             GamePlayHelpers.RollDice(gs);
        //         }
        //     }
            
        //     GamePlayHelpers.GameLoop(gs);
        // }
    }

    [Fact]
    public void DebugProductionIssuesInDev()
    {
        // var json = """{"id":"b3830827-cccb-4955-9aeb-1b586f1deb12","settings":{"type":"Starter","maxPlayers":2,"victoryPointsToWin":10,"roadsPerPlayer":15,"settlementsPerPlayer":5,"citiesPerPlayer":4},"phase":{"currentPlayerId":"P1","phaseState":"BuildOrTrade","endPlayerId":"P1","waitingForRoll":false,"devCardPlayedThisRound":false},"robberTileId":"T16","dice":{"die1":{"value":3,"random":true},"die2":{"value":6,"random":true}},"players":[{"id":"P1","name":"Lisa","color":"Red","isBot":false,"resources":{"Brick":1,"Wood":1,"Ore":0,"Grain":2,"Wool":1},"resourceCount":5,"devCardsPurchasedThisRound":[],"devCardsPlayed":[],"devCardsReadyToPlay":[],"developmentCardCount":0,"victoryPoints":2},{"id":"P2","name":"Hal","color":"Blue","isBot":true,"resources":{"Brick":1,"Wood":2,"Ore":1,"Grain":1,"Wool":1},"resourceCount":6,"devCardsPurchasedThisRound":[],"devCardsPlayed":[],"devCardsReadyToPlay":[],"developmentCardCount":0,"victoryPoints":2}],"tiles":[{"id":"T1","resource":"Ore","diceNumber":10,"x":-2,"y":-2},{"id":"T2","resource":"Grain","diceNumber":12,"x":-3,"y":-1},{"id":"T3","resource":"Grain","diceNumber":9,"x":-4,"y":0},{"id":"T4","resource":"Wood","diceNumber":8,"x":-3,"y":1},{"id":"T5","resource":"Brick","diceNumber":5,"x":-2,"y":2},{"id":"T6","resource":"Grain","diceNumber":6,"x":0,"y":2},{"id":"T7","resource":"Wool","diceNumber":11,"x":2,"y":2},{"id":"T8","resource":"Wool","diceNumber":5,"x":3,"y":1},{"id":"T9","resource":"Ore","diceNumber":8,"x":4,"y":0},{"id":"T10","resource":"Brick","diceNumber":10,"x":3,"y":-1},{"id":"T11","resource":"Wood","diceNumber":9,"x":2,"y":-2},{"id":"T12","resource":"Wool","diceNumber":2,"x":0,"y":-2},{"id":"T13","resource":"Brick","diceNumber":6,"x":-1,"y":-1},{"id":"T14","resource":"Wood","diceNumber":11,"x":-2,"y":0},{"id":"T15","resource":"Ore","diceNumber":3,"x":-1,"y":1},{"id":"T16","resource":"Grain","diceNumber":4,"x":1,"y":1},{"id":"T17","resource":"Wood","diceNumber":3,"x":2,"y":0},{"id":"T18","resource":"Wool","diceNumber":4,"x":1,"y":-1},{"id":"T19","resource":"Desert","diceNumber":7,"x":0,"y":0}],"edges":[{"id":"E1","tileIds":["T19","T18"]},{"id":"E2","tileIds":["T19","T17"]},{"id":"E3","tileIds":["T19","T16"]},{"id":"E4","tileIds":["T19","T15"]},{"id":"E5","tileIds":["T19","T14"]},{"id":"E6","tileIds":["T19","T13"]},{"id":"E7","tileIds":["T13","T12"]},{"id":"E8","tileIds":["T13","T18"]},{"id":"E9","tileIds":["T13","T14"]},{"id":"E10","tileIds":["T13","T2"]},{"id":"E11","tileIds":["T13","T1"]},{"id":"E12","direction":"NE","tileIds":["T1"]},{"id":"E13","tileIds":["T1","T12"]},{"id":"E14","tileIds":["T1","T2"]},{"id":"E15","direction":"W","tileIds":["T1"]},{"id":"E16","direction":"NW","tileIds":["T1"]},{"id":"E17","playerId":"P2","tileIds":["T2","T14"]},{"id":"E18","tileIds":["T2","T3"]},{"id":"E19","direction":"W","tileIds":["T2"]},{"id":"E20","direction":"NW","tileIds":["T2"]},{"id":"E21","playerId":"P2","tileIds":["T3","T14"]},{"id":"E22","tileIds":["T3","T4"]},{"id":"E23","direction":"SW","tileIds":["T3"]},{"id":"E24","direction":"W","tileIds":["T3"]},{"id":"E25","direction":"NW","tileIds":["T3"]},{"id":"E26","tileIds":["T4","T14"]},{"id":"E27","tileIds":["T4","T15"]},{"id":"E28","tileIds":["T4","T5"]},{"id":"E29","direction":"SW","tileIds":["T4"]},{"id":"E30","direction":"W","tileIds":["T4"]},{"id":"E31","tileIds":["T5","T15"]},{"id":"E32","tileIds":["T5","T6"]},{"id":"E33","direction":"SE","tileIds":["T5"]},{"id":"E34","direction":"SW","tileIds":["T5"]},{"id":"E35","direction":"W","tileIds":["T5"]},{"id":"E36","tileIds":["T6","T16"]},{"id":"E37","tileIds":["T6","T7"]},{"id":"E38","direction":"SE","tileIds":["T6"]},{"id":"E39","direction":"SW","tileIds":["T6"]},{"id":"E40","playerId":"P2","tileIds":["T6","T15"]},{"id":"E41","tileIds":["T15","T16"]},{"id":"E42","tileIds":["T15","T14"]},{"id":"E43","tileIds":["T16","T17"]},{"id":"E44","tileIds":["T16","T8"]},{"id":"E45","playerId":"P1","tileIds":["T16","T7"]},{"id":"E46","tileIds":["T7","T8"]},{"id":"E47","direction":"E","tileIds":["T7"]},{"id":"E48","direction":"SE","tileIds":["T7"]},{"id":"E49","direction":"SW","tileIds":["T7"]},{"id":"E50","tileIds":["T8","T9"]},{"id":"E51","direction":"E","tileIds":["T8"]},{"id":"E52","direction":"SE","tileIds":["T8"]},{"id":"E53","tileIds":["T8","T17"]},{"id":"E54","tileIds":["T17","T10"]},{"id":"E55","playerId":"P1","tileIds":["T17","T9"]},{"id":"E56","tileIds":["T17","T18"]},{"id":"E57","tileIds":["T18","T11"]},{"id":"E58","tileIds":["T18","T10"]},{"id":"E59","tileIds":["T18","T12"]},{"id":"E60","direction":"NE","tileIds":["T12"]},{"id":"E61","tileIds":["T12","T11"]},{"id":"E62","direction":"NW","tileIds":["T12"]},{"id":"E63","direction":"NE","tileIds":["T11"]},{"id":"E64","direction":"E","tileIds":["T11"]},{"id":"E65","tileIds":["T11","T10"]},{"id":"E66","direction":"NW","tileIds":["T11"]},{"id":"E67","direction":"NE","tileIds":["T10"]},{"id":"E68","direction":"E","tileIds":["T10"]},{"id":"E69","tileIds":["T10","T9"]},{"id":"E70","direction":"NE","tileIds":["T9"]},{"id":"E71","direction":"E","tileIds":["T9"]},{"id":"E72","direction":"SE","tileIds":["T9"]}],"vertices":[{"id":"V1","tileIds":["T19","T18","T13"]},{"id":"V2","tileIds":["T19","T17","T18"]},{"id":"V3","tileIds":["T19","T16","T17"]},{"id":"V4","tileIds":["T19","T15","T16"]},{"id":"V5","tileIds":["T19","T14","T15"]},{"id":"V6","tileIds":["T19","T13","T14"]},{"id":"V7","tileIds":["T13","T12","T1"]},{"id":"V8","tileIds":["T13","T18","T12"]},{"id":"V9","tileIds":["T13","T2","T14"]},{"id":"V10","tileIds":["T13","T1","T2"]},{"id":"V11","direction":"N","tileIds":["T1"]},{"id":"V12","tileIds":["T1","T12"]},{"id":"V13","tileIds":["T1","T2"]},{"id":"V14","direction":"NW","tileIds":["T1"]},{"id":"V15","building":"Blocked","tileIds":["T2","T3","T14"]},{"id":"V16","tileIds":["T2","T3"]},{"id":"V17","direction":"NW","tileIds":["T2"]},{"id":"V18","building":"Settlement","playerId":"P2","tileIds":["T3","T4","T14"]},{"id":"V19","building":"Blocked","tileIds":["T3","T4"]},{"id":"V20","direction":"SW","tileIds":["T3"]},{"id":"V21","direction":"NW","tileIds":["T3"]},{"id":"V22","building":"Blocked","tileIds":["T4","T15","T14"]},{"id":"V23","building":"Blocked","tileIds":["T4","T5","T15"]},{"id":"V24","tileIds":["T4","T5"]},{"id":"V25","direction":"SW","tileIds":["T4"]},{"id":"V26","building":"Settlement","playerId":"P2","tileIds":["T5","T6","T15"]},{"id":"V27","building":"Blocked","tileIds":["T5","T6"]},{"id":"V28","direction":"S","tileIds":["T5"]},{"id":"V29","direction":"SW","tileIds":["T5"]},{"id":"V30","building":"Blocked","tileIds":["T6","T16","T15"]},{"id":"V31","building":"Settlement","playerId":"P1","tileIds":["T6","T7","T16"]},{"id":"V32","building":"Blocked","tileIds":["T6","T7"]},{"id":"V33","direction":"S","tileIds":["T6"]},{"id":"V34","tileIds":["T16","T8","T17"]},{"id":"V35","building":"Blocked","tileIds":["T16","T7","T8"]},{"id":"V36","tileIds":["T7","T8"]},{"id":"V37","direction":"SE","tileIds":["T7"]},{"id":"V38","direction":"S","tileIds":["T7"]},{"id":"V39","building":"Blocked","tileIds":["T8","T9","T17"]},{"id":"V40","tileIds":["T8","T9"]},{"id":"V41","direction":"SE","tileIds":["T8"]},{"id":"V42","building":"Blocked","tileIds":["T17","T10","T18"]},{"id":"V43","building":"Settlement","playerId":"P1","tileIds":["T17","T9","T10"]},{"id":"V44","tileIds":["T18","T11","T12"]},{"id":"V45","tileIds":["T18","T10","T11"]},{"id":"V46","direction":"N","tileIds":["T12"]},{"id":"V47","tileIds":["T12","T11"]},{"id":"V48","direction":"N","tileIds":["T11"]},{"id":"V49","direction":"NE","tileIds":["T11"]},{"id":"V50","tileIds":["T11","T10"]},{"id":"V51","direction":"NE","tileIds":["T10"]},{"id":"V52","building":"Blocked","tileIds":["T10","T9"]},{"id":"V53","direction":"NE","tileIds":["T9"]},{"id":"V54","direction":"SE","tileIds":["T9"]}],"ports":[{"id":"R1","type":"ThreeToOne","vertices":["V11","V14"]},{"id":"R2","type":"Grain","vertices":["V46","V47"]},{"id":"R3","type":"Ore","vertices":["V51","V50"]},{"id":"R4","type":"ThreeToOne","vertices":["V53","V54"]},{"id":"R5","type":"Wool","vertices":["V41","V36"]},{"id":"R6","type":"ThreeToOne","vertices":["V33","V32"]},{"id":"R7","type":"ThreeToOne","vertices":["V28","V29"]},{"id":"R8","type":"Brick","vertices":["V25","V19"]},{"id":"R9","type":"Wood","vertices":["V17","V16"]}],"developmentCards":["Knight","Knight","VictoryPoint","VictoryPoint","VictoryPoint","Monopoly","RoadBuilding","Knight","Knight","Knight","Knight","Monopoly","VictoryPoint","Knight","Knight","Knight","VictoryPoint","RoadBuilding","Knight","YearOfPlenty","Knight","Knight","YearOfPlenty","Knight","Knight"],"eventRecord":[]}""";
        // var options = new JsonSerializerOptions
        // {
        //     PropertyNameCaseInsensitive = true,
        //     Converters = { new JsonStringEnumConverter() }
        // };

        // var dto = JsonSerializer.Deserialize<DTOs.GameStateDTO>(json, options);
        // var gs = GamePlayHelpers.LoadAndPrepareGameStateDTO(dto);
        // var possibleActions = GamePlayHelpers.GetPossiblePlayerActions(gs, gs.Phase.CurrentPlayer);
        // Assert.NotNull(possibleActions);

        // Assert.True(false);
    }
}
