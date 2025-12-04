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
    }

    [Fact]
    public async Task FullGameThroughInterfacesExposedToUser()
    {
        var gameService = new GameService();
        var gs = gameService.CreateGame(GameType.Starter.ToString());

        Assert.NotNull(gs);

        var gameId = gs.Id;
        var humanId = gs.Players.Find(p => p.IsBot == false);
        var botId = gs.Players.Find(p => p.IsBot == true);

        Assert.Equal(GameStates.SettingUpBoard, gs.Phase.PhaseState);

        // TODO: Need to figure out how to set up (or bypass) connection to blob container.
        // var response = await gameService.StartGameAsync(gameId);
        // Assert.True(response.Success);

        // var vp = new VertexPicker(gs);
        // Assert.NotNull(vp);

        // Assert.Equal(GameStates.PlaceFirstSettlement, gs.Phase.PhaseState);
        // response =  await gameService.BuildSettlementAsync(gameId, vp.PickVertex().Id, gs.Phase.CurrentPlayer.Id);
        // Assert.True(response.Success);

        // Assert.Equal(GameStates.PlaceFirstRoad, gs.Phase.PhaseState);
        // response = await gameService.BuildRoadAsync(gameId, vp.PickEdge().Id, gs.Phase.CurrentPlayer.Id);
        // Assert.True(response.Success);

        // Assert.Equal(GameStates.PlaceSecondSettlement, gs.Phase.PhaseState);
        // response =  await gameService.BuildSettlementAsync(gameId, vp.PickVertex().Id, gs.Phase.CurrentPlayer.Id);
        // Assert.True(response.Success);

        // Assert.Equal(GameStates.PlaceSecondSettlement, gs.Phase.PhaseState);
        // response = await gameService.BuildRoadAsync(gameId, vp.PickEdge().Id, gs.Phase.CurrentPlayer.Id);
        // Assert.True(response.Success);

        // Assert.Equal(GameStates.RollOrUseDevCard, gs.Phase.PhaseState);
        // response = await gameService.RollDiceAsync(gameId);
        // Assert.True(response.Success);


        // TODO: Continue to implement. Stopped because I couldn't figure out an easy way to deal with
        // blob container.
    }

    [Fact]
    public void DebugProductionIssuesInDev()
    {
    //     var json = """{"id":"5adc4f4c-c056-4dee-a1d7-a012b8d17bcd","settings":{"type":"Default","maxPlayers":2,"victoryPointsToWin":10,"roadsPerPlayer":15,"settlementsPerPlayer":5,"citiesPerPlayer":4},"phase":{"currentPlayerId":"P1","phaseState":"BuildOrTrade","endPlayerId":"P1","waitingForRoll":false},"robberTileId":"T4","dice":{"die1":{"value":2,"random":true},"die2":{"value":2,"random":true}},"players":[{"id":"P1","name":"Lisa","color":"Red","isBot":false,"resources":{"Brick":1,"Wood":2,"Ore":2,"Grain":0,"Wool":4},"resourceCount":9,"devCardsPurchasedThisRound":[],"devCardsPlayed":[],"devCardsReadyToPlay":[],"developmentCardCount":0,"victoryPoints":5},{"id":"P2","name":"Hal","color":"Blue","isBot":true,"resources":{"Brick":2,"Wood":2,"Ore":2,"Grain":0,"Wool":0},"resourceCount":6,"devCardsPurchasedThisRound":[],"devCardsPlayed":[],"devCardsReadyToPlay":[],"developmentCardCount":0,"victoryPoints":9}],"tiles":[{"id":"T1","resource":"Desert","diceNumber":7,"x":-2,"y":-2},{"id":"T2","resource":"Wool","diceNumber":5,"x":-3,"y":-1},{"id":"T3","resource":"Wood","diceNumber":2,"x":-4,"y":0},{"id":"T4","resource":"Brick","diceNumber":6,"x":-3,"y":1},{"id":"T5","resource":"Wool","diceNumber":3,"x":-2,"y":2},{"id":"T6","resource":"Ore","diceNumber":8,"x":0,"y":2},{"id":"T7","resource":"Grain","diceNumber":10,"x":2,"y":2},{"id":"T8","resource":"Wood","diceNumber":9,"x":3,"y":1},{"id":"T9","resource":"Ore","diceNumber":12,"x":4,"y":0},{"id":"T10","resource":"Ore","diceNumber":11,"x":3,"y":-1},{"id":"T11","resource":"Grain","diceNumber":4,"x":2,"y":-2},{"id":"T12","resource":"Brick","diceNumber":8,"x":0,"y":-2},{"id":"T13","resource":"Wood","diceNumber":10,"x":-1,"y":-1},{"id":"T14","resource":"Wood","diceNumber":9,"x":-2,"y":0},{"id":"T15","resource":"Wool","diceNumber":4,"x":-1,"y":1},{"id":"T16","resource":"Brick","diceNumber":5,"x":1,"y":1},{"id":"T17","resource":"Grain","diceNumber":6,"x":2,"y":0},{"id":"T18","resource":"Wool","diceNumber":3,"x":1,"y":-1},{"id":"T19","resource":"Grain","diceNumber":11,"x":0,"y":0}],"edges":[{"id":"E1","playerId":"P2","tileIds":["T19","T18"]},{"id":"E2","playerId":"P2","tileIds":["T19","T17"]},{"id":"E3","tileIds":["T19","T16"]},{"id":"E4","tileIds":["T19","T15"]},{"id":"E5","tileIds":["T19","T14"]},{"id":"E6","playerId":"P2","tileIds":["T19","T13"]},{"id":"E7","tileIds":["T13","T12"]},{"id":"E8","playerId":"P2","tileIds":["T13","T18"]},{"id":"E9","tileIds":["T13","T14"]},{"id":"E10","tileIds":["T13","T2"]},{"id":"E11","tileIds":["T13","T1"]},{"id":"E12","direction":"NE","tileIds":["T1"]},{"id":"E13","tileIds":["T1","T12"]},{"id":"E14","tileIds":["T1","T2"]},{"id":"E15","direction":"W","tileIds":["T1"]},{"id":"E16","direction":"NW","tileIds":["T1"]},{"id":"E17","tileIds":["T2","T14"]},{"id":"E18","tileIds":["T2","T3"]},{"id":"E19","direction":"W","tileIds":["T2"]},{"id":"E20","direction":"NW","tileIds":["T2"]},{"id":"E21","playerId":"P1","tileIds":["T3","T14"]},{"id":"E22","playerId":"P1","tileIds":["T3","T4"]},{"id":"E23","direction":"SW","tileIds":["T3"]},{"id":"E24","direction":"W","tileIds":["T3"]},{"id":"E25","direction":"NW","tileIds":["T3"]},{"id":"E26","playerId":"P1","tileIds":["T4","T14"]},{"id":"E27","tileIds":["T4","T15"]},{"id":"E28","tileIds":["T4","T5"]},{"id":"E29","direction":"SW","tileIds":["T4"]},{"id":"E30","direction":"W","tileIds":["T4"]},{"id":"E31","tileIds":["T5","T15"]},{"id":"E32","tileIds":["T5","T6"]},{"id":"E33","direction":"SE","tileIds":["T5"]},{"id":"E34","direction":"SW","tileIds":["T5"]},{"id":"E35","direction":"W","tileIds":["T5"]},{"id":"E36","tileIds":["T6","T16"]},{"id":"E37","tileIds":["T6","T7"]},{"id":"E38","direction":"SE","tileIds":["T6"]},{"id":"E39","direction":"SW","tileIds":["T6"]},{"id":"E40","playerId":"P1","tileIds":["T6","T15"]},{"id":"E41","playerId":"P1","tileIds":["T15","T16"]},{"id":"E42","tileIds":["T15","T14"]},{"id":"E43","playerId":"P2","tileIds":["T16","T17"]},{"id":"E44","tileIds":["T16","T8"]},{"id":"E45","playerId":"P2","tileIds":["T16","T7"]},{"id":"E46","playerId":"P2","tileIds":["T7","T8"]},{"id":"E47","direction":"E","tileIds":["T7"]},{"id":"E48","direction":"SE","tileIds":["T7"]},{"id":"E49","direction":"SW","tileIds":["T7"]},{"id":"E50","tileIds":["T8","T9"]},{"id":"E51","direction":"E","tileIds":["T8"]},{"id":"E52","direction":"SE","tileIds":["T8"]},{"id":"E53","playerId":"P2","tileIds":["T8","T17"]},{"id":"E54","tileIds":["T17","T10"]},{"id":"E55","playerId":"P2","tileIds":["T17","T9"]},{"id":"E56","tileIds":["T17","T18"]},{"id":"E57","tileIds":["T18","T11"]},{"id":"E58","tileIds":["T18","T10"]},{"id":"E59","tileIds":["T18","T12"]},{"id":"E60","direction":"NE","tileIds":["T12"]},{"id":"E61","tileIds":["T12","T11"]},{"id":"E62","direction":"NW","tileIds":["T12"]},{"id":"E63","direction":"NE","tileIds":["T11"]},{"id":"E64","direction":"E","tileIds":["T11"]},{"id":"E65","tileIds":["T11","T10"]},{"id":"E66","direction":"NW","tileIds":["T11"]},{"id":"E67","direction":"NE","tileIds":["T10"]},{"id":"E68","direction":"E","tileIds":["T10"]},{"id":"E69","tileIds":["T10","T9"]},{"id":"E70","direction":"NE","tileIds":["T9"]},{"id":"E71","direction":"E","tileIds":["T9"]},{"id":"E72","direction":"SE","tileIds":["T9"]}],"vertices":[{"id":"V1","building":"Blocked","tileIds":["T19","T18","T13"]},{"id":"V2","building":"Settlement","playerId":"P2","tileIds":["T19","T17","T18"]},{"id":"V3","building":"Blocked","tileIds":["T19","T16","T17"]},{"id":"V4","building":"Settlement","playerId":"P1","tileIds":["T19","T15","T16"]},{"id":"V5","building":"Blocked","tileIds":["T19","T14","T15"]},{"id":"V6","building":"Settlement","playerId":"P2","tileIds":["T19","T13","T14"]},{"id":"V7","building":"Blocked","tileIds":["T13","T12","T1"]},{"id":"V8","building":"Settlement","playerId":"P2","tileIds":["T13","T18","T12"]},{"id":"V9","building":"Blocked","tileIds":["T13","T2","T14"]},{"id":"V10","tileIds":["T13","T1","T2"]},{"id":"V11","direction":"N","tileIds":["T1"]},{"id":"V12","tileIds":["T1","T12"]},{"id":"V13","tileIds":["T1","T2"]},{"id":"V14","direction":"NW","tileIds":["T1"]},{"id":"V15","building":"Settlement","playerId":"P1","tileIds":["T2","T3","T14"]},{"id":"V16","building":"Blocked","tileIds":["T2","T3"]},{"id":"V17","direction":"NW","tileIds":["T2"]},{"id":"V18","building":"Blocked","tileIds":["T3","T4","T14"]},{"id":"V19","building":"Settlement","playerId":"P1","tileIds":["T3","T4"]},{"id":"V20","building":"Blocked","direction":"SW","tileIds":["T3"]},{"id":"V21","direction":"NW","tileIds":["T3"]},{"id":"V22","building":"Settlement","playerId":"P1","tileIds":["T4","T15","T14"]},{"id":"V23","building":"Blocked","tileIds":["T4","T5","T15"]},{"id":"V24","tileIds":["T4","T5"]},{"id":"V25","building":"Blocked","direction":"SW","tileIds":["T4"]},{"id":"V26","building":"Settlement","playerId":"P1","tileIds":["T5","T6","T15"]},{"id":"V27","building":"Blocked","tileIds":["T5","T6"]},{"id":"V28","direction":"S","tileIds":["T5"]},{"id":"V29","direction":"SW","tileIds":["T5"]},{"id":"V30","building":"Blocked","tileIds":["T6","T16","T15"]},{"id":"V31","building":"City","playerId":"P2","tileIds":["T6","T7","T16"]},{"id":"V32","building":"Blocked","tileIds":["T6","T7"]},{"id":"V33","direction":"S","tileIds":["T6"]},{"id":"V34","building":"City","playerId":"P2","tileIds":["T16","T8","T17"]},{"id":"V35","building":"Blocked","tileIds":["T16","T7","T8"]},{"id":"V36","building":"Settlement","playerId":"P2","tileIds":["T7","T8"]},{"id":"V37","building":"Blocked","direction":"SE","tileIds":["T7"]},{"id":"V38","direction":"S","tileIds":["T7"]},{"id":"V39","building":"Blocked","tileIds":["T8","T9","T17"]},{"id":"V40","tileIds":["T8","T9"]},{"id":"V41","building":"Blocked","direction":"SE","tileIds":["T8"]},{"id":"V42","building":"Blocked","tileIds":["T17","T10","T18"]},{"id":"V43","building":"Settlement","playerId":"P2","tileIds":["T17","T9","T10"]},{"id":"V44","building":"Blocked","tileIds":["T18","T11","T12"]},{"id":"V45","tileIds":["T18","T10","T11"]},{"id":"V46","direction":"N","tileIds":["T12"]},{"id":"V47","tileIds":["T12","T11"]},{"id":"V48","direction":"N","tileIds":["T11"]},{"id":"V49","direction":"NE","tileIds":["T11"]},{"id":"V50","tileIds":["T11","T10"]},{"id":"V51","direction":"NE","tileIds":["T10"]},{"id":"V52","building":"Blocked","tileIds":["T10","T9"]},{"id":"V53","direction":"NE","tileIds":["T9"]},{"id":"V54","direction":"SE","tileIds":["T9"]}],"ports":[{"id":"R1","type":"Wood","vertices":["V49","V50"]},{"id":"R2","type":"ThreeToOne","vertices":["V53","V54"]},{"id":"R3","type":"ThreeToOne","vertices":["V41","V36"]},{"id":"R4","type":"Brick","vertices":["V38","V32"]},{"id":"R5","type":"ThreeToOne","vertices":["V28","V29"]},{"id":"R6","type":"ThreeToOne","vertices":["V25","V19"]},{"id":"R7","type":"Ore","vertices":["V21","V16"]},{"id":"R8","type":"Wool","vertices":["V14","V11"]},{"id":"R9","type":"Grain","vertices":["V46","V47"]}],"eventRecord":[{"playerId":"P2","action":"PlaceSettlement","vertexId":"V31"},{"playerId":"P2","action":"PlaceRoad","edgeId":"E45"},{"playerId":"P1","action":"PlaceSettlement","vertexId":"V26"},{"playerId":"P1","action":"PlaceRoad","edgeId":"E40"},{"playerId":"P1","action":"PlaceSettlement","vertexId":"V22"},{"playerId":"P1","action":"PlaceRoad","edgeId":"E26"},{"playerId":"P2","action":"PlaceSettlement","vertexId":"V34"},{"playerId":"P2","action":"PlaceRoad","edgeId":"E43"},{"playerId":"P2","action":"RollDice","diceRoll":6},{"playerId":"P2","action":"PlaceRoad","edgeId":"E2"},{"playerId":"P1","action":"RollDice","diceRoll":3},{"playerId":"P1","action":"PlaceRoad","edgeId":"E41"},{"playerId":"P2","action":"RollDice","diceRoll":4},{"playerId":"P1","action":"RollDice","diceRoll":12},{"playerId":"P2","action":"RollDice","diceRoll":4},{"playerId":"P1","action":"RollDice","diceRoll":6},{"playerId":"P1","action":"TradeWithBank","resourcesUsed":{"Wool":4},"resourcesReceived":{"Grain":1}},{"playerId":"P2","action":"RollDice","diceRoll":5},{"playerId":"P1","action":"RollDice","diceRoll":5},{"playerId":"P2","action":"RollDice","diceRoll":6},{"playerId":"P1","action":"RollDice","diceRoll":9},{"playerId":"P1","action":"PlaceSettlement","vertexId":"V4"},{"playerId":"P2","action":"RollDice","diceRoll":6},{"playerId":"P2","action":"PlaceSettlement","vertexId":"V2"},{"playerId":"P1","action":"RollDice","diceRoll":6},{"playerId":"P2","action":"RollDice","diceRoll":5},{"playerId":"P2","action":"PlaceRoad","edgeId":"E46"},{"playerId":"P1","action":"RollDice","diceRoll":9},{"playerId":"P1","action":"PlaceRoad","edgeId":"E22"},{"playerId":"P2","action":"RollDice","diceRoll":7},{"playerId":"P2","action":"PlaceRobber","tileId":"T15"},{"playerId":"P2","action":"StealResource","resourcesReceived":{"Wool":1},"targetPlayerId":"P1"},{"playerId":"P1","action":"RollDice","diceRoll":9},{"playerId":"P1","action":"PlaceRoad","edgeId":"E21"},{"playerId":"P2","action":"RollDice","diceRoll":6},{"playerId":"P1","action":"RollDice","diceRoll":8},{"playerId":"P2","action":"RollDice","diceRoll":6},{"playerId":"P2","action":"PlaceSettlement","vertexId":"V36"},{"playerId":"P1","action":"RollDice","diceRoll":3},{"playerId":"P2","action":"RollDice","diceRoll":5},{"playerId":"P2","action":"PlaceRoad","edgeId":"E1"},{"playerId":"P1","action":"RollDice","diceRoll":8},{"playerId":"P2","action":"RollDice","diceRoll":7},{"playerId":"P2","action":"PlaceRobber","tileId":"T4"},{"playerId":"P2","action":"StealResource","resourcesReceived":{"Brick":1},"targetPlayerId":"P1"},{"playerId":"P1","action":"RollDice","diceRoll":6},{"playerId":"P2","action":"RollDice","diceRoll":9},{"playerId":"P2","action":"PlaceRoad","edgeId":"E8"},{"playerId":"P2","action":"PlaceSettlement","vertexId":"V8"},{"playerId":"P1","action":"RollDice","diceRoll":8},{"playerId":"P1","action":"TradeWithBank","resourcesUsed":{"Brick":4},"resourcesReceived":{"Grain":1}},{"playerId":"P1","action":"PlaceSettlement","vertexId":"V19"},{"playerId":"P2","action":"RollDice","diceRoll":10},{"playerId":"P2","action":"UpgradeSettlement","vertexId":"V31"},{"playerId":"P2","action":"PlaceRoad","edgeId":"E6"},{"playerId":"P1","action":"RollDice","diceRoll":5},{"playerId":"P2","action":"RollDice","diceRoll":9},{"playerId":"P1","action":"RollDice","diceRoll":8},{"playerId":"P2","action":"RollDice","diceRoll":3},{"playerId":"P2","action":"PlaceSettlement","vertexId":"V6"},{"playerId":"P1","action":"RollDice","diceRoll":6},{"playerId":"P1","action":"TradeWithBank","resourcesUsed":{"Ore":3},"resourcesReceived":{"Grain":1}},{"playerId":"P1","action":"PlaceSettlement","vertexId":"V15"},{"playerId":"P2","action":"RollDice","diceRoll":8},{"playerId":"P2","action":"UpgradeSettlement","vertexId":"V34"},{"playerId":"P2","action":"PlaceRoad","edgeId":"E53"},{"playerId":"P1","action":"RollDice","diceRoll":9},{"playerId":"P2","action":"RollDice","diceRoll":5},{"playerId":"P2","action":"PlaceRoad","edgeId":"E55"},{"playerId":"P2","action":"PlaceSettlement","vertexId":"V43"},{"playerId":"P1","action":"RollDice","diceRoll":4}],"developmentCards":["Knight","Knight","Knight","Knight","Knight","VictoryPoint","Knight","Knight","YearOfPlenty","VictoryPoint","Knight","VictoryPoint","Knight","VictoryPoint","RoadBuilding","Knight","VictoryPoint","Knight","Knight","Monopoly","Knight","YearOfPlenty","Knight","RoadBuilding","Monopoly"]}""";
    //     var options = new JsonSerializerOptions
    //     {
    //         PropertyNameCaseInsensitive = true,
    //         Converters = { new JsonStringEnumConverter() }
    //     };

    //     var dto = JsonSerializer.Deserialize<DTOs.GameStateDTO>(json, options);
    //     var gs = GamePlayHelpers.LoadAndPrepareGameStateDTO(dto);
    //     var player = gs.Players.First(p => !p.IsBot);
    //     GamePlayHelpers.BankTradeFromUser(gs, new TradeRequestDTO(player.Id, new Dictionary<string, int>() {{ResourceType.Wool.ToString(), 2}}, new Dictionary<string, int>() {{ResourceType.Grain.ToString(), 1}}));

    //     Assert.True(false);
    }
}
