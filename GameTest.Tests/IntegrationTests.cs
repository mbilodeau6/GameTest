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
        Assert.True(GamePlayHelpers.CountSettlementsForPlayer(board.GetGameState(), board.GetBluePlayer()) >= 2);
        Assert.True(GamePlayHelpers.CountRoadsForPlayer(board.GetGameState(), board.GetBluePlayer()) >= 2);

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

    // [Fact]
    // public void DebugProductionIssuesInDev()
    // {
    //     var id = "ea59ea20-dd20-42dc-8b68-b00ba5e6c440";
    //     Guid.TryParse(id, out var guid);
    //     var json = """{"id":"ea59ea20-dd20-42dc-8b68-b00ba5e6c440","settings":{"type":"Starter","maxPlayers":2,"victoryPointsToWin":10,"roadsPerPlayer":15,"settlementsPerPlayer":5,"citiesPerPlayer":4},"phase":{"currentPlayerId":"P1","phaseState":"BuildOrTrade","endPlayerId":"P1"},"hasLongestRoadPlayerId":"","hasLargestArmyPlayerId":"","robberTileId":"T19","dice":{"die1":{"value":3,"random":true},"die2":{"value":6,"random":true},"waitingForRoll":false},"players":[{"id":"P1","name":"Lisa","color":"Red","isBot":false,"resources":{"Brick":1,"Wood":0,"Ore":2,"Grain":1,"Wool":2},"resourceCount":6,"developmentCards":{"RoadBuilding":0,"VictoryPoint":0,"Monopoly":0,"YearOfPlenty":0,"Knight":0},"developmentCardCount":0},{"id":"P2","name":"Hal","color":"Blue","isBot":true,"resources":{"Brick":2,"Wood":4,"Ore":0,"Grain":5,"Wool":0},"resourceCount":11,"developmentCards":{"RoadBuilding":0,"VictoryPoint":0,"Monopoly":0,"YearOfPlenty":0,"Knight":0},"developmentCardCount":0}],"tiles":[{"id":"T1","resource":"Ore","diceNumber":10,"x":-2,"y":-2},{"id":"T2","resource":"Grain","diceNumber":12,"x":-3,"y":-1},{"id":"T3","resource":"Grain","diceNumber":9,"x":-4,"y":0},{"id":"T4","resource":"Wood","diceNumber":8,"x":-3,"y":1},{"id":"T5","resource":"Brick","diceNumber":5,"x":-2,"y":2},{"id":"T6","resource":"Grain","diceNumber":6,"x":0,"y":2},{"id":"T7","resource":"Wool","diceNumber":11,"x":2,"y":2},{"id":"T8","resource":"Wool","diceNumber":5,"x":3,"y":1},{"id":"T9","resource":"Ore","diceNumber":8,"x":4,"y":0},{"id":"T10","resource":"Brick","diceNumber":10,"x":3,"y":-1},{"id":"T11","resource":"Wood","diceNumber":9,"x":2,"y":-2},{"id":"T12","resource":"Wool","diceNumber":2,"x":0,"y":-2},{"id":"T13","resource":"Brick","diceNumber":6,"x":-1,"y":-1},{"id":"T14","resource":"Wood","diceNumber":11,"x":-2,"y":0},{"id":"T15","resource":"Ore","diceNumber":3,"x":-1,"y":1},{"id":"T16","resource":"Grain","diceNumber":4,"x":1,"y":1},{"id":"T17","resource":"Wood","diceNumber":3,"x":2,"y":0},{"id":"T18","resource":"Wool","diceNumber":4,"x":1,"y":-1},{"id":"T19","resource":"Desert","diceNumber":7,"x":0,"y":0}],"edges":[{"id":"E1","playerId":null,"direction":null,"tileIds":["T19","T18"]},{"id":"E2","playerId":null,"direction":null,"tileIds":["T19","T17"]},{"id":"E3","playerId":null,"direction":null,"tileIds":["T19","T16"]},{"id":"E4","playerId":null,"direction":null,"tileIds":["T19","T15"]},{"id":"E5","playerId":null,"direction":null,"tileIds":["T19","T14"]},{"id":"E6","playerId":null,"direction":null,"tileIds":["T19","T13"]},{"id":"E7","playerId":null,"direction":null,"tileIds":["T13","T12"]},{"id":"E8","playerId":null,"direction":null,"tileIds":["T13","T18"]},{"id":"E9","playerId":null,"direction":null,"tileIds":["T13","T14"]},{"id":"E10","playerId":null,"direction":null,"tileIds":["T13","T2"]},{"id":"E11","playerId":null,"direction":null,"tileIds":["T13","T1"]},{"id":"E12","playerId":null,"direction":"NE","tileIds":["T1"]},{"id":"E13","playerId":null,"direction":null,"tileIds":["T1","T12"]},{"id":"E14","playerId":null,"direction":null,"tileIds":["T1","T2"]},{"id":"E15","playerId":null,"direction":"W","tileIds":["T1"]},{"id":"E16","playerId":null,"direction":"NW","tileIds":["T1"]},{"id":"E17","playerId":"P2","direction":null,"tileIds":["T2","T14"]},{"id":"E18","playerId":null,"direction":null,"tileIds":["T2","T3"]},{"id":"E19","playerId":null,"direction":"W","tileIds":["T2"]},{"id":"E20","playerId":null,"direction":"NW","tileIds":["T2"]},{"id":"E21","playerId":"P2","direction":null,"tileIds":["T3","T14"]},{"id":"E22","playerId":null,"direction":null,"tileIds":["T3","T4"]},{"id":"E23","playerId":null,"direction":"SW","tileIds":["T3"]},{"id":"E24","playerId":null,"direction":"W","tileIds":["T3"]},{"id":"E25","playerId":null,"direction":"NW","tileIds":["T3"]},{"id":"E26","playerId":null,"direction":null,"tileIds":["T4","T14"]},{"id":"E27","playerId":null,"direction":null,"tileIds":["T4","T15"]},{"id":"E28","playerId":null,"direction":null,"tileIds":["T4","T5"]},{"id":"E29","playerId":null,"direction":"SW","tileIds":["T4"]},{"id":"E30","playerId":null,"direction":"W","tileIds":["T4"]},{"id":"E31","playerId":null,"direction":null,"tileIds":["T5","T15"]},{"id":"E32","playerId":null,"direction":null,"tileIds":["T5","T6"]},{"id":"E33","playerId":null,"direction":"SE","tileIds":["T5"]},{"id":"E34","playerId":null,"direction":"SW","tileIds":["T5"]},{"id":"E35","playerId":null,"direction":"W","tileIds":["T5"]},{"id":"E36","playerId":null,"direction":null,"tileIds":["T6","T16"]},{"id":"E37","playerId":null,"direction":null,"tileIds":["T6","T7"]},{"id":"E38","playerId":null,"direction":"SE","tileIds":["T6"]},{"id":"E39","playerId":null,"direction":"SW","tileIds":["T6"]},{"id":"E40","playerId":"P2","direction":null,"tileIds":["T6","T15"]},{"id":"E41","playerId":null,"direction":null,"tileIds":["T15","T16"]},{"id":"E42","playerId":null,"direction":null,"tileIds":["T15","T14"]},{"id":"E43","playerId":null,"direction":null,"tileIds":["T16","T17"]},{"id":"E44","playerId":null,"direction":null,"tileIds":["T16","T8"]},{"id":"E45","playerId":"P1","direction":null,"tileIds":["T16","T7"]},{"id":"E46","playerId":null,"direction":null,"tileIds":["T7","T8"]},{"id":"E47","playerId":null,"direction":"E","tileIds":["T7"]},{"id":"E48","playerId":null,"direction":"SE","tileIds":["T7"]},{"id":"E49","playerId":null,"direction":"SW","tileIds":["T7"]},{"id":"E50","playerId":null,"direction":null,"tileIds":["T8","T9"]},{"id":"E51","playerId":null,"direction":"E","tileIds":["T8"]},{"id":"E52","playerId":null,"direction":"SE","tileIds":["T8"]},{"id":"E53","playerId":null,"direction":null,"tileIds":["T8","T17"]},{"id":"E54","playerId":"P1","direction":null,"tileIds":["T17","T10"]},{"id":"E55","playerId":null,"direction":null,"tileIds":["T17","T9"]},{"id":"E56","playerId":null,"direction":null,"tileIds":["T17","T18"]},{"id":"E57","playerId":null,"direction":null,"tileIds":["T18","T11"]},{"id":"E58","playerId":"P1","direction":null,"tileIds":["T18","T10"]},{"id":"E59","playerId":null,"direction":null,"tileIds":["T18","T12"]},{"id":"E60","playerId":null,"direction":"NE","tileIds":["T12"]},{"id":"E61","playerId":null,"direction":null,"tileIds":["T12","T11"]},{"id":"E62","playerId":null,"direction":"NW","tileIds":["T12"]},{"id":"E63","playerId":null,"direction":"NE","tileIds":["T11"]},{"id":"E64","playerId":null,"direction":"E","tileIds":["T11"]},{"id":"E65","playerId":null,"direction":null,"tileIds":["T11","T10"]},{"id":"E66","playerId":null,"direction":"NW","tileIds":["T11"]},{"id":"E67","playerId":null,"direction":"NE","tileIds":["T10"]},{"id":"E68","playerId":null,"direction":"E","tileIds":["T10"]},{"id":"E69","playerId":null,"direction":null,"tileIds":["T10","T9"]},{"id":"E70","playerId":null,"direction":"NE","tileIds":["T9"]},{"id":"E71","playerId":null,"direction":"E","tileIds":["T9"]},{"id":"E72","playerId":null,"direction":"SE","tileIds":["T9"]}],"vertices":[{"id":"V1","building":null,"playerId":null,"direction":null,"tileIds":["T19","T18","T13"]},{"id":"V2","building":null,"playerId":null,"direction":null,"tileIds":["T19","T17","T18"]},{"id":"V3","building":null,"playerId":null,"direction":null,"tileIds":["T19","T16","T17"]},{"id":"V4","building":null,"playerId":null,"direction":null,"tileIds":["T19","T15","T16"]},{"id":"V5","building":null,"playerId":null,"direction":null,"tileIds":["T19","T14","T15"]},{"id":"V6","building":null,"playerId":null,"direction":null,"tileIds":["T19","T13","T14"]},{"id":"V7","building":null,"playerId":null,"direction":null,"tileIds":["T13","T12","T1"]},{"id":"V8","building":null,"playerId":null,"direction":null,"tileIds":["T13","T18","T12"]},{"id":"V9","building":null,"playerId":null,"direction":null,"tileIds":["T13","T2","T14"]},{"id":"V10","building":null,"playerId":null,"direction":null,"tileIds":["T13","T1","T2"]},{"id":"V11","building":null,"playerId":null,"direction":"N","tileIds":["T1"]},{"id":"V12","building":null,"playerId":null,"direction":null,"tileIds":["T1","T12"]},{"id":"V13","building":null,"playerId":null,"direction":null,"tileIds":["T1","T2"]},{"id":"V14","building":null,"playerId":null,"direction":"NW","tileIds":["T1"]},{"id":"V15","building":"Blocked","playerId":null,"direction":null,"tileIds":["T2","T3","T14"]},{"id":"V16","building":null,"playerId":null,"direction":null,"tileIds":["T2","T3"]},{"id":"V17","building":null,"playerId":null,"direction":"NW","tileIds":["T2"]},{"id":"V18","building":"Settlement","playerId":"P2","direction":null,"tileIds":["T3","T4","T14"]},{"id":"V19","building":"Blocked","playerId":null,"direction":null,"tileIds":["T3","T4"]},{"id":"V20","building":null,"playerId":null,"direction":"SW","tileIds":["T3"]},{"id":"V21","building":null,"playerId":null,"direction":"NW","tileIds":["T3"]},{"id":"V22","building":"Blocked","playerId":null,"direction":null,"tileIds":["T4","T15","T14"]},{"id":"V23","building":"Blocked","playerId":null,"direction":null,"tileIds":["T4","T5","T15"]},{"id":"V24","building":null,"playerId":null,"direction":null,"tileIds":["T4","T5"]},{"id":"V25","building":null,"playerId":null,"direction":"SW","tileIds":["T4"]},{"id":"V26","building":"Settlement","playerId":"P2","direction":null,"tileIds":["T5","T6","T15"]},{"id":"V27","building":"Blocked","playerId":null,"direction":null,"tileIds":["T5","T6"]},{"id":"V28","building":null,"playerId":null,"direction":"S","tileIds":["T5"]},{"id":"V29","building":null,"playerId":null,"direction":"SW","tileIds":["T5"]},{"id":"V30","building":"Blocked","playerId":null,"direction":null,"tileIds":["T6","T16","T15"]},{"id":"V31","building":"Settlement","playerId":"P1","direction":null,"tileIds":["T6","T7","T16"]},{"id":"V32","building":"Blocked","playerId":null,"direction":null,"tileIds":["T6","T7"]},{"id":"V33","building":null,"playerId":null,"direction":"S","tileIds":["T6"]},{"id":"V34","building":null,"playerId":null,"direction":null,"tileIds":["T16","T8","T17"]},{"id":"V35","building":"Blocked","playerId":null,"direction":null,"tileIds":["T16","T7","T8"]},{"id":"V36","building":null,"playerId":null,"direction":null,"tileIds":["T7","T8"]},{"id":"V37","building":null,"playerId":null,"direction":"SE","tileIds":["T7"]},{"id":"V38","building":null,"playerId":null,"direction":"S","tileIds":["T7"]},{"id":"V39","building":"Blocked","playerId":null,"direction":null,"tileIds":["T8","T9","T17"]},{"id":"V40","building":null,"playerId":null,"direction":null,"tileIds":["T8","T9"]},{"id":"V41","building":null,"playerId":null,"direction":"SE","tileIds":["T8"]},{"id":"V42","building":"Blocked","playerId":null,"direction":null,"tileIds":["T17","T10","T18"]},{"id":"V43","building":"Settlement","playerId":"P1","direction":null,"tileIds":["T17","T9","T10"]},{"id":"V44","building":null,"playerId":null,"direction":null,"tileIds":["T18","T11","T12"]},{"id":"V45","building":null,"playerId":null,"direction":null,"tileIds":["T18","T10","T11"]},{"id":"V46","building":null,"playerId":null,"direction":"N","tileIds":["T12"]},{"id":"V47","building":null,"playerId":null,"direction":null,"tileIds":["T12","T11"]},{"id":"V48","building":null,"playerId":null,"direction":"N","tileIds":["T11"]},{"id":"V49","building":null,"playerId":null,"direction":"NE","tileIds":["T11"]},{"id":"V50","building":null,"playerId":null,"direction":null,"tileIds":["T11","T10"]},{"id":"V51","building":null,"playerId":null,"direction":"NE","tileIds":["T10"]},{"id":"V52","building":"Blocked","playerId":null,"direction":null,"tileIds":["T10","T9"]},{"id":"V53","building":null,"playerId":null,"direction":"NE","tileIds":["T9"]},{"id":"V54","building":null,"playerId":null,"direction":"SE","tileIds":["T9"]}]}""";
    //     var options = new JsonSerializerOptions
    //     {
    //         PropertyNameCaseInsensitive = true,
    //         Converters = { new JsonStringEnumConverter() }
    //     };

    //     var dto = JsonSerializer.Deserialize<DTOs.GameStateDTO>(json, options);
    //     var gs = new GameState(dto);
    //     GamePlayHelpers.EndTurn(gs.Phase.CurrentPlayer, gs);

    //     Assert.True(false);
    // }
}
