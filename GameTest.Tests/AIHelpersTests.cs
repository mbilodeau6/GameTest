using Xunit;
using GameTest.Models;
using GameTest.Services;
using TH = GameTest.Tests.TestHelpers.SetUpPhaseTestReferences;
using Microsoft.AspNetCore.Razor.TagHelpers;

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
        var gs = BoardCreationHelpers.CreateNewBoard(GameType.Test);
        gs.AddPlayer(new Player("Player3", PlayerColor.Red, isBot: false));
        
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
        var gs = BoardCreationHelpers.CreateNewBoard(GameType.Test);
        Assert.Equal(PlayerColor.Blue, gs.Players[1].Color);

        Dictionary<ResourceType, double> baseRates = AIHelpers.GetBaseResourceAcquisitionRates(gs, gs.Players[1]);

        Assert.Equal(0.0, baseRates[ResourceType.Brick]);
        Assert.Equal(0.0, baseRates[ResourceType.Wool]);
        Assert.Equal(AIHelpers.GetProbabilityForDiceRoll(9), baseRates[ResourceType.Grain]);
        Assert.Equal(AIHelpers.GetProbabilityForDiceRoll(3), baseRates[ResourceType.Ore]);
        Assert.Equal(AIHelpers.GetProbabilityForDiceRoll(6), baseRates[ResourceType.Wood]);
    }

    [Fact]
    public void GetSettlementToUpgrade_NoOwnedSettlements_Null()
    {
        var gs = BoardCreationHelpers.CreateNewBoard(GameType.Test);
        Assert.Equal(PlayerColor.Red, gs.Players[0].Color);
        gs.Phase.CurrentPlayer = gs.Players[0];
        var existingSettlement = gs.Vertices.First(v => v.Owner != null && v.Owner.Id == gs.Players[0].Id);
        existingSettlement.UpgradeToCity();

        var settlementToUpgrade = AIHelpers.GetSettlementToUpgrade(gs);

        Assert.Null(settlementToUpgrade);
    }

    [Fact]
    public void GetSettlementToUpgrade_SelectHigherProducingSettlement()
    {
        var gs = BoardCreationHelpers.CreateNewBoard(GameType.Test);
        Assert.Equal(PlayerColor.Red, gs.Players[0].Color);
        gs.Phase.CurrentPlayer = gs.Players[0];
        gs.Players[0].AssignResources(ResourceType.Ore, 3);
        gs.Players[0].AssignResources(ResourceType.Grain, 2);

        var vertex1 = gs.Vertices.First(v => v.Owner != null && v.Owner.Id == gs.Players[0].Id);

        var tile1 = gs.Tiles.First(t => t.Resource == ResourceType.Brick);
        var tile2 = gs.Tiles.First(t => t.Resource == ResourceType.Desert);
        var vertex2 = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, tile1, tile2, null, null);
        vertex2.BuildSettlement(gs.Phase.CurrentPlayer);

        var settlementToUpgrade = AIHelpers.GetSettlementToUpgrade(gs);

        Assert.NotNull(settlementToUpgrade);
        Assert.Equal(vertex1.Id, settlementToUpgrade.Id);
    }

    [Fact]
    public void GetVertexReadyForSettlement_NoAvailableVertices_Null()
    {
        var gs = BoardCreationHelpers.CreateNewBoard(GameType.Test);
        Assert.Equal(PlayerColor.Red, gs.Players[0].Color);
        gs.Phase.CurrentPlayer = gs.Players[0];

        var vertexForSettlement = AIHelpers.GetVertexReadyForSettlement(gs);

        Assert.Null(vertexForSettlement);
    }

    [Fact]
    public void GetVertexReadyForSettlement_ThreeOptions_BestSelected()
    {
        var gs = BoardCreationHelpers.CreateNewBoard(GameType.Test);
        GamePlayHelpers.MarkBlockedVertices(gs);
        BoardCreationHelpers.LinkEdgesAndVertices(gs);

        Assert.Equal(PlayerColor.Red, gs.Players[0].Color);
        gs.Phase.CurrentPlayer = gs.Players[0];

        var brickTile = gs.Tiles.First(t => t.Resource == ResourceType.Brick);
        var e1 = BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, brickTile, null, HexDirection.SW);
        e1.BuildRoad(gs.Phase.CurrentPlayer);
        var wool2Tile = gs.Tiles.First(t => t.Resource == ResourceType.Wool && t.DiceNumber == 2);
        var e2 = BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, wool2Tile, null, HexDirection.W);
        e2.BuildRoad(gs.Phase.CurrentPlayer);
        var grainTile = gs.Tiles.First(t => t.Resource == ResourceType.Grain);
        var e3 = BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, grainTile, brickTile, null);
        e3.BuildRoad(gs.Phase.CurrentPlayer);
        var dessertTile = gs.Tiles.First(t => t.Resource == ResourceType.Desert);
        var e4 = BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, dessertTile, grainTile, null);
        e4.BuildRoad(gs.Phase.CurrentPlayer);

        var wool11Tile = gs.Tiles.First(t => t.Resource == ResourceType.Wool && t.DiceNumber == 11);
        var expectedVertex = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, wool11Tile, grainTile, dessertTile, null);

        var vertexForSettlement = AIHelpers.GetVertexReadyForSettlement(gs);

        Assert.NotNull(vertexForSettlement);
        Assert.Equal(expectedVertex.Id, vertexForSettlement.Id);
    }

    private GameState CreateGameStateForOwnershipTesting()
    {
        var gs = TestHelpers.CreateGameStateForSetUpPhase();
        BoardCreationHelpers.LinkEdgesAndVertices(gs);
        var v1 = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, TH.DesertTile, TH.Wool2Tile, null, null);
        v1.BuildSettlement(TH.HumanPlayer);
        var v3 = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, TH.DesertTile, TH.GrainTile, TH.WoodTile, null);
        v3.BuildSettlement(TH.HumanPlayer);
        var v39 = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, TH.WoodTile, TH.OreTile, TH.Wool5Tile, null);
        v39.BuildSettlement(TH.HumanPlayer);
        var v52 = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, TH.BrickTile, TH.OreTile, null, null);
        v52.BuildSettlement(TH.HumanPlayer);

        return gs;
    }

    [Fact]
    public void GetAllOwnedBuildings_NoneFound()
    {
        // Arrange
        var gs = CreateGameStateForOwnershipTesting();
        var expectedVertex = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, TH.DesertTile, TH.Wool2Tile, null, null);

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
        var gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter);
        gs.Players.Add(new Player("Player3", PlayerColor.Green, isBot: false));
        Assert.Equal(PlayerColor.Blue, gs.Players[1].Color);
        gs.Phase.CurrentPlayer = gs.Players[1];

        // Get references to all tiles needed for test
        var t1 = BoardCreationHelpers.GetTileAt(gs.Tiles, -2, -2);
        var t2 = BoardCreationHelpers.GetTileAt(gs.Tiles, -3, -1);
        var t3 = BoardCreationHelpers.GetTileAt(gs.Tiles, -4, 0);
        var t4 = BoardCreationHelpers.GetTileAt(gs.Tiles, -3, 1);
        var t5 = BoardCreationHelpers.GetTileAt(gs.Tiles, -2, 2);
        var t6 = BoardCreationHelpers.GetTileAt(gs.Tiles, 0, 2);
        var t7 = BoardCreationHelpers.GetTileAt(gs.Tiles, 2, 2);
        var t8 = BoardCreationHelpers.GetTileAt(gs.Tiles, 3, 1);
        var t9 = BoardCreationHelpers.GetTileAt(gs.Tiles, 4, 0);
        var t10 = BoardCreationHelpers.GetTileAt(gs.Tiles, 3, -1);
        var t12 = BoardCreationHelpers.GetTileAt(gs.Tiles, 0, -2);
        var t13 = BoardCreationHelpers.GetTileAt(gs.Tiles, -1, -1);
        var t14 = BoardCreationHelpers.GetTileAt(gs.Tiles, -2, 0);
        var t15 = BoardCreationHelpers.GetTileAt(gs.Tiles, -1, 1);
        var t16 = BoardCreationHelpers.GetTileAt(gs.Tiles, 1, 1);
        var t17 = BoardCreationHelpers.GetTileAt(gs.Tiles, 2, 0);
        var t18 = BoardCreationHelpers.GetTileAt(gs.Tiles, 1, -1);
        var t19 = BoardCreationHelpers.GetTileAt(gs.Tiles, 0, 0);

        // Place green opponent pieces (blocking)
        BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, t3, null, null, VertexDirection.SW).BuildSettlement(gs.Players[2]);
        BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, t3, null, HexDirection.W).BuildRoad(gs.Players[2]);
        BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, t3, null, HexDirection.NW).BuildRoad(gs.Players[2]);
        BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, t12, t13, t18, null).BuildSettlement(gs.Players[2]);
        BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, t1, t13, t2, null).BuildSettlement(gs.Players[2]);
        BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, t2, null, HexDirection.NW).BuildRoad(gs.Players[2]);
        BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, t2, t1, null).BuildRoad(gs.Players[2]);
        BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, t1, t13, null).BuildRoad(gs.Players[2]);
        BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, t12, t13, null).BuildRoad(gs.Players[2]);

        // Place red opponent pieces (blocking)
        BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, t9, t10, t17, null).BuildSettlement(gs.Players[0]);
        BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, t18, t10, null).BuildRoad(gs.Players[0]);
        BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, t17, t10, null).BuildRoad(gs.Players[0]);
        BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, t17, t9, null).BuildRoad(gs.Players[0]);
        BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, t17, t8, null).BuildRoad(gs.Players[0]);
        BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, t7, t8, null, null).BuildSettlement(gs.Players[0]);
        BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, t16, t6, t7, null).BuildSettlement(gs.Players[0]);
        BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, t7, t8, null).BuildRoad(gs.Players[0]);
        BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, t7, t16, null).BuildRoad(gs.Players[0]);
        BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, t6, t7, null).BuildRoad(gs.Players[0]);

        // Place pre-existing blue (current-player) pieces
        BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, t13, t14, t19, null).BuildSettlement(gs.Players[1]);
        BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, t14, t19, null).BuildRoad(gs.Players[1]);
        BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, t15, t19, null).BuildRoad(gs.Players[1]);
        BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, t4, t5, null, null).BuildSettlement(gs.Players[1]);
        BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, t15, t6, t5, null).BuildSettlement(gs.Players[1]);
        BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, t4, t5, null).BuildRoad(gs.Players[1]);
        BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, t15, t5, null).BuildRoad(gs.Players[1]);

        GamePlayHelpers.MarkBlockedVertices(gs);
        BoardCreationHelpers.LinkEdgesAndVertices(gs);

        var rankedGoals = AIHelpers.GetRankedListOfVertexTargets(gs, AIHelpers.GetAllOwnedBuildings(gs, gs.Phase.CurrentPlayer));

        Assert.Equal(12, rankedGoals.Count);
        foreach (var goal in rankedGoals)
        {
            Assert.Null(goal.TargetVertex.Building);
        }
        
        var v16 = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, t2, t3, null, null);
        var g16 = rankedGoals.First(g => g.TargetVertex.Id == v16.Id);
        Assert.NotNull(g16);
        Assert.Equal(3, g16.RoadsNeeded);

        var v17 = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, t2, null, null, VertexDirection.NW);
        var g17 = rankedGoals.First(g => g.TargetVertex.Id == v17.Id);
        Assert.NotNull(g17);
        Assert.Equal(4, g17.RoadsNeeded);
        Assert.True(g16.OverallScore > g17.OverallScore); // g17 needs more roads and provide less resources

        var v15 = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, t2, t3, t14, null);
        var g15 = rankedGoals.First(g => g.TargetVertex.Id == v15.Id);
        Assert.NotNull(g15);
        Assert.Equal(2, g15.RoadsNeeded);
        Assert.True(g15.OverallScore > g16.OverallScore); // g16 needs more roads and provides less resources

        var v18 = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, t3, t14, t4, null);
        var g18 = rankedGoals.First(g => g.TargetVertex.Id == v18.Id);
        Assert.NotNull(g18);
        Assert.Equal(2, g18.RoadsNeeded);


        var v22 = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, t14, t4, t15, null);
        var g22 = rankedGoals.First(g => g.TargetVertex.Id == v22.Id);
        Assert.NotNull(g22);
        Assert.Equal(1, g22.RoadsNeeded);
        Assert.True(g22.OverallScore > g18.OverallScore); // g18 needs more roads and provides less valuable resources

        var v2 = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, t18, t19, t17, null);
        var g2 = rankedGoals.First(g => g.TargetVertex.Id == v2.Id);
        Assert.NotNull(g2);
        Assert.Equal(2, g2.RoadsNeeded);

        var v3 = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, t19, t17, t16, null);
        var g3 = rankedGoals.First(g => g.TargetVertex.Id == v3.Id);
        Assert.NotNull(g3);
        Assert.Equal(1, g3.RoadsNeeded);

        var v4 = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, t19, t15, t16, null);
        var g4 = rankedGoals.First(g => g.TargetVertex.Id == v4.Id);
        Assert.NotNull(g4);
        Assert.Equal(0, g4.RoadsNeeded);

        var v34 = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, t17, t16, t8, null);
        var g34 = rankedGoals.First(g => g.TargetVertex.Id == v34.Id);
        Assert.NotNull(g34);
        Assert.Equal(2, g34.RoadsNeeded);

        var v28 = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, t5, null, null, VertexDirection.S);
        var g28 = rankedGoals.First(g => g.TargetVertex.Id == v28.Id);
        Assert.NotNull(g28);
        Assert.Equal(2, g28.RoadsNeeded);

        var v33 = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, t6, null, null, VertexDirection.S);
        var g33 = rankedGoals.First(g => g.TargetVertex.Id == v33.Id);
        Assert.NotNull(g33);
        Assert.Equal(2, g33.RoadsNeeded);

        var v38 = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, t7, null, null, VertexDirection.S);
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
        var v35 = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, TH.GrainTile, TH.Wool5Tile, null, null);
        v35.BuildSettlement(TH.BotPlayer);
        var e44 = BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, TH.GrainTile, TH.Wool5Tile, null);
        e44.BuildRoad(TH.BotPlayer);
        var v42 = BoardCreationHelpers.GetVertexFromTileInfo(gs.Vertices, TH.Wool2Tile, TH.WoodTile, TH.BrickTile, null);
        v42.BuildSettlement(TH.BotPlayer);
        GamePlayHelpers.MarkBlockedVertices(gs);
        gs.Phase.CurrentPlayer = TH.BotPlayer;
        gs.Phase.PhaseState = GameStates.PlaceSecondRoad;

        var expectedEdge = BoardCreationHelpers.GetEdgeFromTileInfo(gs.Edges, TH.Wool2Tile, TH.BrickTile, null);

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
    public void FindVertexWithoutRoads_OnlyOneSettelment_HasNoRoad()
    {
        // Arrange
        var gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter);
        gs.Vertices[0].BuildSettlement(gs.Players[0]);
        GamePlayHelpers.MarkBlockedVertices(gs);
        BoardCreationHelpers.LinkEdgesAndVertices(gs);

        // Act
        var vertex = AIHelpers.FindVertexWithoutRoads(gs, gs.Players[0]);

        // Assert
        Assert.NotNull(vertex);
        Assert.NotEmpty(vertex.Edges);
        Assert.Equal(gs.Vertices[0].Id, vertex.Id);
        Assert.True(vertex.Edges.All(e => e.Owner == null));
    }

    [Fact]
    public void FindVertexWithoutRoads_OnlyOneSettelment_HasRoad()
    {
        // Arrange
        var gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter);
        BoardCreationHelpers.LinkEdgesAndVertices(gs);
        gs.Vertices[0].BuildSettlement(gs.Players[0]);
        gs.Vertices[0].Edges[0].BuildRoad(gs.Players[0]);
        gs.Vertices[2].BuildSettlement(gs.Players[1]); // Opponents settlment
        GamePlayHelpers.MarkBlockedVertices(gs);

        // Act
        var vertex = AIHelpers.FindVertexWithoutRoads(gs, gs.Players[0]);

        // Assert
        Assert.Null(vertex);
    }

    [Fact]
    public void FindVertexWithoutRoads_TwoSettelments_OneWithoutRoad()
    {
        var gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter);
        BoardCreationHelpers.LinkEdgesAndVertices(gs);
        gs.Vertices[0].BuildSettlement(gs.Players[0]);
        gs.Vertices[0].Edges[0].BuildRoad(gs.Players[0]);
        gs.Vertices[2].BuildSettlement(gs.Players[1]); // Opponents settlment
        gs.Vertices[2].Edges[0].BuildRoad(gs.Players[1]); // Oppenents settlement
        gs.Vertices[4].BuildSettlement(gs.Players[0]);
        GamePlayHelpers.MarkBlockedVertices(gs);

        // Act
        var vertex = AIHelpers.FindVertexWithoutRoads(gs, gs.Players[0]);

        // Assert
        Assert.NotNull(vertex);
        Assert.NotEmpty(vertex.Edges);
        Assert.Equal(gs.Vertices[4].Id, vertex.Id);
        Assert.True(vertex.Edges.All(e => e.Owner == null));    
    }

    [Fact]
    public void FindVertexWithoutRoads_TwoSettelments_BothHaveRoad()
    {
        var gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter);
        BoardCreationHelpers.LinkEdgesAndVertices(gs);
        gs.Vertices[0].BuildSettlement(gs.Players[0]);
        gs.Vertices[0].Edges[0].BuildRoad(gs.Players[0]);
        gs.Vertices[2].BuildSettlement(gs.Players[1]); // Opponents settlment
        gs.Vertices[4].BuildSettlement(gs.Players[0]);
        gs.Vertices[4].Edges[0].BuildRoad(gs.Players[0]);
        GamePlayHelpers.MarkBlockedVertices(gs);

        // Act
        var vertex = AIHelpers.FindVertexWithoutRoads(gs, gs.Players[0]);

        // Assert
        Assert.Null(vertex);
    }

    [Fact]
    public void FindVertexWithoutRoads_TwoSettelmentsWithoutRoad_Exception()
    {
        var gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter);
        BoardCreationHelpers.LinkEdgesAndVertices(gs);
        gs.Vertices[0].BuildSettlement(gs.Players[0]);
        gs.Vertices[2].BuildSettlement(gs.Players[1]); // Opponents settlment
        gs.Vertices[4].BuildSettlement(gs.Players[0]);
        GamePlayHelpers.MarkBlockedVertices(gs);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => AIHelpers.FindVertexWithoutRoads(gs, gs.Players[0]));
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
}



