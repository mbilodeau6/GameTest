using Xunit;
using GameTest.Models;
using GameTest.DTOs;
using GameTest.Services;
using GameTest.Functions;
using Microsoft.VisualStudio.TestPlatform.Common.ExtensionFramework;
using System.Drawing.Printing;

namespace GameTest.Tests;

public class IntegrationTests
{
    private GameState CreateGameStateForGameLoopTesting()
    {
        var gs = new GameState(new Guid());
        gs.Tiles.AddRange(BoardCreationHelpers.CreateTilesForTestBoard());
        BoardCreationHelpers.CreateEdgesAndVerticesForBoard(gs);
        GamePlayHelpers.LinkEdgesAndVertices(gs);
        gs.AddPlayer(new Player("Lisa", PlayerColor.White));
        gs.AddPlayer(new Player("Hal", PlayerColor.Green, true));

        return gs;
    }

    private bool IsTooCloseToAnotherBuilding(GameState gs, Vertex vertex)
    {
        foreach (var edge in vertex.Edges)
        {
            foreach (var linkedVertex in edge.Vertices)
            {
                if (linkedVertex.Id != vertex.Id && GamePlayHelpers.HasBuilding(linkedVertex))
                    return true; 
            }
        }
        return false;
    }

    [Fact]
    public void BotBuildAtEndOfSetupPhase_TriggeredByBuildRoadByUser()
    {
        var gs = CreateGameStateForGameLoopTesting();
        gs.Phase = new GamePhase(GameStates.PlaceFirstSettlement, gs.Players[1], gs.Players[0]);

        var grainTile = BoardCreationHelpers.GetTileAt(gs.Tiles, 0, 0);
        var brickTile = BoardCreationHelpers.GetTileAt(gs.Tiles, -2, 0);
        var woolTile = BoardCreationHelpers.GetTileAt(gs.Tiles, -1, 1);
        var woodTile = BoardCreationHelpers.GetTileAt(gs.Tiles, 1, 1);
        var oreTile = BoardCreationHelpers.GetTileAt(gs.Tiles, 2, 0);

        // Build Bot's first settlement and road
        var v1 = GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, grainTile, brickTile, woolTile, null);
        var result = GamePlayHelpers.BuildSettlementRequestFromUser(gs, gs.Players[1].Id, v1.Id);
        Assert.Empty(result);
        // var e1 = GamePlayHelpers.GetEdgeFromTileInfo(gs.Edges, grainTile, brickTile, null);
        // result = GamePlayHelpers.BuildRoad(gs, gs.Players[1].Id, e1.Id);
        // Assert.Empty(result);

        // Build User's settlements and first road
        var v2 = GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, woodTile, oreTile, grainTile, null);
        result = GamePlayHelpers.BuildSettlementRequestFromUser(gs, gs.Players[0].Id, v2.Id);
        Assert.Empty(result);
        var e2 = GamePlayHelpers.GetEdgeFromTileInfo(gs.Edges, grainTile, oreTile, null);
        result = GamePlayHelpers.BuildRoadRequestFromUser(gs, gs.Players[0].Id, e2.Id);
        Assert.Empty(result);

        var v3 = GamePlayHelpers.GetVertexFromTileInfo(gs.Vertices, woodTile, null, null, VertexDirection.SE);
        result = GamePlayHelpers.BuildSettlementRequestFromUser(gs, gs.Players[0].Id, v3.Id);
        Assert.Empty(result);

        // Act
        // Build user's second road - this should trigger the bot to build its second settlement and road
        var e3 = GamePlayHelpers.GetEdgeFromTileInfo(gs.Edges, woodTile, null, HexDirection.SE);
        result = GamePlayHelpers.BuildRoadRequestFromUser(gs, gs.Players[0].Id, e3.Id);
        Assert.Empty(result);


        // Assert
        Assert.Equal(GameStates.RollOrUseDevCard, gs.Phase.PhaseState);
        Assert.Equal(gs.Players[0].Id, gs.Phase.CurrentPlayer.Id);

        // TODO: Figure out a way to know the type/count of vertices/edges. Will likely need
        // mock dice implemented.
        Assert.True(gs.Vertices.Count(v => v.Building == BuildingType.Settlement && v.Owner.Id == gs.Players[1].Id) >= 2);
        Assert.True(gs.Edges.Count(e => e.Owner != null && e.Owner.Id == gs.Players[1].Id) >= 2);

        foreach(var vertex in gs.Vertices)
        {
            if (GamePlayHelpers.HasBuilding(vertex))
                Assert.False(IsTooCloseToAnotherBuilding(gs, vertex));
        }
    }
}
