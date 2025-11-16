using Xunit;
using GameTest.Models;
using GameTest.Services;

namespace GameTest.Tests;

public class VertexPickerTests
{
    private static Tile WoodTile = new Tile(ResourceType.Wood, 9, 0, 0);
    private static Tile Wool2Tile = new Tile(ResourceType.Wool, 2, -1, -1);
    private static Tile BrickTile = new Tile(ResourceType.Brick, 3, 1, -1);
    private static Tile OreTile = new Tile(ResourceType.Ore, 4, 2, 0);
    private static Tile Wool5Tile = new Tile(ResourceType.Wool, 5, 1, 1);
    private static Tile GrainTile = new Tile(ResourceType.Grain, 6, -1, 1);
    private static Tile DesertTile = new Tile(ResourceType.Desert, 0, -2, 0);
    private static Player HumanPlayer = new Player("Player1", PlayerColor.Red, isBot: false);
    private static Player BotPlayer = new Player("Player2", PlayerColor.Blue, isBot: true);

    private GameState CreateGameStateForSetUpPhase()
    {
        var gs = new GameState(new Guid());
        gs.Phase.PhaseState = GameStates.PlaceFirstSettlement;
        gs.Phase.CurrentPlayer = BotPlayer;

        gs.Tiles.AddRange(new List<Tile>() {
            WoodTile,
            Wool2Tile,
            BrickTile,
            OreTile,
            Wool5Tile,
            GrainTile,
            DesertTile
        });

        BoardCreationHelpers.CreateEdgesAndVerticesForBoard(gs);

        gs.AddPlayer(HumanPlayer);
        gs.AddPlayer(BotPlayer);

        return gs;
    }


    // TODO: Need to identify specific scenarios to test
    [Fact]
    public void PickVertex_SetUpPhaseOpenBoard()
    {
        // Arrange
        var gs = CreateGameStateForSetUpPhase();

        VertexPicker picker = new VertexPicker(gs);
        var expectedVertex1 = GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, WoodTile, GrainTile, Wool5Tile, null);
        var expectedVertex2 = GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, Wool5Tile, WoodTile, OreTile, null);

        // Act
        var selectedVertex = picker.PickVertex();

        // Assert - Test will accept the vertex with the highest probability of producing resources
        // and the highest probability of producing ore and other resources.
        Assert.True(selectedVertex.Id == expectedVertex1.Id || selectedVertex.Id == expectedVertex2.Id);
    }

    [Fact]
    public void PickVertex_SetUpPhaseBestSpotsUnavailable()
    {
        // Arrange
        var gs = CreateGameStateForSetUpPhase();
        var oreWoodWool5Vertex = GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, Wool5Tile, WoodTile, OreTile, null);
        oreWoodWool5Vertex.BuildSettlement(HumanPlayer);
        GamePlayHelpers.LinkEdgesAndVertices(gs);
        GamePlayHelpers.MarkBlockedVertices(gs, oreWoodWool5Vertex);
        

        VertexPicker picker = new VertexPicker(gs);
        var expectedVertex1 = GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, WoodTile, BrickTile, OreTile, null);
        var expectedVertex2 = GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, GrainTile, WoodTile, DesertTile, null);

        // Act
        var selectedVertex = picker.PickVertex();

        // Assert - Test will accept the vertex with the highest probability of producing resources
        // and the highest probability of producing ore and other resources.
        Assert.True(selectedVertex.Id == expectedVertex1.Id || selectedVertex.Id == expectedVertex2.Id);
    }

    [Fact]
    public void PickVertex_BuildPhase()
    {
        Assert.True(false);
    }
    [Fact]

    public void PickVertex_NoVerticesAccessible()
    {
        // This can happen if the other players box a player in completely (no new place to build).
        // Assumes this class is only used to determine locations to build settlements (not upgrades to cities).
        Assert.True(false);
    }
}
