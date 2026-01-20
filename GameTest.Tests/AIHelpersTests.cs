using Xunit;
using GameTest.Models;
using GameTest.Services;
using TH = GameTest.Tests.TestHelpers.SetUpPhaseTestReferences;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.VisualBasic;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.SignalR;
using System.Runtime.CompilerServices;

namespace GameTest.Tests;

public class AIHelpersTests
{
    [Fact]
    public void GetProbabilityForDiceRoll_TooLow()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => AIHelpers.GetProbabilityForDiceRoll(1));
    }

    [Fact]
    public void GetProbabilityForDiceRoll_TooHigh()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => AIHelpers.GetProbabilityForDiceRoll(13));
    }

    [Fact]
    public void GetProbabilityForDiceRoll_Roll6()
    {
        var probability = AIHelpers.GetProbabilityForDiceRoll(8);

        Assert.Equal(5.0 / 36.0, probability);
    }

    [Fact]
    public void GetBaseResourceAcquisitionRates_NoOwnedVertices_AllZero()
    {
        var gs = TestHelpers.CreateOriginalTestBoardWithSettlements().GetGameState();
        gs.AddPlayer(Player.CreateTestPlayer("Player3", PlayerColor.Red, isBot: false));
        
        Dictionary<ResourceType, double> baseRates = AIHelpers.GetBaseResourceAcquisitionRates(gs, gs.Players[2]);

        foreach (ResourceType rt in Enum.GetValues(typeof(ResourceType)))
        {
            if (rt == ResourceType.Desert)
                continue;

            Assert.Equal(0.0, baseRates[rt]);
        }
    }

    [Fact]
    public void GetBaseResourceAcquisitionRates_OwnedVertices_CorrectRates()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements();

        Dictionary<ResourceType, double> baseRates = AIHelpers.GetBaseResourceAcquisitionRates(board.GetGameState(), board.GetBluePlayer());

        Assert.Equal(0.0, baseRates[ResourceType.Brick]);
        Assert.Equal(0.0, baseRates[ResourceType.Wool]);
        Assert.Equal(AIHelpers.GetProbabilityForDiceRoll(9), baseRates[ResourceType.Grain]);
        Assert.Equal(AIHelpers.GetProbabilityForDiceRoll(3), baseRates[ResourceType.Ore]);
        Assert.Equal(AIHelpers.GetProbabilityForDiceRoll(6), baseRates[ResourceType.Wood]);
    }

    [Fact]
    public void GetSettlementToUpgrade_NoOwnedSettlements_Null()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements();
        board.GetGameState().Phase.CurrentPlayer = board.GetRedPlayer();
        board.GetVertex(TestVertex.V5).UpgradeToCity();

        var settlementToUpgrade = AIHelpers.GetSettlementToUpgrade(board.GetGameState());

        Assert.Null(settlementToUpgrade);
    }

    [Fact]
    public void GetSettlementToUpgrade_SelectHigherProducingSettlement()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements();
        board.GetGameState().Phase.CurrentPlayer = board.GetRedPlayer();
        board.GetRedPlayer().AssignResources(ResourceType.Ore, 3);
        board.GetRedPlayer().AssignResources(ResourceType.Grain, 2);
        board.GetVertex(TestVertex.V22).BuildSettlement(board.GetRedPlayer());

        var settlementToUpgrade = AIHelpers.GetSettlementToUpgrade(board.GetGameState());

        Assert.NotNull(settlementToUpgrade);
        Assert.Equal(board.GetVertex(TestVertex.V5).Id, settlementToUpgrade.Id);
    }

    [Fact]
    public void GetVertexReadyForSettlement_NoAvailableVertices_Null()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements();
        board.GetGameState().Phase.CurrentPlayer = board.GetRedPlayer();

        var vertexForSettlement = AIHelpers.GetVertexReadyForSettlement(board.GetGameState());

        Assert.Null(vertexForSettlement);
    }

    [Fact]
    public void GetVertexReadyForSettlement_ThreeOptions_BestSelected()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements();
        GamePlayHelpers.MarkBlockedVertices(board.GetGameState());

        board.GetGameState().Phase.CurrentPlayer = board.GetRedPlayer();

        board.GetEdge(TestEdge.E25).BuildRoad(board.GetRedPlayer());
        board.GetEdge(TestEdge.E24).BuildRoad(board.GetRedPlayer());
        board.GetEdge(TestEdge.E5).BuildRoad(board.GetRedPlayer());
        board.GetEdge(TestEdge.E6).BuildRoad(board.GetRedPlayer());

        var vertexForSettlement = AIHelpers.GetVertexReadyForSettlement(board.GetGameState());

        Assert.NotNull(vertexForSettlement);
        Assert.Equal(board.GetVertex(TestVertex.V1).Id, vertexForSettlement.Id);
    }

    private GameState CreateGameStateForOwnershipTesting()
    {
        var gs = TestHelpers.CreateGameStateForSetUpPhase();
        BoardCreationHelpers.LinkEdgesAndVertices(gs);
        var v1 = gs.GetVertexFromTileInfo(TH.DesertTile, TH.Wool2Tile, null, null);
        v1.BuildSettlement(TH.HumanPlayer);
        var v3 = gs.GetVertexFromTileInfo(TH.DesertTile, TH.GrainTile, TH.WoodTile, null);
        v3.BuildSettlement(TH.HumanPlayer);
        var v39 = gs.GetVertexFromTileInfo(TH.WoodTile, TH.OreTile, TH.Wool5Tile, null);
        v39.BuildSettlement(TH.HumanPlayer);
        var v52 = gs.GetVertexFromTileInfo(TH.BrickTile, TH.OreTile, null, null);
        v52.BuildSettlement(TH.HumanPlayer);

        return gs;
    }

    [Fact]
    public void GetAllOwnedBuildings_NoneFound()
    {
        // Arrange
        var gs = CreateGameStateForOwnershipTesting();
        var expectedVertex = gs.GetVertexFromTileInfo(TH.DesertTile, TH.Wool2Tile, null, null);

        // Act
        var vertices = AIHelpers.GetAllOwnedBuildings(gs, TH.HumanPlayer);

        // Assert
        Assert.Equal(4, vertices.Count);
        Assert.Contains(vertices, v => v.Id == expectedVertex.Id);
    }

    [Fact]
    public void GetAllOwnedBuildings_4Found()
    {
        // Arrange
        var gs = CreateGameStateForOwnershipTesting();        

        // Act
        var vertices = AIHelpers.GetAllOwnedBuildings(gs, TH.BotPlayer);

        // Assert
        Assert.Empty(vertices);
    }

    [Fact]
    public void GetRankedListOfVertexTargets_StartingFromAllOwned()
    {
        // TODO: Should change to Test Board
        GameState gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter, "UT");
        TestHelpers.AddPlayers(gs);

        gs.Players.Add(Player.CreateTestPlayer("Player3", PlayerColor.Green, isBot: false));
        Assert.Equal(PlayerColor.Blue, gs.Players[1].Color);
        gs.Phase.CurrentPlayer = gs.Players[1];

        // Get references to all tiles needed for test
        var t1 = gs.GetTileAt(-2, -2);
        var t2 = gs.GetTileAt(-3, -1);
        var t3 = gs.GetTileAt(-4, 0);
        var t4 = gs.GetTileAt(-3, 1);
        var t5 = gs.GetTileAt(-2, 2);
        var t6 = gs.GetTileAt(0, 2);
        var t7 = gs.GetTileAt(2, 2);
        var t8 = gs.GetTileAt(3, 1);
        var t9 = gs.GetTileAt(4, 0);
        var t10 = gs.GetTileAt(3, -1);
        var t12 = gs.GetTileAt(0, -2);
        var t13 = gs.GetTileAt(-1, -1);
        var t14 = gs.GetTileAt(-2, 0);
        var t15 = gs.GetTileAt(-1, 1);
        var t16 = gs.GetTileAt(1, 1);
        var t17 = gs.GetTileAt(2, 0);
        var t18 = gs.GetTileAt(1, -1);
        var t19 = gs.GetTileAt(0, 0);

        // Place green opponent pieces (blocking)
        gs.GetVertexFromTileInfo(t3, null, null, VertexDirection.SW).BuildSettlement(gs.Players[2]);
        gs.GetEdgeFromTileInfo(t3, null, HexDirection.W).BuildRoad(gs.Players[2]);
        gs.GetEdgeFromTileInfo(t3, null, HexDirection.NW).BuildRoad(gs.Players[2]);
        gs.GetVertexFromTileInfo(t12, t13, t18, null).BuildSettlement(gs.Players[2]);
        gs.GetVertexFromTileInfo(t1, t13, t2, null).BuildSettlement(gs.Players[2]);
        gs.GetEdgeFromTileInfo(t2, null, HexDirection.NW).BuildRoad(gs.Players[2]);
        gs.GetEdgeFromTileInfo(t2, t1, null).BuildRoad(gs.Players[2]);
        gs.GetEdgeFromTileInfo(t1, t13, null).BuildRoad(gs.Players[2]);
        gs.GetEdgeFromTileInfo(t12, t13, null).BuildRoad(gs.Players[2]);

        // Place red opponent pieces (blocking)
        gs.GetVertexFromTileInfo(t9, t10, t17, null).BuildSettlement(gs.Players[0]);
        gs.GetEdgeFromTileInfo(t18, t10, null).BuildRoad(gs.Players[0]);
        gs.GetEdgeFromTileInfo(t17, t10, null).BuildRoad(gs.Players[0]);
        gs.GetEdgeFromTileInfo(t17, t9, null).BuildRoad(gs.Players[0]);
        gs.GetEdgeFromTileInfo(t17, t8, null).BuildRoad(gs.Players[0]);
        gs.GetVertexFromTileInfo(t7, t8, null, null).BuildSettlement(gs.Players[0]);
        gs.GetVertexFromTileInfo(t16, t6, t7, null).BuildSettlement(gs.Players[0]);
        gs.GetEdgeFromTileInfo(t7, t8, null).BuildRoad(gs.Players[0]);
        gs.GetEdgeFromTileInfo(t7, t16, null).BuildRoad(gs.Players[0]);
        gs.GetEdgeFromTileInfo(t6, t7, null).BuildRoad(gs.Players[0]);

        // Place pre-existing blue (current-player) pieces
        gs.GetVertexFromTileInfo(t13, t14, t19, null).BuildSettlement(gs.Players[1]);
        gs.GetEdgeFromTileInfo(t14, t19, null).BuildRoad(gs.Players[1]);
        gs.GetEdgeFromTileInfo(t15, t19, null).BuildRoad(gs.Players[1]);
        gs.GetVertexFromTileInfo(t4, t5, null, null).BuildSettlement(gs.Players[1]);
        gs.GetVertexFromTileInfo(t15, t6, t5, null).BuildSettlement(gs.Players[1]);
        gs.GetEdgeFromTileInfo(t4, t5, null).BuildRoad(gs.Players[1]);
        gs.GetEdgeFromTileInfo(t15, t5, null).BuildRoad(gs.Players[1]);

        GamePlayHelpers.MarkBlockedVertices(gs);
        BoardCreationHelpers.LinkEdgesAndVertices(gs);

        var rankedGoals = AIHelpers.GetRankedListOfVertexTargets(gs, AIHelpers.GetAllOwnedBuildings(gs, gs.Phase.CurrentPlayer));

        Assert.Equal(12, rankedGoals.Count);
        foreach (var goal in rankedGoals)
        {
            Assert.Null(goal.TargetVertex.Building);
        }
        
        var v16 = gs.GetVertexFromTileInfo(t2, t3, null, null);
        var g16 = rankedGoals.First(g => g.TargetVertex.Id == v16.Id);
        Assert.NotNull(g16);
        Assert.Equal(3, g16.RoadsNeeded);

        var v17 = gs.GetVertexFromTileInfo(t2, null, null, VertexDirection.NW);
        var g17 = rankedGoals.First(g => g.TargetVertex.Id == v17.Id);
        Assert.NotNull(g17);
        Assert.Equal(4, g17.RoadsNeeded);
        Assert.True(g16.OverallScore > g17.OverallScore); // g17 needs more roads and provide less resources

        var v15 = gs.GetVertexFromTileInfo(t2, t3, t14, null);
        var g15 = rankedGoals.First(g => g.TargetVertex.Id == v15.Id);
        Assert.NotNull(g15);
        Assert.Equal(2, g15.RoadsNeeded);
        Assert.True(g15.OverallScore > g16.OverallScore); // g16 needs more roads and provides less resources

        var v18 = gs.GetVertexFromTileInfo(t3, t14, t4, null);
        var g18 = rankedGoals.First(g => g.TargetVertex.Id == v18.Id);
        Assert.NotNull(g18);
        Assert.Equal(2, g18.RoadsNeeded);


        var v22 = gs.GetVertexFromTileInfo(t14, t4, t15, null);
        var g22 = rankedGoals.First(g => g.TargetVertex.Id == v22.Id);
        Assert.NotNull(g22);
        Assert.Equal(1, g22.RoadsNeeded);
        Assert.True(g22.OverallScore > g18.OverallScore); // g18 needs more roads and provides less valuable resources

        var v2 = gs.GetVertexFromTileInfo(t18, t19, t17, null);
        var g2 = rankedGoals.First(g => g.TargetVertex.Id == v2.Id);
        Assert.NotNull(g2);
        Assert.Equal(2, g2.RoadsNeeded);

        var v3 = gs.GetVertexFromTileInfo(t19, t17, t16, null);
        var g3 = rankedGoals.First(g => g.TargetVertex.Id == v3.Id);
        Assert.NotNull(g3);
        Assert.Equal(1, g3.RoadsNeeded);

        var v4 = gs.GetVertexFromTileInfo(t19, t15, t16, null);
        var g4 = rankedGoals.First(g => g.TargetVertex.Id == v4.Id);
        Assert.NotNull(g4);
        Assert.Equal(0, g4.RoadsNeeded);

        var v34 = gs.GetVertexFromTileInfo(t17, t16, t8, null);
        var g34 = rankedGoals.First(g => g.TargetVertex.Id == v34.Id);
        Assert.NotNull(g34);
        Assert.Equal(2, g34.RoadsNeeded);

        var v28 = gs.GetVertexFromTileInfo(t5, null, null, VertexDirection.S);
        var g28 = rankedGoals.First(g => g.TargetVertex.Id == v28.Id);
        Assert.NotNull(g28);
        Assert.Equal(2, g28.RoadsNeeded);

        var v33 = gs.GetVertexFromTileInfo(t6, null, null, VertexDirection.S);
        var g33 = rankedGoals.First(g => g.TargetVertex.Id == v33.Id);
        Assert.NotNull(g33);
        Assert.Equal(2, g33.RoadsNeeded);

        var v38 = gs.GetVertexFromTileInfo(t7, null, null, VertexDirection.S);
        var g38 = rankedGoals.First(g => g.TargetVertex.Id == v38.Id);
        Assert.NotNull(g38);
        Assert.Equal(4, g38.RoadsNeeded);
        Assert.True(g38.OverallScore < g28.OverallScore && g38.OverallScore < g33.OverallScore);
    }

    [Fact]
    public void GetRankedListOfVertexTargets_StartingFromSingleVertex()
    {
        // Create board state where HumanPlayer has built settlements that divide the board in half and
        // BotPlayer has a settlement in both halves.
        var gs = CreateGameStateForOwnershipTesting();
        var v35 = gs.GetVertexFromTileInfo(TH.GrainTile, TH.Wool5Tile, null, null);
        v35.BuildSettlement(TH.BotPlayer);
        var e44 = gs.GetEdgeFromTileInfo(TH.GrainTile, TH.Wool5Tile, null);
        e44.BuildRoad(TH.BotPlayer);
        var v42 = gs.GetVertexFromTileInfo(TH.Wool2Tile, TH.WoodTile, TH.BrickTile, null);
        v42.BuildSettlement(TH.BotPlayer);
        GamePlayHelpers.MarkBlockedVertices(gs);
        gs.Phase.CurrentPlayer = TH.BotPlayer;
        gs.Phase.PhaseState = GameStates.PlaceSecondRoad;

        var expectedEdge = gs.GetEdgeFromTileInfo(TH.Wool2Tile, TH.BrickTile, null);

        // Make sure when we ask the AI to pick an edge from V42, it ranks the the vertices to show preference
        // for the road going away (North) from HumanPlayer's effective blockaid that splits the board.
        var rankedGoals = AIHelpers.GetRankedListOfVertexTargets(gs, new List<Vertex> { v42 });

        Assert.Equal(2, rankedGoals.Count);
        foreach (var goal in rankedGoals)
        {
            Assert.Null(goal.TargetVertex.Building);
            Assert.NotNull(goal.NextEdgeToTarget);
            Assert.Equal(expectedEdge.Id, goal.NextEdgeToTarget.Id);
        }
    }

    [Fact]
    public void GetRankedListOfVertexTargets_PlayerRoadsIntersect()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var orangePlayer = Player.CreateTestPlayer("WallE", PlayerColor.Orange, true);
        var gs = board.GetGameState();
        gs.Players.Add(orangePlayer);
        board.GetVertex(TestVertex.V16).BuildSettlement(orangePlayer);
        board.GetEdge(TestEdge.E10).BuildRoad(orangePlayer);
        board.GetEdge(TestEdge.E4).BuildRoad(orangePlayer);
        board.GetVertex(TestVertex.V1).BuildSettlement(orangePlayer);
        board.GetEdge(TestEdge.E1).BuildRoad(orangePlayer);
        board.GetEdge(TestEdge.E2).BuildRoad(board.GetBluePlayer());
        board.GetVertex(TestVertex.V18).BuildSettlement(board.GetBluePlayer());
        board.GetEdge(TestEdge.E24).BuildRoad(board.GetBluePlayer());
        board.GetEdge(TestEdge.E25).BuildRoad(board.GetRedPlayer());
        board.GetVertex(TestVertex.V10).BuildSettlement(board.GetRedPlayer());
        board.GetEdge(TestEdge.E16).BuildRoad(board.GetRedPlayer());

        GamePlayHelpers.MarkBlockedVertices(gs);
        gs.Phase.CurrentPlayer = board.GetBluePlayer();
        gs.Phase.PhaseState = GameStates.BuildOrTrade;

        var rankedGoals = AIHelpers.GetRankedListOfVertexTargets(gs, new List<Vertex> { board.GetVertex(TestVertex.V3), board.GetVertex(TestVertex.V18) });

        Assert.Equal(2, rankedGoals.Count);
        Assert.Contains(rankedGoals, g => g.TargetVertex == board.GetVertex(TestVertex.V12));
        Assert.Contains(rankedGoals, g => g.TargetVertex == board.GetVertex(TestVertex.V14));
    }


    [Fact]
    public void CalculateResourcesNeededForRoad_NeedAll()
    {
        var owned = new Dictionary<ResourceType, int>()
        {
            {ResourceType.Wool, 2},
            {ResourceType.Ore, 1}
        };

        var needed = AIHelpers.CalculateResourcesNeededForRoad(owned);

        Assert.Equal(2, needed.Count);
        Assert.Contains(ResourceType.Wood, needed.Keys);
        Assert.Equal(1, needed[ResourceType.Wood]);
        Assert.Contains(ResourceType.Brick, needed.Keys);
        Assert.Equal(1, needed[ResourceType.Brick]);
    }

    [Fact]
    public void CalculateResourcesNeededForRoad_NeedNone()
    {
        var owned = new Dictionary<ResourceType, int>()
        {
            {ResourceType.Wood, 2},
            {ResourceType.Brick, 1}
        };

        var needed = AIHelpers.CalculateResourcesNeededForRoad(owned);

        Assert.Empty(needed);
    }

    [Fact]
    public void CalculateResourcesNeededForRoad_NeedOne()
    {
        var owned = new Dictionary<ResourceType, int>()
        {
            {ResourceType.Wood, 2},
            {ResourceType.Ore, 1}
        };

        var needed = AIHelpers.CalculateResourcesNeededForRoad(owned);

        Assert.Single(needed);
        Assert.Contains(ResourceType.Brick, needed.Keys);
        Assert.Equal(1, needed[ResourceType.Brick]);
    }

    [Fact]
    public void CalculateResourcesNeededForSettlementNeedAll()
    {
        var owned = new Dictionary<ResourceType, int>();

        var needed = AIHelpers.CalculateResourcesNeededForSettlement(owned);

        Assert.Equal(4, needed.Count);
        Assert.Contains(ResourceType.Wood, needed.Keys);
        Assert.Equal(1, needed[ResourceType.Wood]);
        Assert.Contains(ResourceType.Brick, needed.Keys);
        Assert.Equal(1, needed[ResourceType.Brick]);
        Assert.Contains(ResourceType.Wool, needed.Keys);
        Assert.Equal(1, needed[ResourceType.Wool]);
        Assert.Contains(ResourceType.Grain, needed.Keys);
        Assert.Equal(1, needed[ResourceType.Grain]);
    }

    [Fact]
    public void CalculateResourcesNeededForSettlement_NeedNone()
    {
        var owned = new Dictionary<ResourceType, int>()
        {
            {ResourceType.Wood, 2},
            {ResourceType.Grain, 4},
            {ResourceType.Wool, 1},
            {ResourceType.Ore, 2},
            {ResourceType.Brick, 1}
        };

        var needed = AIHelpers.CalculateResourcesNeededForSettlement(owned);

        Assert.Empty(needed);
    }

    [Fact]
    public void CalculateResourcesNeededForSettlement_NeedOne()
    {
        var owned = new Dictionary<ResourceType, int>()
        {
            {ResourceType.Wood, 2},
            {ResourceType.Grain, 4},
            {ResourceType.Ore, 2},
            {ResourceType.Brick, 1}
        };

        var needed = AIHelpers.CalculateResourcesNeededForSettlement(owned);

        Assert.Single(needed);
        Assert.Contains(ResourceType.Wool, needed.Keys);
        Assert.Equal(1, needed[ResourceType.Wool]);
    }

    [Fact]
    public void CalculateResourcesNeededForSettlement_NeedMultiple()
    {
        var owned = new Dictionary<ResourceType, int>()
        {
            {ResourceType.Wood, 2},
            {ResourceType.Grain, 0},
            {ResourceType.Ore, 2},
            {ResourceType.Brick, 1}
        };

        var needed = AIHelpers.CalculateResourcesNeededForSettlement(owned);

        Assert.Equal(2, needed.Count);
        Assert.Contains(ResourceType.Wool, needed.Keys);
        Assert.Equal(1, needed[ResourceType.Wool]);
        Assert.Contains(ResourceType.Grain, needed.Keys);
        Assert.Equal(1, needed[ResourceType.Grain]);
    }

    [Fact]
    public void CalculateResourcesNeededForCityNeedAll()
    {
        var owned = new Dictionary<ResourceType, int>()
        {
            {ResourceType.Wood, 2},
            {ResourceType.Brick, 1}
        };

        var needed = AIHelpers.CalculateResourcesNeededForCity(owned);

        Assert.Equal(2, needed.Count);
        Assert.Contains(ResourceType.Ore, needed.Keys);
        Assert.Equal(3, needed[ResourceType.Ore]);
        Assert.Contains(ResourceType.Grain, needed.Keys);
        Assert.Equal(2, needed[ResourceType.Grain]);
    }

    [Fact]
    public void CalculateResourcesNeededForCity_NeedNone()
    {
        var owned = new Dictionary<ResourceType, int>()
        {
            {ResourceType.Wood, 2},
            {ResourceType.Ore, 5},
            {ResourceType.Grain, 2}
        };

        var needed = AIHelpers.CalculateResourcesNeededForCity(owned);

        Assert.Empty(needed);
    }

    [Fact]
    public void CalculateResourcesNeededForCity_NeedOne()
    {
        var owned = new Dictionary<ResourceType, int>()
        {
            {ResourceType.Ore, 2},
            {ResourceType.Grain, 2}
        };

        var needed = AIHelpers.CalculateResourcesNeededForCity(owned);

        Assert.Single(needed);
        Assert.Contains(ResourceType.Ore, needed.Keys);
        Assert.Equal(1, needed[ResourceType.Ore]);
    }

    [Fact]
    public void CalculateResourcesNeededForCity_NeedMultiple()
    {
        var owned = new Dictionary<ResourceType, int>()
        {
            {ResourceType.Ore, 1},
            {ResourceType.Grain, 2}
        };

        var needed = AIHelpers.CalculateResourcesNeededForCity(owned);

        Assert.Single(needed);
        Assert.Contains(ResourceType.Ore, needed.Keys);
        Assert.Equal(2, needed[ResourceType.Ore]);
    }

    [Fact]
    public void GetAIResourceAcquisitionScore_NoPortBonusIfBelowThreshold()
    {
        var belowMinimumRate = 1/72;
        var scoreWithPort = AIHelpers.GetAIResourceAcquisitionScore(belowMinimumRate, ResourceType.Ore, true);
        var scoreWithoutPort = AIHelpers.GetAIResourceAcquisitionScore(belowMinimumRate, ResourceType.Ore, false);

        Assert.Equal(scoreWithPort, scoreWithoutPort);
    }

    [Fact]
    public void GetAIResourceAcquisitionScore_PortBonusIfAboveThreshold()
    {
        var minimumRate = 2.0/36.0;
        var scoreWithPort = AIHelpers.GetAIResourceAcquisitionScore(minimumRate, ResourceType.Ore, true);
        var scoreWithoutPort = AIHelpers.GetAIResourceAcquisitionScore(minimumRate, ResourceType.Ore, false);

        Assert.True(scoreWithPort > scoreWithoutPort);
    }

    [Fact]
    public void GetAIResourceAcquisitionScore_OreGrainGreaterThanWoodWool()
    {
        var sameRateForAll = 4.0/36.0;
        var oreScore = AIHelpers.GetAIResourceAcquisitionScore(sameRateForAll, ResourceType.Ore, false);
        var grainScore = AIHelpers.GetAIResourceAcquisitionScore(sameRateForAll, ResourceType.Grain, false);
        var woodScore = AIHelpers.GetAIResourceAcquisitionScore(sameRateForAll, ResourceType.Wood, false);
        var woolScore = AIHelpers.GetAIResourceAcquisitionScore(sameRateForAll, ResourceType.Wool, false);
        
        Assert.True(oreScore > woodScore);
        Assert.True(oreScore > woolScore);
        Assert.True(grainScore > woodScore);
        Assert.True(grainScore > woolScore);
    }

    private static GameState CreateGameForPickRobberTargetTests()
    {
        // TODO: Should change to Test Board
        GameState gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter, "UT");
        TestHelpers.AddPlayers(gs);


        var wood9Tile = gs.GetTileAt(2, -2);
        var brick10Tile = gs.GetTileAt(3, -1);
        var wool4Tile = gs.GetTileAt(1, -1);
        var wood3Tile = gs.GetTileAt(2, 0);
        var ore8Tile = gs.GetTileAt(4, 0);

        var human = gs.Players.First(p => !p.IsBot);
        var bot = gs.Players.First(p => p.IsBot);
        gs.Phase.CurrentPlayer = bot;
        gs.Phase.PhaseState = GameStates.RollOrUseDevCard;

        var v1 = gs.GetVertexFromTileInfo(wood9Tile, brick10Tile, wool4Tile, null);
        v1.BuildSettlement(human);
        var v2 = gs.GetVertexFromTileInfo(brick10Tile, wood3Tile, ore8Tile, null);
        v2.BuildSettlement(human);

        return gs;
    }

    [Fact]
    public void PickTargetForRobber_OreBest()
    {
        var gs = CreateGameForPickRobberTargetTests();
        var bot = gs.Players.First(p => p.IsBot);

        var target = AIHelpers.PickTargetForRobber(gs, bot);

        var ore8Tile = gs.GetTileAt(4, 0);

        Assert.NotNull(target);
        Assert.Equal(ore8Tile.Id, target.Id);
    }

    [Fact]
    public void PickTargetForRobber_CityMakesBrickBest()
    {
        var gs = CreateGameForPickRobberTargetTests();
        var human = gs.Players.First(p => !p.IsBot);
        var bot = gs.Players.First(p => p.IsBot);

        var wood9Tile = gs.GetTileAt(2, -2);
        var brick10Tile = gs.GetTileAt(3, -1);

        var v1 = gs.Vertices.First(v => v.Owner != null && v.Owner.Id == human.Id && v.Tiles.Contains(wood9Tile));
        v1.UpgradeToCity();

        var target = AIHelpers.PickTargetForRobber(gs, bot);

        Assert.NotNull(target);
        Assert.Equal(brick10Tile.Id, target.Id);
    }

    [Fact]
    public void PickTargetForRobber_OreBestButBlocked()
    {
        var gs = CreateGameForPickRobberTargetTests();

        var brick10Tile = gs.GetTileAt(3, -1);
        var ore8Tile = gs.GetTileAt(4, 0);

        var human = gs.Players.First(p => !p.IsBot);
        var bot = gs.Players.First(p => p.IsBot);

        var v1 = gs.GetVertexFromTileInfo(ore8Tile, null, null, VertexDirection.NE);
        v1.BuildSettlement(bot);

        var target = AIHelpers.PickTargetForRobber(gs, bot);

        Assert.NotNull(target);
        Assert.Equal(brick10Tile.Id, target.Id);
    }

    [Fact]
    public void PickTargetForRobber_BrickBlockedAndRobberOnOre()
    {
        var gs = CreateGameForPickRobberTargetTests();

        var brick10Tile = gs.GetTileAt(3, -1);
        var human = gs.Players.First(p => !p.IsBot);
        var bot = gs.Players.First(p => p.IsBot);

        var v1 = gs.GetVertexFromTileInfo(brick10Tile, null, null, VertexDirection.NE);
        v1.BuildSettlement(bot);

        var ore8Tile = gs.GetTileAt(4, 0);
        gs.SetRobberTile(ore8Tile);

        var wood9Tile = gs.GetTileAt(2, -2);

        var target = AIHelpers.PickTargetForRobber(gs, bot);

        Assert.NotNull(target);
        Assert.Equal(wood9Tile.Id, target.Id);
    }

    [Fact]
    public void PickTargetForRobber_PickRandomIfNoOtherOption()
    {
        // TODO: Should change to Test Board
        GameState gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter, "UT");
        TestHelpers.AddPlayers(gs);

        var originalRobberTile = gs.RobberTile;

        var wood9Tile = gs.GetTileAt(2, -2);
        var brick10Tile = gs.GetTileAt(3, -1);
        var wool4Tile = gs.GetTileAt(1, -1);
        var wood3Tile = gs.GetTileAt(2, 0);
        var ore8Tile = gs.GetTileAt(4, 0);

        var human = gs.Players.First(p => !p.IsBot);
        var bot = gs.Players.First(p => p.IsBot);

        gs.Phase.CurrentPlayer = bot;
        gs.Phase.PhaseState = GameStates.RollOrUseDevCard;


        var v1 = gs.GetVertexFromTileInfo(wood9Tile, brick10Tile, wool4Tile, null);
        v1.BuildSettlement(bot);
        var v2 = gs.GetVertexFromTileInfo(brick10Tile, wood3Tile, ore8Tile, null);
        v2.BuildSettlement(bot);

        var v3 = gs.GetVertexFromTileInfo(brick10Tile, null, null, VertexDirection.NE);
        v3.BuildSettlement(human);
        var v4 = gs.GetVertexFromTileInfo(ore8Tile, null, null, VertexDirection.NE);
        v4.BuildSettlement(human);

        var target = AIHelpers.PickTargetForRobber(gs, human);

        Assert.NotNull(target);
        Assert.True(target.Id != brick10Tile.Id && target.Id != ore8Tile.Id && target.Id != originalRobberTile.Id);
    }

    [Fact]
    public void GetResourceProbabilityForTile_PlayerNotOnTile()
    {
        var gs = CreateGameForPickRobberTargetTests();
        var brick10Tile = gs.GetTileAt(3, -1);
        var bot = gs.Players.First(p => p.IsBot);

        var value = AIHelpers.GetResourcePayoutValueForTile(gs, brick10Tile, bot);

        Assert.Equal(0.0, value);
    }

    [Fact]
    public void GetResourceProbabilityForTile_SingleSettlement()
    {
        var gs = CreateGameForPickRobberTargetTests();
        var ore8Tile = gs.GetTileAt(4, 0);
        var brick10Tile = gs.GetTileAt(3, -1);

        var v1 = gs.GetVertexFromTileInfo(ore8Tile, brick10Tile, null, null);
        var bot = gs.Players.First(p => p.IsBot);
        v1.BuildSettlement(bot);

        var value = AIHelpers.GetResourcePayoutValueForTile(gs, ore8Tile, bot);

        Assert.Equal(1.2 * 5.0/36.0, value);
    }

    [Fact]
    public void GetResourceProbabilityForTile_SettlementAndCity()
    {
        var gs = CreateGameForPickRobberTargetTests();
        var brick10Tile = gs.GetTileAt(3, -1);
        var human = gs.Players.First(p => !p.IsBot);
        var v1 = gs.Vertices.First(v => v.Tiles.Contains(brick10Tile) && v.Owner != null && v.Owner.Id == human.Id);
        v1.UpgradeToCity();

        var value = AIHelpers.GetResourcePayoutValueForTile(gs, brick10Tile, human);

        Assert.Equal(3.0/36.0 * 3, value);
    }

    [Fact]
    public void MultiSetSubtraction_NoOverlap_FullFirstReturned()
    {
        var first = new List<ResourceType>() { ResourceType.Brick, ResourceType.Wood };
        var second = new List<ResourceType>() { ResourceType.Ore, ResourceType.Grain };

        var result = AIHelpers.MultiSetSubtraction(first, second);

        Assert.Equal(2, result.Count);
        Assert.Contains(ResourceType.Brick, result);
        Assert.Contains(ResourceType.Wood, result);
    }

    [Fact]
    public void MultiSetSubtraction_SameList_EmptyReturned()
    {
        var first = new List<ResourceType>() { ResourceType.Brick, ResourceType.Wood, ResourceType.Wood };
        var second = new List<ResourceType>() { ResourceType.Wood, ResourceType.Brick, ResourceType.Wood };

        var result = AIHelpers.MultiSetSubtraction(first, second);

        Assert.Empty(result);
    }

    [Fact]
    public void MultiSetSubtraction_MultiOfOne_OnlyRemoveNumberFromB()
    {
        var first = new List<ResourceType>() { ResourceType.Brick, ResourceType.Wood, ResourceType.Wood, ResourceType.Wood };
        var second = new List<ResourceType>() { ResourceType.Wood, ResourceType.Wood };

        var result = AIHelpers.MultiSetSubtraction(first, second);

        Assert.Equal(2, result.Count);
        Assert.Contains(ResourceType.Brick, result);
        Assert.Contains(ResourceType.Wood, result);
    }

    [Fact]
    public void ConvertResourceDictToList_CorrectCounts()
    {
        var resourceDict = new Dictionary<ResourceType, int>()
        {
            { ResourceType.Brick, 2 },
            { ResourceType.Wood, 1 },
            { ResourceType.Ore, 0 },
            { ResourceType.Grain, 3 },
            { ResourceType.Wool, 1 }
        };

        var result = AIHelpers.ConvertResourceDictToList(resourceDict);

        Assert.Equal(7, result.Count);
        Assert.Equal(2, result.Count(r => r == ResourceType.Brick));
        Assert.Equal(1, result.Count(r => r == ResourceType.Wood));
        Assert.Equal(0, result.Count(r => r == ResourceType.Ore));
        Assert.Equal(3, result.Count(r => r == ResourceType.Grain));
        Assert.Equal(1, result.Count(r => r == ResourceType.Wool));
    }

    [Fact]
    public void ShouldBuyDevelopmentCard_No_CanBuildSettlement()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var bot = board.GetBluePlayer();
        board.GetEdge(TestEdge.E10).BuildRoad(bot);
        bot.AssignResources(ResourceType.Wood, 1);
        bot.AssignResources(ResourceType.Wool, 1);
        bot.AssignResources(ResourceType.Brick, 1);
        bot.AssignResources(ResourceType.Grain, 1);

        board.GetGameState().Phase = new GamePhase(GameStates.BuildOrTrade, board.GetBluePlayer());

        var result = AIHelpers.ShouldBuyDevelopmentCard(board.GetGameState(), bot, true);

        Assert.True(result <= 0.2);
    }

    [Fact]
    public void ShouldBuyDevelopmentCard_No_CanUpgradeSettlement()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var bot = board.GetBluePlayer();
        bot.AssignResources(ResourceType.Ore, 3);
        bot.AssignResources(ResourceType.Wool, 1);
        bot.AssignResources(ResourceType.Grain, 2);

        board.GetGameState().Phase = new GamePhase(GameStates.BuildOrTrade, board.GetBluePlayer());

        var result = AIHelpers.ShouldBuyDevelopmentCard(board.GetGameState(), bot);

        Assert.True(result <= 0.2);
    }

    [Fact]
    public void ShouldBuyDevelopmentCard_Yes_Over7CardsAndCantBuildAnythingElse()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var bot = board.GetBluePlayer();
        bot.AssignResources(ResourceType.Ore, 1);
        bot.AssignResources(ResourceType.Wool, 3);
        bot.AssignResources(ResourceType.Grain, 1);
        bot.AssignResources(ResourceType.Brick, 3);

        board.GetGameState().Phase = new GamePhase(GameStates.BuildOrTrade, board.GetBluePlayer());

        var result = AIHelpers.ShouldBuyDevelopmentCard(board.GetGameState(), bot);

        Assert.True(result >= 0.8);
    }

    [Fact]
    public void ShouldBuyDevelopmentCard_Yes_NoMoreCitiesAndNowhereToBuild()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var bot = board.GetBluePlayer();
        board.GetVertex(TestVertex.V3).UpgradeToCity();
        board.GetEdge(TestEdge.E10).BuildRoad(bot);
        board.GetVertex(TestVertex.V16).BuildSettlement(bot);
        board.GetVertex(TestVertex.V16).UpgradeToCity();
        bot.AssignResources(ResourceType.Wool, 1);
        bot.AssignResources(ResourceType.Ore, 1);
        bot.AssignResources(ResourceType.Grain, 1);

        board.GetGameState().Phase = new GamePhase(GameStates.BuildOrTrade, board.GetBluePlayer());

        var result = AIHelpers.ShouldBuyDevelopmentCard(board.GetGameState(), bot);

        Assert.True(result >= 0.8);
    }

    [Fact]
    public void ShouldBuyDevelopmentCard_Yes_GoingForLargestArmyAndGameAlmostOver()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var bot = board.GetBluePlayer();
        var human = board.GetRedPlayer();
        board.GetVertex(TestVertex.V5).UpgradeToCity();
        board.GetVertex(TestVertex.V7).BuildSettlement(human);

        board.GetVertex(TestVertex.V3).UpgradeToCity();
        board.GetEdge(TestEdge.E10).BuildRoad(bot);
        bot.AssignResources(ResourceType.Wool, 1);
        bot.AssignResources(ResourceType.Ore, 1);
        bot.AssignResources(ResourceType.Grain, 1);
        bot.AssignResources(ResourceType.Brick, 1);
        bot.AssignResources(ResourceType.Wood, 1);
        bot.AssignDevelopmentCard(DevelopmentCardType.Knight);
        bot.AssignDevelopmentCard(DevelopmentCardType.Knight);
        bot.MakeNewDevelopmentCardsPlayable();
        bot.PlayDevelopmentCard(DevelopmentCardType.Knight);
        bot.PlayDevelopmentCard(DevelopmentCardType.Knight);
        board.GetGameState().Phase = new GamePhase(GameStates.BuildOrTrade, board.GetBluePlayer());
        board.GetGameState().UpdatePlayerVictoryPoints();

        var result = AIHelpers.ShouldBuyDevelopmentCard(board.GetGameState(), bot, true);

        Assert.True(result >= 0.8);
    }

    [Fact]
    public void ShouldBuyDevelopmentCard_No_InsufficientResources()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var bot = board.GetBluePlayer();
        // Dev card costs: 1 Ore, 1 Wool, 1 Grain - give bot only 2 of the 3
        bot.AssignResources(ResourceType.Ore, 1);
        bot.AssignResources(ResourceType.Wool, 1);

        board.GetGameState().Phase = new GamePhase(GameStates.BuildOrTrade, board.GetBluePlayer());

        var result = AIHelpers.ShouldBuyDevelopmentCard(board.GetGameState(), bot);

        Assert.Equal(0.0, result);
    }

    [Fact]
    public void GetDevCardToPlay_NoDevCards_ReturnsNull()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var bot = board.GetBluePlayer();
        // Bot has no development cards
        board.GetGameState().Phase = new GamePhase(GameStates.BuildOrTrade, board.GetBluePlayer());

        var result = AIHelpers.GetDevCardToPlay(board.GetGameState(), bot);

        Assert.Null(result);
    }

    [Fact]
    public void GetDevCardToPlay_DevCardsPurchasedThisRound_ReturnsNull()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var bot = board.GetBluePlayer();
        // Bot has Year of Plenty purchased this round (not yet playable)
        bot.AssignDevelopmentCard(DevelopmentCardType.YearOfPlenty);
        // Don't call MakeNewDevelopmentCardsPlayable - card stays in DevCardsPurchasedThisRound

        // Bot has resources close to a city (needs 3 Ore, 2 Grain) but is short 2 Ore
        // YoP would let them get the 2 Ore they need - so it would be desirable to play
        bot.AssignResources(ResourceType.Ore, 1);
        bot.AssignResources(ResourceType.Grain, 2);

        board.GetGameState().Phase = new GamePhase(GameStates.BuildOrTrade, board.GetBluePlayer());

        var result = AIHelpers.GetDevCardToPlay(board.GetGameState(), bot);

        Assert.Null(result);
    }

    [Fact]
    public void GetDevCardToPlay_OnlyVictoryPoints_ReturnsNull()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var bot = board.GetBluePlayer();
        // Bot only has Victory Point cards (which are never actively played)
        bot.AssignDevelopmentCard(DevelopmentCardType.VictoryPoint);
        bot.MakeNewDevelopmentCardsPlayable();
        board.GetGameState().Phase = new GamePhase(GameStates.BuildOrTrade, board.GetBluePlayer());

        var result = AIHelpers.GetDevCardToPlay(board.GetGameState(), bot);

        Assert.Null(result);
    }

    [Fact]
    public void GetDevCardToPlay_AlreadyPlayedCardThisRound_ReturnsNull()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var bot = board.GetBluePlayer();
        // Bot has a playable Knight
        bot.AssignDevelopmentCard(DevelopmentCardType.Knight);
        bot.MakeNewDevelopmentCardsPlayable();
        board.GetGameState().Phase = new GamePhase(GameStates.BuildOrTrade, board.GetBluePlayer());
        // But already played a dev card this round
        board.GetGameState().Phase.SetDevCardPlayedThisRound();

        var result = AIHelpers.GetDevCardToPlay(board.GetGameState(), bot);

        Assert.Null(result);
    }

    [Fact]
    public void GetDevCardToPlay_Knight_RobberOnBotsTile()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var bot = board.GetBluePlayer();
        // Bot has settlement at V3 which touches tiles T0, T2, T3
        // Place robber on T2 (blocking bot's resource production)
        board.SetRobberTile(board.GetTile(TestTile.T2));

        // Give bot multiple dev cards - Knight should be chosen because robber is hurting bot
        bot.AssignDevelopmentCard(DevelopmentCardType.Knight);
        bot.AssignDevelopmentCard(DevelopmentCardType.Monopoly);
        bot.AssignDevelopmentCard(DevelopmentCardType.YearOfPlenty);
        bot.MakeNewDevelopmentCardsPlayable();

        // Opponents have few resources (so Monopoly isn't attractive)
        board.GetGameState().Phase = new GamePhase(GameStates.BuildOrTrade, board.GetBluePlayer());

        var result = AIHelpers.GetDevCardToPlay(board.GetGameState(), bot);

        Assert.Equal(DevelopmentCardType.Knight, result);
    }

    [Fact]
    public void GetDevCardToPlay_Monopoly_OpponentsHaveManyOfOneResource()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var bot = board.GetBluePlayer();
        var opponent = board.GetRedPlayer();

        // Opponent has lots of Wool
        opponent.AssignResources(ResourceType.Wool, 6);

        // Robber is at T6 (from board setup) which doesn't touch bot's V3
        Assert.Equal(board.GetTile(TestTile.T6).Id, board.GetGameState().RobberTile.Id);

        // Give bot multiple dev cards - Monopoly should be chosen to steal 6 Wool
        bot.AssignDevelopmentCard(DevelopmentCardType.Knight);
        bot.AssignDevelopmentCard(DevelopmentCardType.Monopoly);
        bot.AssignDevelopmentCard(DevelopmentCardType.RoadBuilding);
        bot.MakeNewDevelopmentCardsPlayable();

        board.GetGameState().Phase = new GamePhase(GameStates.BuildOrTrade, board.GetBluePlayer());

        var result = AIHelpers.GetDevCardToPlay(board.GetGameState(), bot);

        Assert.Equal(DevelopmentCardType.Monopoly, result);
    }

    [Fact]
    public void GetDevCardToPlay_YearOfPlenty_TwoResourcesFromCity()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var bot = board.GetBluePlayer();
        var opponent = board.GetRedPlayer();

        // Bot has resources close to city (needs 3 Ore, 2 Grain) but short 2 Ore
        bot.AssignResources(ResourceType.Ore, 1);
        bot.AssignResources(ResourceType.Grain, 2);

        // Robber is at T6 (from board setup) which doesn't touch bot's V3
        Assert.Equal(board.GetTile(TestTile.T6).Id, board.GetGameState().RobberTile.Id);

        // Opponent has no resources worth monopolizing

        // Give bot multiple dev cards - YoP should be chosen to get 2 Ore for city
        bot.AssignDevelopmentCard(DevelopmentCardType.Knight);
        bot.AssignDevelopmentCard(DevelopmentCardType.Monopoly);
        bot.AssignDevelopmentCard(DevelopmentCardType.YearOfPlenty);
        bot.MakeNewDevelopmentCardsPlayable();

        board.GetGameState().Phase = new GamePhase(GameStates.BuildOrTrade, board.GetBluePlayer());

        var result = AIHelpers.GetDevCardToPlay(board.GetGameState(), bot);

        Assert.Equal(DevelopmentCardType.YearOfPlenty, result);
    }

    [Fact]
    public void GetDevCardToPlay_RoadBuilding_NoOpenRoadsButHasSettlementResources()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var bot = board.GetBluePlayer();
        var opponent = board.GetRedPlayer();

        // Blue bot has settlement at V3 and road at E3 (V3-V4)
        // Block V4 by placing opponent settlement at V16 (adjacent to V4, causes V4 to be blocked)
        board.GetEdge(TestEdge.E10).BuildRoad(opponent);
        board.GetVertex(TestVertex.V16).BuildSettlement(opponent);
        GamePlayHelpers.MarkBlockedVertices(board.GetGameState());

        // Bot has settlement resources - wants to expand but needs roads first
        bot.AssignResources(ResourceType.Wood, 1);
        bot.AssignResources(ResourceType.Brick, 1);
        bot.AssignResources(ResourceType.Wool, 1);
        bot.AssignResources(ResourceType.Grain, 1);

        // Robber is at T6 (from board setup) which doesn't touch bot's V3
        Assert.Equal(board.GetTile(TestTile.T6).Id, board.GetGameState().RobberTile.Id);

        // Opponent has no resources worth monopolizing

        // Give bot multiple dev cards - RoadBuilding should be chosen to extend toward new vertex
        bot.AssignDevelopmentCard(DevelopmentCardType.Knight);
        bot.AssignDevelopmentCard(DevelopmentCardType.Monopoly);
        bot.AssignDevelopmentCard(DevelopmentCardType.RoadBuilding);
        bot.MakeNewDevelopmentCardsPlayable();

        board.GetGameState().Phase = new GamePhase(GameStates.BuildOrTrade, board.GetBluePlayer());

        var result = AIHelpers.GetDevCardToPlay(board.GetGameState(), bot);

        Assert.Equal(DevelopmentCardType.RoadBuilding, result);
    }

    // ==================== GetMonopolyTarget Tests ====================

    [Fact]
    public void GetMonopolyTarget_ReturnsResourceOpponentsHaveMostOf()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var bot = board.GetBluePlayer();
        var opponent = board.GetRedPlayer();

        // Opponent has lots of Wool, some Brick
        opponent.AssignResources(ResourceType.Wool, 6);
        opponent.AssignResources(ResourceType.Brick, 2);

        var result = AIHelpers.GetMonopolyTarget(board.GetGameState(), bot);

        Assert.Equal(ResourceType.Wool, result);
    }

    [Fact]
    public void GetMonopolyTarget_TieBreaker_UsesResourceWeight()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var bot = board.GetBluePlayer();
        var opponent = board.GetRedPlayer();

        // Opponent has equal amounts of Ore and Wool - Ore should win (higher weight)
        opponent.AssignResources(ResourceType.Ore, 4);
        opponent.AssignResources(ResourceType.Wool, 4);

        var result = AIHelpers.GetMonopolyTarget(board.GetGameState(), bot);

        Assert.Equal(ResourceType.Ore, result);
    }

    // ==================== GetYearOfPlentyResources Tests ====================

    [Fact]
    public void GetYearOfPlentyResources_TwoOfSameResource_ForCity()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var bot = board.GetBluePlayer();

        // Bot has 1 Ore, 2 Grain - needs 2 more Ore for city
        bot.AssignResources(ResourceType.Ore, 1);
        bot.AssignResources(ResourceType.Grain, 2);

        var result = AIHelpers.GetYearOfPlentyResources(board.GetGameState(), bot);

        Assert.Equal(2, result.Count);
        Assert.Equal(2, result.Count(r => r == ResourceType.Ore));
    }

    [Fact]
    public void GetYearOfPlentyResources_TwoDifferentResources_ForCity()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var bot = board.GetBluePlayer();

        // Bot has 2 Ore, 1 Grain - needs 1 Ore and 1 Grain for city
        bot.AssignResources(ResourceType.Ore, 2);
        bot.AssignResources(ResourceType.Grain, 1);

        var result = AIHelpers.GetYearOfPlentyResources(board.GetGameState(), bot);

        Assert.Equal(2, result.Count);
        Assert.Contains(ResourceType.Ore, result);
        Assert.Contains(ResourceType.Grain, result);
    }

    [Fact]
    public void GetYearOfPlentyResources_ForSettlement()
    {
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var bot = board.GetBluePlayer();

        // Bot has Wood and Wool - needs Brick and Grain for settlement
        bot.AssignResources(ResourceType.Wood, 1);
        bot.AssignResources(ResourceType.Wool, 1);

        // Build two roads so there's a valid settlement spot
        // (need 2 roads because adjacent vertex is blocked due to distance rule)
        board.GetEdge(TestEdge.E2).BuildRoad(bot);
        board.GetEdge(TestEdge.E8).BuildRoad(bot);
        GamePlayHelpers.MarkBlockedVertices(board.GetGameState());

        // Set current player so GetVertexReadyForSettlement works
        board.GetGameState().Phase = new GamePhase(GameStates.BuildOrTrade, bot);

        var result = AIHelpers.GetYearOfPlentyResources(board.GetGameState(), bot);

        Assert.Equal(2, result.Count);
        Assert.Contains(ResourceType.Brick, result);
        Assert.Contains(ResourceType.Grain, result);
    }

    // ==================== ShouldAcceptTrade Tests ====================

    [Fact]
    public void ShouldAcceptTrade_CantFulfill_ReturnsFalse()
    {
        // Bot doesn't have the requested resource - can't fulfill trade
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var bot = board.GetBluePlayer();

        // Bot has no wool
        bot.Resources[ResourceType.Wool] = 0;

        // Human offers brick, requests wool
        var offer = new Dictionary<ResourceType, int> { { ResourceType.Brick, 1 } };
        var request = new Dictionary<ResourceType, int> { { ResourceType.Wool, 1 } };

        var result = AIHelpers.ShouldAcceptTrade(board.GetGameState(), bot, offer, request);

        Assert.False(result);
    }

    [Fact]
    public void ShouldAcceptTrade_CantFulfillMultiple_ReturnsFalse()
    {
        // Bot doesn't have enough of the requested resource
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var bot = board.GetBluePlayer();

        // Bot has only 1 ore
        bot.Resources[ResourceType.Ore] = 1;

        // Human offers brick, requests 2 ore
        var offer = new Dictionary<ResourceType, int> { { ResourceType.Brick, 1 } };
        var request = new Dictionary<ResourceType, int> { { ResourceType.Ore, 2 } };

        var result = AIHelpers.ShouldAcceptTrade(board.GetGameState(), bot, offer, request);

        Assert.False(result);
    }

    [Fact]
    public void ShouldAcceptTrade_TerribleDeal_ReturnsFalse()
    {
        // Human offers 1 wood for 3 ore - terrible deal for bot
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var bot = board.GetBluePlayer();

        // Bot has plenty of ore
        bot.Resources[ResourceType.Ore] = 4;

        var offer = new Dictionary<ResourceType, int> { { ResourceType.Wood, 1 } };
        var request = new Dictionary<ResourceType, int> { { ResourceType.Ore, 3 } };

        var result = AIHelpers.ShouldAcceptTrade(board.GetGameState(), bot, offer, request);

        Assert.False(result);
    }

    [Fact]
    public void ShouldAcceptTrade_GoodDeal_CompletesRoad_ReturnsTrue()
    {
        // Bot has 2 wood but no brick - getting brick lets it build a road
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var bot = board.GetBluePlayer();

        bot.Resources[ResourceType.Wood] = 2;
        bot.Resources[ResourceType.Brick] = 0;

        // Human offers brick for wood - bot can build a road after this
        var offer = new Dictionary<ResourceType, int> { { ResourceType.Brick, 1 } };
        var request = new Dictionary<ResourceType, int> { { ResourceType.Wood, 1 } };

        var result = AIHelpers.ShouldAcceptTrade(board.GetGameState(), bot, offer, request);

        Assert.True(result);
    }

    [Fact]
    public void ShouldAcceptTrade_GoodDeal_CompletesSettlement_ReturnsTrue()
    {
        // Bot is one grain away from settlement
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var bot = board.GetBluePlayer();

        // Build a road so there's an open vertex for settlement
        board.GetEdge(TestEdge.E10).BuildRoad(bot);
        GamePlayHelpers.MarkBlockedVertices(board.GetGameState());

        bot.Resources[ResourceType.Wood] = 2;
        bot.Resources[ResourceType.Brick] = 1;
        bot.Resources[ResourceType.Wool] = 1;
        bot.Resources[ResourceType.Grain] = 0;

        // Human offers grain for wood - bot can build a settlement after this
        var offer = new Dictionary<ResourceType, int> { { ResourceType.Grain, 1 } };
        var request = new Dictionary<ResourceType, int> { { ResourceType.Wood, 1 } };

        board.GetGameState().Phase = new GamePhase(GameStates.BuildOrTrade, bot);

        var result = AIHelpers.ShouldAcceptTrade(board.GetGameState(), bot, offer, request);

        Assert.True(result);
    }

    [Fact]
    public void ShouldAcceptTrade_GoodDeal_CompletesCity_ReturnsTrue()
    {
        // Bot is one ore away from city
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var bot = board.GetBluePlayer();

        bot.Resources[ResourceType.Ore] = 2;
        bot.Resources[ResourceType.Grain] = 2;
        bot.Resources[ResourceType.Wood] = 2;

        // Human offers ore for wood - bot can build a city after this
        var offer = new Dictionary<ResourceType, int> { { ResourceType.Ore, 1 } };
        var request = new Dictionary<ResourceType, int> { { ResourceType.Wood, 1 } };

        var result = AIHelpers.ShouldAcceptTrade(board.GetGameState(), bot, offer, request);

        Assert.True(result);
    }

    [Fact]
    public void ShouldAcceptTrade_WouldLoseResourceNeededForCity_ReturnsFalse()
    {
        // Bot has exactly what it needs for a city - shouldn't trade away needed resources
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var bot = board.GetBluePlayer();

        // Bot has exactly 3 ore, 2 grain for city
        bot.Resources[ResourceType.Ore] = 3;
        bot.Resources[ResourceType.Grain] = 2;

        // Human offers wood for ore - bot would lose ability to build city
        var offer = new Dictionary<ResourceType, int> { { ResourceType.Wood, 1 } };
        var request = new Dictionary<ResourceType, int> { { ResourceType.Ore, 1 } };

        var result = AIHelpers.ShouldAcceptTrade(board.GetGameState(), bot, offer, request);

        Assert.False(result);
    }

    [Fact]
    public void ShouldAcceptTrade_OpponentAboutToWin_ReturnsFalse()
    {
        // Don't help opponent who is within 2 VP of winning (based on visible VP)
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var gs = board.GetGameState();
        var bot = board.GetBluePlayer();
        var human = board.GetRedPlayer();

        // Test board uses 5 VP to win, so "about to win" = 3+ visible VP
        // Human has settlement at V5 (1 VP). Upgrade to city for 2 VP, add another settlement for 3 VP.
        board.GetVertex(TestVertex.V5).UpgradeToCity(); // 2 VP
        board.GetEdge(TestEdge.E25).BuildRoad(human);
        board.GetVertex(TestVertex.V20).BuildSettlement(human); // 3 VP
        gs.UpdatePlayerVictoryPoints();

        // Human is the trade initiator (CurrentPlayer in RespondToTrade phase)
        gs.Phase = new GamePhase(GameStates.RespondToTrade, human, human);

        // Verify human is within 2 VP of winning (visible VP >= VictoryPointsToWin - 2)
        Assert.True(human.VisibleVictoryPoints >= gs.Settings.VictoryPointsToWin - 2);

        // Bot has wood, human wants wood - even a "fair" trade should be rejected
        bot.Resources[ResourceType.Wood] = 2;

        var offer = new Dictionary<ResourceType, int> { { ResourceType.Brick, 1 } };
        var request = new Dictionary<ResourceType, int> { { ResourceType.Wood, 1 } };

        var result = AIHelpers.ShouldAcceptTrade(gs, bot, offer, request);

        Assert.False(result);
    }

    // ==================== ShouldInitiateTrade Tests ====================

    [Fact]
    public void ShouldInitiateTrade_OneResourceFromRoad_ReturnsTrue()
    {
        // Bot has wood but needs brick for road
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var bot = board.GetBluePlayer();

        bot.Resources[ResourceType.Wood] = 1;
        bot.Resources[ResourceType.Wool] = 2; // Excess to offer
        bot.Resources[ResourceType.Brick] = 0;

        var result = AIHelpers.ShouldInitiateTrade(board.GetGameState(), bot);

        Assert.True(result);
    }

    [Fact]
    public void ShouldInitiateTrade_OneResourceFromCity_ReturnsTrue()
    {
        // Bot has 2 grain, 2 ore - needs 1 more ore
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var bot = board.GetBluePlayer();

        bot.Resources[ResourceType.Grain] = 2;
        bot.Resources[ResourceType.Ore] = 2;
        bot.Resources[ResourceType.Wood] = 2; // Excess to offer

        var result = AIHelpers.ShouldInitiateTrade(board.GetGameState(), bot);

        Assert.True(result);
    }

    [Fact]
    public void ShouldInitiateTrade_NotCloseToAnyBuild_ReturnsFalse()
    {
        // Bot has no resources
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var bot = board.GetBluePlayer();

        bot.Resources[ResourceType.Wood] = 0;
        bot.Resources[ResourceType.Brick] = 0;
        bot.Resources[ResourceType.Wool] = 0;
        bot.Resources[ResourceType.Grain] = 0;
        bot.Resources[ResourceType.Ore] = 0;

        var result = AIHelpers.ShouldInitiateTrade(board.GetGameState(), bot);

        Assert.False(result);
    }

    [Fact]
    public void ShouldInitiateTrade_NoExcessToOffer_ReturnsFalse()
    {
        // Bot is one away from road but has nothing extra to offer
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var bot = board.GetBluePlayer();

        bot.Resources[ResourceType.Wood] = 1;
        bot.Resources[ResourceType.Brick] = 0;
        // No excess resources

        var result = AIHelpers.ShouldInitiateTrade(board.GetGameState(), bot);

        Assert.False(result);
    }

    [Fact]
    public void ShouldInitiateTrade_CanAlreadyBuild_ReturnsFalse()
    {
        // Bot already has resources for a road - no need to trade
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var bot = board.GetBluePlayer();

        bot.Resources[ResourceType.Wood] = 1;
        bot.Resources[ResourceType.Brick] = 1;

        var result = AIHelpers.ShouldInitiateTrade(board.GetGameState(), bot);

        Assert.False(result);
    }

    // ==================== GetTradeOffer Tests ====================

    [Fact]
    public void GetTradeOffer_OneFromRoad_RequestsBrick()
    {
        // Bot has wood, needs brick
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var bot = board.GetBluePlayer();

        bot.Resources[ResourceType.Wood] = 1;
        bot.Resources[ResourceType.Wool] = 2; // Excess
        bot.Resources[ResourceType.Brick] = 0;

        var offer = AIHelpers.GetTradeOffer(board.GetGameState(), bot);

        Assert.NotNull(offer);
        Assert.True(offer.Request.ContainsKey(ResourceType.Brick));
        Assert.Equal(1, offer.Request[ResourceType.Brick]);
        Assert.True(offer.Offer.ContainsKey(ResourceType.Wool));
    }

    [Fact]
    public void GetTradeOffer_OneFromCity_RequestsOre()
    {
        // Bot has 2 grain, 2 ore - needs 1 more ore
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var bot = board.GetBluePlayer();

        bot.Resources[ResourceType.Grain] = 2;
        bot.Resources[ResourceType.Ore] = 2;
        bot.Resources[ResourceType.Wood] = 2; // Excess

        var offer = AIHelpers.GetTradeOffer(board.GetGameState(), bot);

        Assert.NotNull(offer);
        Assert.True(offer.Request.ContainsKey(ResourceType.Ore));
        Assert.Equal(1, offer.Request[ResourceType.Ore]);
    }

    [Fact]
    public void GetTradeOffer_HasExcess_Offers2For1()
    {
        // Bot has 3 wool (excess), needs brick
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var bot = board.GetBluePlayer();

        bot.Resources[ResourceType.Wood] = 1;
        bot.Resources[ResourceType.Wool] = 3;
        bot.Resources[ResourceType.Brick] = 0;

        var offer = AIHelpers.GetTradeOffer(board.GetGameState(), bot);

        Assert.NotNull(offer);
        // Should offer 2 wool for 1 brick (better chance of acceptance)
        Assert.True(offer.Offer[ResourceType.Wool] >= 1);
    }

    [Fact]
    public void GetTradeOffer_NotCloseToAnyBuild_ReturnsNull()
    {
        // Bot has no resources
        var board = TestHelpers.CreateOriginalTestBoardWithSettlements(true);
        var bot = board.GetBluePlayer();

        bot.Resources[ResourceType.Wood] = 0;
        bot.Resources[ResourceType.Brick] = 0;
        bot.Resources[ResourceType.Wool] = 0;
        bot.Resources[ResourceType.Grain] = 0;
        bot.Resources[ResourceType.Ore] = 0;

        var offer = AIHelpers.GetTradeOffer(board.GetGameState(), bot);

        Assert.Null(offer);
    }
}



