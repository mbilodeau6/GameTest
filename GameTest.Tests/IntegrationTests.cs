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
        gs.AddPlayer(new Player("List", PlayerColor.White));
        gs.AddPlayer(new Player("Hal", PlayerColor.Green, true));

        return gs;
    }

    private bool IsTooCloseToAnotherBuilding(GameState gs, Vertex vertex)
    {
        foreach (var edge in vertex.Edges)
        {
            foreach (var linkedVertex in edge.Vertices)
            {
                if (linkedVertex.Id != vertex.Id && (linkedVertex.Building == BuildingType.Settlement || linkedVertex.Building == BuildingType.City))
                    return true; 
            }
        }
        return false;
    }

    [Fact]
    public void BotBuildAtEndOfSetupPhase_TriggeredByBuildSettlementByUser()
    {
        var gs = CreateGameStateForGameLoopTesting();
        GamePlayHelpers.BuildSettlement(gs, gs.Players[1].Id, gs.Vertices[0].Id);
        gs.Edges[0].BuildRoad(gs.Players[1]);
        gs.Phase = new GamePhase(GameStates.PlaceSecondSettlement, gs.Players[1], gs.Players[1]);

        GamePlayHelpers.GameLoop(gs);

        Assert.Equal(GameStates.RollOrUseDevCard, gs.Phase.PhaseState);
        Assert.Equal(gs.Players[0].Id, gs.Phase.CurrentPlayer.Id);

        // TODO: Figure out a way to know the type/count of vertices/edges. Will likely need
        // mock dice implemented.
        Assert.True(gs.Vertices.Count(v => v.Building == BuildingType.Settlement && v.Owner.Id == gs.Players[1].Id) >= 2);
        Assert.True(gs.Edges.Count(e => e.Owner != null && e.Owner.Id == gs.Players[1].Id) >= 2);

        foreach(var vertex in gs.Vertices)
        {
            if (vertex.Building == BuildingType.Settlement || vertex.Building == BuildingType.City)
                Assert.False(IsTooCloseToAnotherBuilding(gs, vertex));
        }
    }
}
