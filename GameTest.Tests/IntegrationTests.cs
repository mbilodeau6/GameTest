using Xunit;
using GameTest.Models;
using GameTest.DTOs;
using GameTest.Services;

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
        var possibleActions = PossiblePlayerActions.GetPossiblePlayerActions(board.GetGameState(), board.GetGameState().Phase.CurrentPlayer);
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
        var orangePlayer = Player.CreateTestPlayer("Tim", PlayerColor.Orange);
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
    public void SelectTargetAfterPlayKnight()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var orangePlayer = Player.CreateTestPlayer("Tim", PlayerColor.Orange);
        var gs = board.GetGameState();
        gs.Players.Add(orangePlayer);
        board.GetVertex(TestVertex.V1).BuildSettlement(orangePlayer);
        board.GetRedPlayer().AssignDevelopmentCard(DevelopmentCardType.Knight);
        board.GetRedPlayer().MakeNewDevelopmentCardsPlayable();
        gs.Phase = new GamePhase(GameStates.RollOrUseDevCard, board.GetRedPlayer(), orangePlayer);

        var devCardRequest = new PlayDevCardRequest(board.GetRedPlayer().Id, DevelopmentCardType.Knight, null, board.GetTile(TestTile.T0).Id);
        var response = GamePlayHelpers.PlayKnightDevCardFromUser(gs, devCardRequest);

        Assert.True(response.Success);
        Assert.NotNull(response.GameState);
        Assert.Equal(GameStates.SelectTarget, gs.Phase.PhaseState);
        Assert.NotNull(gs.Phase.TargetPlayers);
        Assert.Equal(2, gs.Phase.TargetPlayers.Count);
    }

    [Fact]
    public void LastPlayer_UndoRedoFirstRoad_ShouldStayOnPlayer()
    {
        var board = TestHelpers.CreateOriginalTestBoard();
        var gs = board.GetGameState();
        gs.Phase = new GamePhase(GameStates.PlaceFirstSettlement, board.GetRedPlayer(), board.GetBluePlayer());

        var response = GamePlayHelpers.BuildSettlementRequestFromUser(gs, board.GetRedPlayer().Id, board.GetVertex(TestVertex.V5).Id);
        Assert.True(response.Success);
        response = GamePlayHelpers.BuildRoadRequestFromUser(gs, board.GetRedPlayer().Id, board.GetEdge(TestEdge.E11).Id);
        Assert.True(response.Success);

        Assert.NotNull(gs.Phase.CurrentPlayer);
        Assert.Equal(board.GetBluePlayer().Id, gs.Phase.CurrentPlayer.Id);
        Assert.NotNull(gs.Phase.EndPlayer);
        Assert.Equal(board.GetBluePlayer().Id, gs.Phase.EndPlayer.Id);
        Assert.Equal(GameStates.PlaceFirstSettlement, gs.Phase.PhaseState);

        response = GamePlayHelpers.BuildSettlementRequestFromUser(gs, board.GetBluePlayer().Id, board.GetVertex(TestVertex.V3).Id);
        Assert.True(response.Success);

        Assert.NotNull(gs.Phase.CurrentPlayer);
        Assert.Equal(board.GetBluePlayer().Id, gs.Phase.CurrentPlayer.Id);
        Assert.NotNull(gs.Phase.EndPlayer);
        Assert.Equal(board.GetBluePlayer().Id, gs.Phase.EndPlayer.Id);
        Assert.Equal(GameStates.PlaceFirstRoad, gs.Phase.PhaseState);
        
        response = GamePlayHelpers.BuildRoadRequestFromUser(gs, board.GetBluePlayer().Id, board.GetEdge(TestEdge.E2).Id);
        Assert.True(response.Success);
        var roadBuildEventId = gs.EventRecord.Last().Id;

        Assert.NotNull(gs.Phase.CurrentPlayer);
        Assert.Equal(board.GetBluePlayer().Id, gs.Phase.CurrentPlayer.Id);
        Assert.NotNull(gs.Phase.EndPlayer);
        Assert.Equal(board.GetRedPlayer().Id, gs.Phase.EndPlayer.Id);
        Assert.Equal(GameStates.PlaceSecondSettlement, gs.Phase.PhaseState);

        response = UndoHelpers.UndoFromUser(gs, new BaseRequest(board.GetBluePlayer().Id));

        Assert.NotNull(gs.Phase.CurrentPlayer);
        Assert.Equal(board.GetBluePlayer().Id, gs.Phase.CurrentPlayer.Id);
        Assert.NotNull(gs.Phase.EndPlayer);
        Assert.Equal(board.GetBluePlayer().Id, gs.Phase.EndPlayer.Id);
        Assert.Equal(GameStates.PlaceFirstRoad, gs.Phase.PhaseState);
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
        // var json = """{"id":"9be9b509-2c3f-4f17-beea-74af44f2c6ea","settings":{"type":"Default","maxPlayers":4,"victoryPointsToWin":10,"roadsPerPlayer":15,"settlementsPerPlayer":5,"citiesPerPlayer":4},"phase":{"currentPlayerId":"P2","phaseState":"BuildOrTrade","endPlayerId":"P1","waitingForRoll":false,"devCardPlayedThisRound":false},"robberTileId":"T11","dice":{"die1":{"value":6,"random":true},"die2":{"value":1,"random":true}},"players":[{"id":"P1","name":"Michael","color":"Orange","isBot":false,"resources":{"Brick":1,"Wood":1,"Ore":2,"Grain":0,"Wool":0},"resourceCount":4,"devCardsPurchasedThisRound":[],"devCardsPlayed":[],"devCardsReadyToPlay":[],"developmentCardCount":0,"victoryPoints":4,"fullVictoryPoints":4},{"id":"P2","name":"Lisa","color":"Blue","isBot":false,"resources":{"Brick":0,"Wood":1,"Ore":0,"Grain":0,"Wool":0},"resourceCount":1,"devCardsPurchasedThisRound":[],"devCardsPlayed":[],"devCardsReadyToPlay":["Knight"],"developmentCardCount":1,"victoryPoints":5,"fullVictoryPoints":5}],"tiles":[{"id":"T1","resource":"Wool","diceNumber":5,"x":-2,"y":-2},{"id":"T2","resource":"Grain","diceNumber":2,"x":-3,"y":-1},{"id":"T3","resource":"Wool","diceNumber":6,"x":-4,"y":0},{"id":"T4","resource":"Wood","diceNumber":3,"x":-3,"y":1},{"id":"T5","resource":"Wool","diceNumber":8,"x":-2,"y":2},{"id":"T6","resource":"Wood","diceNumber":10,"x":0,"y":2},{"id":"T7","resource":"Ore","diceNumber":9,"x":2,"y":2},{"id":"T8","resource":"Wood","diceNumber":12,"x":3,"y":1},{"id":"T9","resource":"Ore","diceNumber":11,"x":4,"y":0},{"id":"T10","resource":"Grain","diceNumber":4,"x":3,"y":-1},{"id":"T11","resource":"Brick","diceNumber":8,"x":2,"y":-2},{"id":"T12","resource":"Wool","diceNumber":10,"x":0,"y":-2},{"id":"T13","resource":"Grain","diceNumber":9,"x":-1,"y":-1},{"id":"T14","resource":"Grain","diceNumber":4,"x":-2,"y":0},{"id":"T15","resource":"Desert","diceNumber":7,"x":-1,"y":1},{"id":"T16","resource":"Wood","diceNumber":5,"x":1,"y":1},{"id":"T17","resource":"Ore","diceNumber":6,"x":2,"y":0},{"id":"T18","resource":"Brick","diceNumber":3,"x":1,"y":-1},{"id":"T19","resource":"Brick","diceNumber":11,"x":0,"y":0}],"edges":[{"id":"E1","tileIds":["T19","T18"]},{"id":"E2","tileIds":["T19","T17"]},{"id":"E3","tileIds":["T19","T16"]},{"id":"E4","tileIds":["T19","T15"]},{"id":"E5","tileIds":["T19","T14"]},{"id":"E6","tileIds":["T19","T13"]},{"id":"E7","tileIds":["T13","T12"]},{"id":"E8","tileIds":["T13","T18"]},{"id":"E9","tileIds":["T13","T14"]},{"id":"E10","tileIds":["T13","T2"]},{"id":"E11","tileIds":["T13","T1"]},{"id":"E12","playerId":"P2","direction":"NE","tileIds":["T1"]},{"id":"E13","playerId":"P2","tileIds":["T1","T12"]},{"id":"E14","tileIds":["T1","T2"]},{"id":"E15","direction":"W","tileIds":["T1"]},{"id":"E16","direction":"NW","tileIds":["T1"]},{"id":"E17","tileIds":["T2","T14"]},{"id":"E18","tileIds":["T2","T3"]},{"id":"E19","direction":"W","tileIds":["T2"]},{"id":"E20","direction":"NW","tileIds":["T2"]},{"id":"E21","tileIds":["T3","T14"]},{"id":"E22","tileIds":["T3","T4"]},{"id":"E23","direction":"SW","tileIds":["T3"]},{"id":"E24","direction":"W","tileIds":["T3"]},{"id":"E25","direction":"NW","tileIds":["T3"]},{"id":"E26","tileIds":["T4","T14"]},{"id":"E27","tileIds":["T4","T15"]},{"id":"E28","tileIds":["T4","T5"]},{"id":"E29","direction":"SW","tileIds":["T4"]},{"id":"E30","direction":"W","tileIds":["T4"]},{"id":"E31","tileIds":["T5","T15"]},{"id":"E32","tileIds":["T5","T6"]},{"id":"E33","direction":"SE","tileIds":["T5"]},{"id":"E34","direction":"SW","tileIds":["T5"]},{"id":"E35","direction":"W","tileIds":["T5"]},{"id":"E36","tileIds":["T6","T16"]},{"id":"E37","playerId":"P1","tileIds":["T6","T7"]},{"id":"E38","direction":"SE","tileIds":["T6"]},{"id":"E39","direction":"SW","tileIds":["T6"]},{"id":"E40","tileIds":["T6","T15"]},{"id":"E41","tileIds":["T15","T16"]},{"id":"E42","tileIds":["T15","T14"]},{"id":"E43","playerId":"P2","tileIds":["T16","T17"]},{"id":"E44","tileIds":["T16","T8"]},{"id":"E45","tileIds":["T16","T7"]},{"id":"E46","tileIds":["T7","T8"]},{"id":"E47","direction":"E","tileIds":["T7"]},{"id":"E48","direction":"SE","tileIds":["T7"]},{"id":"E49","playerId":"P1","direction":"SW","tileIds":["T7"]},{"id":"E50","playerId":"P2","tileIds":["T8","T9"]},{"id":"E51","direction":"E","tileIds":["T8"]},{"id":"E52","direction":"SE","tileIds":["T8"]},{"id":"E53","playerId":"P2","tileIds":["T8","T17"]},{"id":"E54","playerId":"P1","tileIds":["T17","T10"]},{"id":"E55","tileIds":["T17","T9"]},{"id":"E56","tileIds":["T17","T18"]},{"id":"E57","tileIds":["T18","T11"]},{"id":"E58","tileIds":["T18","T10"]},{"id":"E59","tileIds":["T18","T12"]},{"id":"E60","direction":"NE","tileIds":["T12"]},{"id":"E61","tileIds":["T12","T11"]},{"id":"E62","direction":"NW","tileIds":["T12"]},{"id":"E63","direction":"NE","tileIds":["T11"]},{"id":"E64","direction":"E","tileIds":["T11"]},{"id":"E65","tileIds":["T11","T10"]},{"id":"E66","direction":"NW","tileIds":["T11"]},{"id":"E67","direction":"NE","tileIds":["T10"]},{"id":"E68","direction":"E","tileIds":["T10"]},{"id":"E69","playerId":"P1","tileIds":["T10","T9"]},{"id":"E70","direction":"NE","tileIds":["T9"]},{"id":"E71","direction":"E","tileIds":["T9"]},{"id":"E72","direction":"SE","tileIds":["T9"]}],"vertices":[{"id":"V1","tileIds":["T19","T18","T13"]},{"id":"V2","building":"Blocked","tileIds":["T19","T17","T18"]},{"id":"V3","building":"City","playerId":"P2","tileIds":["T19","T16","T17"]},{"id":"V4","building":"Blocked","tileIds":["T19","T15","T16"]},{"id":"V5","tileIds":["T19","T14","T15"]},{"id":"V6","tileIds":["T19","T13","T14"]},{"id":"V7","building":"Settlement","playerId":"P2","tileIds":["T13","T12","T1"]},{"id":"V8","building":"Blocked","tileIds":["T13","T18","T12"]},{"id":"V9","tileIds":["T13","T2","T14"]},{"id":"V10","building":"Blocked","tileIds":["T13","T1","T2"]},{"id":"V11","building":"Settlement","playerId":"P2","direction":"N","tileIds":["T1"]},{"id":"V12","building":"Blocked","tileIds":["T1","T12"]},{"id":"V13","tileIds":["T1","T2"]},{"id":"V14","building":"Blocked","direction":"NW","tileIds":["T1"]},{"id":"V15","tileIds":["T2","T3","T14"]},{"id":"V16","tileIds":["T2","T3"]},{"id":"V17","direction":"NW","tileIds":["T2"]},{"id":"V18","tileIds":["T3","T4","T14"]},{"id":"V19","tileIds":["T3","T4"]},{"id":"V20","direction":"SW","tileIds":["T3"]},{"id":"V21","direction":"NW","tileIds":["T3"]},{"id":"V22","tileIds":["T4","T15","T14"]},{"id":"V23","tileIds":["T4","T5","T15"]},{"id":"V24","tileIds":["T4","T5"]},{"id":"V25","direction":"SW","tileIds":["T4"]},{"id":"V26","tileIds":["T5","T6","T15"]},{"id":"V27","tileIds":["T5","T6"]},{"id":"V28","direction":"S","tileIds":["T5"]},{"id":"V29","direction":"SW","tileIds":["T5"]},{"id":"V30","building":"Blocked","tileIds":["T6","T16","T15"]},{"id":"V31","building":"Settlement","playerId":"P1","tileIds":["T6","T7","T16"]},{"id":"V32","building":"Blocked","tileIds":["T6","T7"]},{"id":"V33","direction":"S","tileIds":["T6"]},{"id":"V34","building":"Blocked","tileIds":["T16","T8","T17"]},{"id":"V35","building":"Blocked","tileIds":["T16","T7","T8"]},{"id":"V36","tileIds":["T7","T8"]},{"id":"V37","direction":"SE","tileIds":["T7"]},{"id":"V38","direction":"S","tileIds":["T7"]},{"id":"V39","building":"Settlement","playerId":"P2","tileIds":["T8","T9","T17"]},{"id":"V40","building":"Blocked","tileIds":["T8","T9"]},{"id":"V41","direction":"SE","tileIds":["T8"]},{"id":"V42","building":"City","playerId":"P1","tileIds":["T17","T10","T18"]},{"id":"V43","building":"Blocked","tileIds":["T17","T9","T10"]},{"id":"V44","tileIds":["T18","T11","T12"]},{"id":"V45","building":"Blocked","tileIds":["T18","T10","T11"]},{"id":"V46","direction":"N","tileIds":["T12"]},{"id":"V47","tileIds":["T12","T11"]},{"id":"V48","direction":"N","tileIds":["T11"]},{"id":"V49","direction":"NE","tileIds":["T11"]},{"id":"V50","tileIds":["T11","T10"]},{"id":"V51","building":"Blocked","direction":"NE","tileIds":["T10"]},{"id":"V52","building":"Settlement","playerId":"P1","tileIds":["T10","T9"]},{"id":"V53","building":"Blocked","direction":"NE","tileIds":["T9"]},{"id":"V54","direction":"SE","tileIds":["T9"]}],"ports":[{"id":"R1","type":"Brick","vertices":["V51","V52"]},{"id":"R2","type":"ThreeToOne","vertices":["V54","V40"]},{"id":"R3","type":"Wood","vertices":["V37","V38"]},{"id":"R4","type":"Wool","vertices":["V33","V27"]},{"id":"R5","type":"Grain","vertices":["V29","V24"]},{"id":"R6","type":"ThreeToOne","vertices":["V20","V21"]},{"id":"R7","type":"Ore","vertices":["V13","V14"]},{"id":"R8","type":"ThreeToOne","vertices":["V11","V12"]},{"id":"R9","type":"ThreeToOne","vertices":["V48","V49"]}],"developmentCards":["Monopoly","Knight","Knight","Knight","Knight","VictoryPoint","RoadBuilding","Knight","Knight","RoadBuilding","Knight","VictoryPoint","YearOfPlenty","Knight","VictoryPoint","YearOfPlenty","Knight","VictoryPoint","Knight","Knight","Knight","Monopoly","VictoryPoint","Knight"],"undoState":[{"eventRecordId":126,"phase":{"currentPlayerId":"P2","phaseState":"PlaceRobber","endPlayerId":"P1","previousState":"BuildOrTrade","originalRobberTileId":"T2","waitingForRoll":false,"devCardPlayedThisRound":false}}],"eventRecord":[{"id":0,"playerId":"P1","action":"PlaceRobber","tileId":"T15"},{"id":1,"playerId":"P1","action":"InitialSetUp","playerLineup":["P1","P2"]},{"id":2,"playerId":"P1","action":"PlaceFirstSettlement","vertexId":"V42"},{"id":3,"playerId":"P1","action":"PlaceRoad","edgeId":"E54"},{"id":4,"playerId":"P2","action":"PlaceFirstSettlement","vertexId":"V3"},{"id":5,"playerId":"P2","action":"Undo","eventReversed":4},{"id":6,"playerId":"P2","action":"PlaceFirstSettlement","vertexId":"V7"},{"id":7,"playerId":"P2","action":"PlaceRoad","edgeId":"E13"},{"id":8,"playerId":"P2","action":"PlaceSecondSettlement","vertexId":"V3","resourcesReceived":{"Brick":1,"Wood":1,"Ore":1}},{"id":9,"playerId":"P2","action":"PlaceRoad","edgeId":"E43"},{"id":10,"playerId":"P1","action":"PlaceSecondSettlement","vertexId":"V31","resourcesReceived":{"Wood":2,"Ore":1}},{"id":11,"playerId":"P1","action":"PlaceRoad","edgeId":"E37"},{"id":12,"playerId":"P1","action":"RollDice","die1":4,"die2":3},{"id":13,"playerId":"P1","action":"PlaceRobber","tileId":"T3"},{"id":14,"playerId":"P2","action":"RollDice","die1":1,"die2":6},{"id":15,"playerId":"P2","action":"PlaceRobber","tileId":"T14"},{"id":16,"playerId":"P2","action":"PlaceRoad","edgeId":"E12"},{"id":17,"playerId":"P1","action":"RollDice","die1":4,"die2":3},{"id":18,"playerId":"P1","action":"PlaceRobber","tileId":"T3"},{"id":19,"playerId":"P2","action":"RollDice","die1":2,"die2":3},{"id":20,"playerId":"P2","action":"ReceivedResources","resourcesReceived":{"Wool":1,"Wood":1}},{"id":21,"playerId":"P1","action":"ReceivedResources","resourcesReceived":{"Wood":1}},{"id":22,"playerId":"P1","action":"RollDice","die1":6,"die2":3},{"id":23,"playerId":"P1","action":"ReceivedResources","resourcesReceived":{"Ore":1}},{"id":24,"playerId":"P2","action":"ReceivedResources","resourcesReceived":{"Grain":1}},{"id":25,"playerId":"P2","action":"RollDice","die1":4,"die2":4},{"id":26,"playerId":"P2","action":"BuyDevelopmentCard","developmentCard":"Knight"},{"id":27,"playerId":"P1","action":"RollDice","die1":6,"die2":1},{"id":28,"playerId":"P1","action":"PlaceRobber","tileId":"T4"},{"id":29,"playerId":"P2","action":"RollDice","die1":2,"die2":1},{"id":30,"playerId":"P1","action":"ReceivedResources","resourcesReceived":{"Brick":1}},{"id":31,"playerId":"P1","action":"RollDice","die1":3,"die2":6},{"id":32,"playerId":"P1","action":"ReceivedResources","resourcesReceived":{"Ore":1}},{"id":33,"playerId":"P2","action":"ReceivedResources","resourcesReceived":{"Grain":1}},{"id":34,"playerId":"P1","action":"PlaceRoad","edgeId":"E69"},{"id":35,"playerId":"P2","action":"RollDice","die1":1,"die2":6},{"id":36,"playerId":"P2","action":"PlaceRobber","tileId":"T3"},{"id":37,"playerId":"P1","action":"RollDice","die1":6,"die2":4},{"id":38,"playerId":"P1","action":"ReceivedResources","resourcesReceived":{"Wood":1}},{"id":39,"playerId":"P2","action":"ReceivedResources","resourcesReceived":{"Wool":1}},{"id":40,"playerId":"P2","action":"RollDice","die1":4,"die2":1},{"id":41,"playerId":"P2","action":"ReceivedResources","resourcesReceived":{"Wool":1,"Wood":1}},{"id":42,"playerId":"P1","action":"ReceivedResources","resourcesReceived":{"Wood":1}},{"id":43,"playerId":"P1","action":"RollDice","die1":4,"die2":3},{"id":44,"playerId":"P1","action":"PlaceRobber","tileId":"T14"},{"id":45,"playerId":"P2","action":"RollDice","die1":3,"die2":3},{"id":46,"playerId":"P2","action":"ReceivedResources","resourcesReceived":{"Ore":1}},{"id":47,"playerId":"P1","action":"ReceivedResources","resourcesReceived":{"Ore":1}},{"id":48,"playerId":"P1","action":"RollDice","die1":1,"die2":3},{"id":49,"playerId":"P1","action":"ReceivedResources","resourcesReceived":{"Grain":1}},{"id":50,"playerId":"P1","action":"TradeWithBank","resourcesUsed":{"Wood":4},"resourcesReceived":{"Brick":1}},{"id":51,"playerId":"P1","action":"Undo","eventReversed":50},{"id":52,"playerId":"P1","action":"TradeWithBank","resourcesUsed":{"Wood":4},"resourcesReceived":{"Ore":1}},{"id":53,"playerId":"P1","action":"Undo","eventReversed":52},{"id":54,"playerId":"P1","action":"TradeWithBank","resourcesUsed":{"Wood":4},"resourcesReceived":{"Grain":1}},{"id":55,"playerId":"P1","action":"UpgradeSettlement","vertexId":"V42"},{"id":56,"playerId":"P2","action":"RollDice","die1":4,"die2":1},{"id":57,"playerId":"P2","action":"ReceivedResources","resourcesReceived":{"Wool":1,"Wood":1}},{"id":58,"playerId":"P1","action":"ReceivedResources","resourcesReceived":{"Wood":1}},{"id":59,"playerId":"P1","action":"RollDice","die1":4,"die2":6},{"id":60,"playerId":"P1","action":"ReceivedResources","resourcesReceived":{"Wood":1}},{"id":61,"playerId":"P2","action":"ReceivedResources","resourcesReceived":{"Wool":1}},{"id":62,"playerId":"P2","action":"RollDice","die1":5,"die2":5},{"id":63,"playerId":"P1","action":"ReceivedResources","resourcesReceived":{"Wood":1}},{"id":64,"playerId":"P2","action":"ReceivedResources","resourcesReceived":{"Wool":1}},{"id":65,"playerId":"P2","action":"TradeWithBank","resourcesUsed":{"Wool":4},"resourcesReceived":{"Grain":1}},{"id":66,"playerId":"P2","action":"Undo","eventReversed":65},{"id":67,"playerId":"P2","action":"TradeWithBank","resourcesUsed":{"Wool":4},"resourcesReceived":{"Brick":1}},{"id":68,"playerId":"P2","action":"PlaceSettlement","vertexId":"V11"},{"id":69,"playerId":"P1","action":"RollDice","die1":1,"die2":5},{"id":70,"playerId":"P2","action":"ReceivedResources","resourcesReceived":{"Ore":1}},{"id":71,"playerId":"P1","action":"ReceivedResources","resourcesReceived":{"Ore":2}},{"id":72,"playerId":"P2","action":"RollDice","die1":1,"die2":5},{"id":73,"playerId":"P2","action":"ReceivedResources","resourcesReceived":{"Ore":1}},{"id":74,"playerId":"P1","action":"ReceivedResources","resourcesReceived":{"Ore":2}},{"id":75,"playerId":"P1","action":"RollDice","die1":3,"die2":3},{"id":76,"playerId":"P2","action":"ReceivedResources","resourcesReceived":{"Ore":1}},{"id":77,"playerId":"P1","action":"ReceivedResources","resourcesReceived":{"Ore":2}},{"id":78,"playerId":"P1","action":"TradeWithBank","resourcesUsed":{"Ore":4},"resourcesReceived":{"Wool":1}},{"id":79,"playerId":"P2","action":"RollDice","die1":2,"die2":3},{"id":80,"playerId":"P2","action":"ReceivedResources","resourcesReceived":{"Wool":2,"Wood":1}},{"id":81,"playerId":"P1","action":"ReceivedResources","resourcesReceived":{"Wood":1}},{"id":82,"playerId":"P2","action":"TradeWithBank","resourcesUsed":{"Ore":3},"resourcesReceived":{"Brick":1}},{"id":83,"playerId":"P2","action":"PlaceRoad","edgeId":"E53"},{"id":84,"playerId":"P1","action":"RollDice","die1":4,"die2":6},{"id":85,"playerId":"P1","action":"ReceivedResources","resourcesReceived":{"Wood":1}},{"id":86,"playerId":"P2","action":"ReceivedResources","resourcesReceived":{"Wool":1}},{"id":87,"playerId":"P1","action":"TradeWithBank","resourcesUsed":{"Wood":4},"resourcesReceived":{"Brick":1}},{"id":88,"playerId":"P2","action":"RollDice","die1":2,"die2":6},{"id":89,"playerId":"P1","action":"RollDice","die1":6,"die2":2},{"id":90,"playerId":"P2","action":"RollDice","die1":5,"die2":4},{"id":91,"playerId":"P1","action":"ReceivedResources","resourcesReceived":{"Ore":1}},{"id":92,"playerId":"P2","action":"ReceivedResources","resourcesReceived":{"Grain":1}},{"id":93,"playerId":"P1","action":"RollDice","die1":6,"die2":3},{"id":94,"playerId":"P1","action":"ReceivedResources","resourcesReceived":{"Ore":1}},{"id":95,"playerId":"P2","action":"ReceivedResources","resourcesReceived":{"Grain":1}},{"id":96,"playerId":"P1","action":"TradeWithBank","resourcesUsed":{"Ore":4},"resourcesReceived":{"Grain":1}},{"id":97,"playerId":"P1","action":"PlaceSettlement","vertexId":"V52"},{"id":98,"playerId":"P1","action":"Undo","eventReversed":97},{"id":99,"playerId":"P1","action":"PlaceSettlement","vertexId":"V52"},{"id":100,"playerId":"P2","action":"RollDice","die1":2,"die2":3},{"id":101,"playerId":"P2","action":"ReceivedResources","resourcesReceived":{"Wool":2,"Wood":1}},{"id":102,"playerId":"P1","action":"ReceivedResources","resourcesReceived":{"Wood":1}},{"id":103,"playerId":"P2","action":"TradeWithBank","resourcesUsed":{"Wood":3},"resourcesReceived":{"Ore":1}},{"id":104,"playerId":"P2","action":"TradeWithBank","resourcesUsed":{"Wool":3},"resourcesReceived":{"Ore":1}},{"id":105,"playerId":"P2","action":"UpgradeSettlement","vertexId":"V3"},{"id":106,"playerId":"P1","action":"RollDice","die1":2,"die2":1},{"id":107,"playerId":"P1","action":"ReceivedResources","resourcesReceived":{"Brick":2}},{"id":108,"playerId":"P2","action":"RollDice","die1":3,"die2":2},{"id":109,"playerId":"P2","action":"ReceivedResources","resourcesReceived":{"Wool":2,"Wood":2}},{"id":110,"playerId":"P1","action":"ReceivedResources","resourcesReceived":{"Wood":1}},{"id":111,"playerId":"P1","action":"RollDice","die1":6,"die2":5},{"id":112,"playerId":"P1","action":"ReceivedResources","resourcesReceived":{"Ore":1}},{"id":113,"playerId":"P2","action":"ReceivedResources","resourcesReceived":{"Brick":2}},{"id":114,"playerId":"P2","action":"RollDice","die1":5,"die2":3},{"id":115,"playerId":"P2","action":"TradeWithBank","resourcesUsed":{"Wool":3},"resourcesReceived":{"Grain":1}},{"id":116,"playerId":"P2","action":"PlaceSettlement","vertexId":"V39"},{"id":117,"playerId":"P2","action":"PlaceRoad","edgeId":"E50"},{"id":118,"playerId":"P1","action":"RollDice","die1":4,"die2":3},{"id":119,"playerId":"P1","action":"PlaceRobber","tileId":"T3"},{"id":120,"playerId":"P1","action":"PlaceRoad","edgeId":"E49"},{"id":121,"playerId":"P2","action":"RollDice","die1":1,"die2":6},{"id":122,"playerId":"P2","action":"PlaceRobber","tileId":"T2"},{"id":123,"playerId":"P1","action":"RollDice","die1":6,"die2":6},{"id":124,"playerId":"P2","action":"ReceivedResources","resourcesReceived":{"Wood":1}},{"id":125,"playerId":"P2","action":"RollDice","die1":6,"die2":1},{"id":126,"playerId":"P2","action":"PlaceRobber","tileId":"T11"}],"nextEventId":127}""";
        // var options = new JsonSerializerOptions
        // {
        //     PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        //     DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        //     Converters = { new JsonStringEnumConverter() }
        // };

        // var dto = JsonSerializer.Deserialize<DTOs.GameStateDTO>(json, options);
        // var gs = GamePlayHelpers.LoadAndPrepareGameStateDTO(dto);
        // var p1 = gs.Players.First(p => p.Id == "P2");
        // var possibleActions = PossiblePlayerActions.GetPossiblePlayerActions(gs, p1);
        // Assert.NotNull(possibleActions);
        // Assert.NotEmpty(possibleActions);

        // Assert.True(false);
    }
}
