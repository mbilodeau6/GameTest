using Xunit;
using GameTest.Models;
using GameTest.Services;
using TH = GameTest.Tests.TestHelpers.SetUpPhaseTestReferences;

namespace GameTest.Tests;

public class VertexPickerTests
{
    // TODO: Need to identify specific scenarios to test
    [Fact]
    public void PickVertex_SetUpSettlementPhaseOpenBoard()
    {
        // Arrange
        var gs = TestHelpers.CreateGameStateForSetUpPhase();

        VertexPicker picker = new VertexPicker(gs);
        var expectedVertex1 = GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, TH.WoodTile, TH.GrainTile, TH.Wool5Tile, null);
        var expectedVertex2 = GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, TH.Wool5Tile, TH.WoodTile, TH.OreTile, null);

        // Act
        var selectedVertex = picker.PickVertex();

        // Assert - Test will accept the vertex with the highest probability of producing resources
        // and the highest probability of producing ore and other resources.
        Assert.NotNull(selectedVertex);
        Assert.True(selectedVertex.Id == expectedVertex1.Id || selectedVertex.Id == expectedVertex2.Id);
    }

    [Fact]
    public void PickVertex_SetUpSettlementPhaseBestSpotsUnavailable()
    {
        // Arrange
        var gs = TestHelpers.CreateGameStateForSetUpPhase();
        var oreWoodWool5Vertex = GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, TH.Wool5Tile, TH.WoodTile, TH.OreTile, null);
        oreWoodWool5Vertex.BuildSettlement(TH.HumanPlayer);
        GamePlayHelpers.LinkEdgesAndVertices(gs);
        GamePlayHelpers.MarkBlockedVertices(gs, oreWoodWool5Vertex);
        

        VertexPicker picker = new VertexPicker(gs);
        var expectedVertex1 = GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, TH.WoodTile, TH.BrickTile, TH.OreTile, null);
        var expectedVertex2 = GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, TH.GrainTile, TH.WoodTile, TH.DesertTile, null);

        // Act
        var selectedVertex = picker.PickVertex();

        // Assert - Test will accept the vertex with the highest probability of producing resources
        // and the highest probability of producing ore and other resources.
        Assert.NotNull(selectedVertex);
        Assert.True(selectedVertex.Id == expectedVertex1.Id || selectedVertex.Id == expectedVertex2.Id);
    }

    [Fact]
    public void PickVertex_SetUpFirstRoad()
    {
        // Arrange
        var gs = TestHelpers.CreateGameStateForSetUpPhase();
        var desertWoodWool2Vertex = GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, TH.Wool2Tile, TH.WoodTile, TH.DesertTile, null);
        desertWoodWool2Vertex.BuildSettlement(TH.BotPlayer);
        GamePlayHelpers.LinkEdgesAndVertices(gs);
        GamePlayHelpers.MarkBlockedVertices(gs, desertWoodWool2Vertex);
        

        VertexPicker picker = new VertexPicker(gs);
        var expectedEdge = GamePlayHelpers.GetEdgeFromTileInfo(gs.Edges, TH.DesertTile, TH.WoodTile, null);

        // Act
        var selectedEdge = picker.PickEdge();

        // Assert
        Assert.NotNull(selectedEdge);
        Assert.Equal(expectedEdge.Id, selectedEdge.Id);
    }

    [Fact]
    public void PickVertex_SetUpSecondRoad()
    {
        Assert.True(false);
    }

    [Fact]
    public void PickVertex_BuildPhase()
    {
        // Arrange
        var gs = TestHelpers.CreateGameStateForSetUpPhase();
        var woodWool2BrickVertex = GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, TH.Wool2Tile, TH.WoodTile, TH.BrickTile, null);
        woodWool2BrickVertex.BuildSettlement(TH.HumanPlayer);
        var desertWoodGrainVertex = GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, TH.WoodTile, TH.GrainTile, TH.DesertTile, null);
        desertWoodGrainVertex.BuildSettlement(TH.BotPlayer);
        var woodWool5Edge = GamePlayHelpers.GetEdgeFromTileInfo(gs.Edges, TH.WoodTile, TH.Wool5Tile, null);
        woodWool5Edge.BuildRoad(TH.BotPlayer);
        gs.Phase.PhaseState = GameStates.BuildOrTrade;
        GamePlayHelpers.LinkEdgesAndVertices(gs);
        GamePlayHelpers.MarkBlockedVertices(gs);
        

        VertexPicker picker = new VertexPicker(gs);
        var expectedVertex1 = GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, TH.WoodTile, TH.Wool5Tile, TH.OreTile, null);

        // Act
        var selectedVertex = picker.PickVertex();

        // Assert - Test will accept the vertex with the highest probability of producing resources
        // and the highest probability of producing ore and other resources.
        Assert.NotNull(selectedVertex);
        Assert.Equal(expectedVertex1.Id, selectedVertex.Id);
    }

    [Fact]
    public void PickVertex_NoVerticesAccessible()
    {
        // This can happen if the other players box a player in completely (no new place to build).
        // Assumes this class is only used to determine locations to build settlements (not upgrades to cities).

        // Arrange
        var gs = TestHelpers.CreateGameStateForSetUpPhase();

        // Bot settlements are all in SE corner
        var oreSEVertex = GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, TH.OreTile, null, null, VertexDirection.SE);
        oreSEVertex.BuildSettlement(TH.BotPlayer);
        var oreSEEdge = GamePlayHelpers.GetEdgeFromTileInfo(gs.Edges, TH.OreTile, null, HexDirection.SE);
        oreSEEdge.BuildRoad(TH.BotPlayer);
        var wool5SEVertex = GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, TH.Wool5Tile, null, null, VertexDirection.SE);
        wool5SEVertex.BuildSettlement(TH.BotPlayer);
        var wool5SEEdge = GamePlayHelpers.GetEdgeFromTileInfo(gs.Edges, TH.Wool5Tile, null, HexDirection.SE);
        wool5SEEdge.BuildRoad(TH.BotPlayer);

        // Set up opponent to block all vertices from bot
        var woodOreBrickVertex = GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, TH.OreTile, TH.WoodTile, TH.BrickTile, null);
        woodOreBrickVertex.BuildSettlement(TH.HumanPlayer);
        var brickOreEdge = GamePlayHelpers.GetEdgeFromTileInfo(gs.Edges, TH.BrickTile, TH.OreTile, null);
        brickOreEdge.BuildRoad(TH.HumanPlayer);
        var oreNEEdge = GamePlayHelpers.GetEdgeFromTileInfo(gs.Edges, TH.OreTile, null, HexDirection.NE);
        oreNEEdge.BuildRoad(TH.HumanPlayer);
        var woodGrainWool5Vertex = GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, TH.WoodTile, TH.Wool5Tile, TH.GrainTile, null);
        woodGrainWool5Vertex.BuildSettlement(TH.HumanPlayer);
        var grainWool5Edge = GamePlayHelpers.GetEdgeFromTileInfo(gs.Edges, TH.GrainTile, TH.Wool5Tile, null);
        grainWool5Edge.BuildRoad(TH.HumanPlayer);
        var wool5SWEdge = GamePlayHelpers.GetEdgeFromTileInfo(gs.Edges, TH.Wool5Tile, null, HexDirection.SW);
        wool5SWEdge.BuildRoad(TH.HumanPlayer);

        gs.Phase.PhaseState = GameStates.BuildOrTrade;
        GamePlayHelpers.LinkEdgesAndVertices(gs);
        GamePlayHelpers.MarkBlockedVertices(gs);
        

        VertexPicker picker = new VertexPicker(gs);

        // Act
        var selectedVertex = picker.PickVertex();

        // There are no vertices accessible to the Bot
        Assert.Null(selectedVertex);
    }
}
