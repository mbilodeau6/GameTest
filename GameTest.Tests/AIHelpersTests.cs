using Xunit;
using GameTest.Models;
using GameTest.Services;

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
        var vertex2 = GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, tile1, tile2, null, null);
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
        GamePlayHelpers.LinkEdgesAndVertices(gs);

        Assert.Equal(PlayerColor.Red, gs.Players[0].Color);
        gs.Phase.CurrentPlayer = gs.Players[0];

        var brickTile = gs.Tiles.First(t => t.Resource == ResourceType.Brick);
        var e1 = GamePlayHelpers.GetEdgeFromTileInfo(gs.Edges, brickTile, null, HexDirection.SW);
        e1.BuildRoad(gs.Phase.CurrentPlayer);
        var wool2Tile = gs.Tiles.First(t => t.Resource == ResourceType.Wool && t.DiceNumber == 2);
        var e2 = GamePlayHelpers.GetEdgeFromTileInfo(gs.Edges, wool2Tile, null, HexDirection.W);
        e2.BuildRoad(gs.Phase.CurrentPlayer);
        var grainTile = gs.Tiles.First(t => t.Resource == ResourceType.Grain);
        var e3 = GamePlayHelpers.GetEdgeFromTileInfo(gs.Edges, grainTile, brickTile, null);
        e3.BuildRoad(gs.Phase.CurrentPlayer);
        var dessertTile = gs.Tiles.First(t => t.Resource == ResourceType.Desert);
        var e4 = GamePlayHelpers.GetEdgeFromTileInfo(gs.Edges, dessertTile, grainTile, null);
        e4.BuildRoad(gs.Phase.CurrentPlayer);

        var wool11Tile = gs.Tiles.First(t => t.Resource == ResourceType.Wool && t.DiceNumber == 11);
        var expectedVertex = GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, wool11Tile, grainTile, dessertTile, null);

        var vertexForSettlement = AIHelpers.GetVertexReadyForSettlement(gs);

        Assert.NotNull(vertexForSettlement);
        Assert.Equal(expectedVertex.Id, vertexForSettlement.Id);
    }

    [Fact]
    public void GetRankedListOfVertexTargets_ExcludeBlocked()
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
        GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, t3, null, null, VertexDirection.SW).BuildSettlement(gs.Players[2]);
        GamePlayHelpers.GetEdgeFromTileInfo(gs.Edges, t3, null, HexDirection.W).BuildRoad(gs.Players[2]);
        GamePlayHelpers.GetEdgeFromTileInfo(gs.Edges, t3, null, HexDirection.NW).BuildRoad(gs.Players[2]);
        GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, t12, t13, t18, null).BuildSettlement(gs.Players[2]);
        GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, t1, t13, t2, null).BuildSettlement(gs.Players[2]);
        GamePlayHelpers.GetEdgeFromTileInfo(gs.Edges, t2, null, HexDirection.NW).BuildRoad(gs.Players[2]);
        GamePlayHelpers.GetEdgeFromTileInfo(gs.Edges, t2, t1, null).BuildRoad(gs.Players[2]);
        GamePlayHelpers.GetEdgeFromTileInfo(gs.Edges, t1, t13, null).BuildRoad(gs.Players[2]);
        GamePlayHelpers.GetEdgeFromTileInfo(gs.Edges, t12, t13, null).BuildRoad(gs.Players[2]);

        // Place red opponent pieces (blocking)
        GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, t9, t10, t17, null).BuildSettlement(gs.Players[0]);
        GamePlayHelpers.GetEdgeFromTileInfo(gs.Edges, t18, t10, null).BuildRoad(gs.Players[0]);
        GamePlayHelpers.GetEdgeFromTileInfo(gs.Edges, t17, t10, null).BuildRoad(gs.Players[0]);
        GamePlayHelpers.GetEdgeFromTileInfo(gs.Edges, t17, t9, null).BuildRoad(gs.Players[0]);
        GamePlayHelpers.GetEdgeFromTileInfo(gs.Edges, t17, t8, null).BuildRoad(gs.Players[0]);
        GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, t7, t8, null, null).BuildSettlement(gs.Players[0]);
        GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, t16, t6, t7, null).BuildSettlement(gs.Players[0]);
        GamePlayHelpers.GetEdgeFromTileInfo(gs.Edges, t7, t8, null).BuildRoad(gs.Players[0]);
        GamePlayHelpers.GetEdgeFromTileInfo(gs.Edges, t7, t16, null).BuildRoad(gs.Players[0]);
        GamePlayHelpers.GetEdgeFromTileInfo(gs.Edges, t6, t7, null).BuildRoad(gs.Players[0]);

        // Place pre-existing blue (current-player) pieces
        GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, t13, t14, t19, null).BuildSettlement(gs.Players[1]);
        GamePlayHelpers.GetEdgeFromTileInfo(gs.Edges, t14, t19, null).BuildRoad(gs.Players[1]);
        GamePlayHelpers.GetEdgeFromTileInfo(gs.Edges, t15, t19, null).BuildRoad(gs.Players[1]);
        GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, t4, t5, null, null).BuildSettlement(gs.Players[1]);
        GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, t15, t6, t5, null).BuildSettlement(gs.Players[1]);
        GamePlayHelpers.GetEdgeFromTileInfo(gs.Edges, t4, t5, null).BuildRoad(gs.Players[1]);
        GamePlayHelpers.GetEdgeFromTileInfo(gs.Edges, t15, t5, null).BuildRoad(gs.Players[1]);

        GamePlayHelpers.MarkBlockedVertices(gs);
        GamePlayHelpers.LinkEdgesAndVertices(gs);

        var rankedGoals = AIHelpers.GetRankedListOfVertexTargets(gs);

        Assert.Equal(12, rankedGoals.Count);
        foreach (var goal in rankedGoals)
        {
            Assert.Null(goal.TargetVertex.Building);
        }
        
        var v16 = GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, t2, t3, null, null);
        var g16 = rankedGoals.First(g => g.TargetVertex.Id == v16.Id);
        Assert.NotNull(g16);
        Assert.Equal(3, g16.RoadsNeeded);

        var v17 = GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, t2, null, null, VertexDirection.NW);
        var g17 = rankedGoals.First(g => g.TargetVertex.Id == v17.Id);
        Assert.NotNull(g17);
        Assert.Equal(4, g17.RoadsNeeded);
        Assert.True(g16.OverallScore > g17.OverallScore); // g17 needs more roads and provide less resources

        var v15 = GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, t2, t3, t14, null);
        var g15 = rankedGoals.First(g => g.TargetVertex.Id == v15.Id);
        Assert.NotNull(g15);
        Assert.Equal(2, g15.RoadsNeeded);
        Assert.True(g15.OverallScore > g16.OverallScore); // g16 needs more roads and provides less resources

        var v18 = GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, t3, t14, t4, null);
        var g18 = rankedGoals.First(g => g.TargetVertex.Id == v18.Id);
        Assert.NotNull(g18);
        Assert.Equal(2, g18.RoadsNeeded);


        var v22 = GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, t14, t4, t15, null);
        var g22 = rankedGoals.First(g => g.TargetVertex.Id == v22.Id);
        Assert.NotNull(g22);
        Assert.Equal(1, g22.RoadsNeeded);
        Assert.True(g22.OverallScore > g18.OverallScore); // g18 needs more roads and provides less valuable resources

        var v2 = GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, t18, t19, t17, null);
        var g2 = rankedGoals.First(g => g.TargetVertex.Id == v2.Id);
        Assert.NotNull(g2);
        Assert.Equal(2, g2.RoadsNeeded);

        var v3 = GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, t19, t17, t16, null);
        var g3 = rankedGoals.First(g => g.TargetVertex.Id == v3.Id);
        Assert.NotNull(g3);
        Assert.Equal(1, g3.RoadsNeeded);

        var v4 = GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, t19, t15, t16, null);
        var g4 = rankedGoals.First(g => g.TargetVertex.Id == v4.Id);
        Assert.NotNull(g4);
        Assert.Equal(0, g4.RoadsNeeded);

        var v34 = GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, t17, t16, t8, null);
        var g34 = rankedGoals.First(g => g.TargetVertex.Id == v34.Id);
        Assert.NotNull(g34);
        Assert.Equal(2, g34.RoadsNeeded);

        var v28 = GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, t5, null, null, VertexDirection.S);
        var g28 = rankedGoals.First(g => g.TargetVertex.Id == v28.Id);
        Assert.NotNull(g28);
        Assert.Equal(2, g28.RoadsNeeded);

        var v33 = GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, t6, null, null, VertexDirection.S);
        var g33 = rankedGoals.First(g => g.TargetVertex.Id == v33.Id);
        Assert.NotNull(g33);
        Assert.Equal(2, g33.RoadsNeeded);

        var v38 = GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, t7, null, null, VertexDirection.S);
        var g38 = rankedGoals.First(g => g.TargetVertex.Id == v38.Id);
        Assert.NotNull(g38);
        Assert.Equal(4, g38.RoadsNeeded);
        Assert.True(g38.OverallScore < g28.OverallScore && g38.OverallScore < g33.OverallScore);
    }

    [Fact]
    public void FindVertexWithoutRoads_OnlyOneSettelment_HasNoRoad()
    {
        // Arrange
        var gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter);
        gs.Vertices[0].BuildSettlement(gs.Players[0]);
        GamePlayHelpers.MarkBlockedVertices(gs);
        GamePlayHelpers.LinkEdgesAndVertices(gs);

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
        GamePlayHelpers.LinkEdgesAndVertices(gs);
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
        GamePlayHelpers.LinkEdgesAndVertices(gs);
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
        GamePlayHelpers.LinkEdgesAndVertices(gs);
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
        GamePlayHelpers.LinkEdgesAndVertices(gs);
        gs.Vertices[0].BuildSettlement(gs.Players[0]);
        gs.Vertices[2].BuildSettlement(gs.Players[1]); // Opponents settlment
        gs.Vertices[4].BuildSettlement(gs.Players[0]);
        GamePlayHelpers.MarkBlockedVertices(gs);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => AIHelpers.FindVertexWithoutRoads(gs, gs.Players[0]));
    }
}



