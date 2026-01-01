using Xunit;
using GameTest.Services;
using GameTest.Models;
using Microsoft.Net.Http.Headers;

namespace GameTest.Tests;

public class BoardCreationHelpersTests
{
    private void ValidateGeneralRulesForDefaultBoard(List<Tile> tiles)
    {
        Assert.Equal(19, tiles.Count);

        Dictionary<ResourceType, int> resourceCounts = new Dictionary<ResourceType, int>();
        Dictionary<int, int> diceCounts = new Dictionary<int, int>();

        foreach (var tile in tiles)
        {
            if (resourceCounts.ContainsKey(tile.Resource))
            {
                resourceCounts[tile.Resource]++;
            }
            else
            {
                resourceCounts[tile.Resource] = 1;
            }

            if (tile.Resource != ResourceType.Desert)
            {
                if (diceCounts.ContainsKey(tile.DiceNumber))
                {
                    diceCounts[tile.DiceNumber]++;
                }
                else
                {
                    diceCounts[tile.DiceNumber] = 1;
                }
            }
        }

        Assert.Equal(3, resourceCounts.GetValueOrDefault(ResourceType.Brick, 0));
        Assert.Equal(4, resourceCounts.GetValueOrDefault(ResourceType.Wood, 0));
        Assert.Equal(4, resourceCounts.GetValueOrDefault(ResourceType.Wool, 0));
        Assert.Equal(4, resourceCounts.GetValueOrDefault(ResourceType.Grain, 0));
        Assert.Equal(3, resourceCounts.GetValueOrDefault(ResourceType.Ore, 0));
        Assert.Equal(1, resourceCounts.GetValueOrDefault(ResourceType.Desert, 0));
        Assert.Equal(1, diceCounts.GetValueOrDefault(2, 0));
        Assert.Equal(2, diceCounts.GetValueOrDefault(3, 0));
        Assert.Equal(2, diceCounts.GetValueOrDefault(4, 0));
        Assert.Equal(2, diceCounts.GetValueOrDefault(5, 0));
        Assert.Equal(2, diceCounts.GetValueOrDefault(6, 0));
        Assert.Equal(2, diceCounts.GetValueOrDefault(8, 0));
        Assert.Equal(2, diceCounts.GetValueOrDefault(9, 0));
        Assert.Equal(2, diceCounts.GetValueOrDefault(10, 0));
        Assert.Equal(2, diceCounts.GetValueOrDefault(11, 0));
        Assert.Equal(1, diceCounts.GetValueOrDefault(12, 0));
        Assert.False(diceCounts.ContainsKey(7));
        Assert.False(diceCounts.ContainsKey(0));
    }

    [Fact]
    public void CreateTilesForRandomBoard_CorrectTiles()
    {
        // Act
        var tiles = BoardCreationHelpers.CreateTilesForRandomBoard();

        // Assert
        ValidateGeneralRulesForDefaultBoard(tiles);
    }

    [Fact]
    public void CreateEdgesAndVerticesForStarterBoard_AllCreated()
    {
        // Act
        var gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter);

        gs.Players.Add(Player.CreateTestPlayer("Alice", PlayerColor.Red));
        gs.Players.Add(Player.CreateTestPlayer("Bob", PlayerColor.Blue));

        // Assert
        Assert.Equal(72, gs.Edges.Count);
        Assert.Equal(54, gs.Vertices.Count);
        Assert.True(TestHelpers.IsGameStateValid(gs));

        // Check that corner tile at (0, 2) has edges and vertices connected correctly
        var t1 = gs.GetTileAt(0, 2);
        Assert.Equal(6, gs.Edges.Count(e => e.Tiles.Contains(t1)));
        Assert.Equal(2, gs.Edges.Count(e => e.Tiles.Contains(t1) && e.Direction != null));
        Assert.Equal(6, gs.Vertices.Count(v => v.Tiles.Contains(t1)));
        var v1 = gs.Vertices.FirstOrDefault(v => v.Tiles.Contains(t1) && v.Direction != null);
        Assert.NotNull(v1);
        Assert.Equal(VertexDirection.S, v1.Direction);
    }

    [Fact]
    public void CreateEdgesAndVerticesForDefaultBoard_AllCreated()
    {
        // Act
        var gs = BoardCreationHelpers.CreateNewBoard(GameType.Default);

        gs.Players.Add(Player.CreateTestPlayer("Alice", PlayerColor.Red));
        gs.Players.Add(Player.CreateTestPlayer("Bob", PlayerColor.Blue));

        // Assert
        Assert.Equal(72, gs.Edges.Count);
        Assert.Equal(54, gs.Vertices.Count);
        Assert.True(TestHelpers.IsGameStateValid(gs));

        // Check that corner tile at (0, 2) has edges and vertices connected correctly
        var t1 = gs.GetTileAt(0, 2);
        Assert.Equal(6, gs.Edges.Count(e => e.Tiles.Contains(t1)));
        Assert.Equal(2, gs.Edges.Count(e => e.Tiles.Contains(t1) && e.Direction != null));
        Assert.Equal(6, gs.Vertices.Count(v => v.Tiles.Contains(t1)));
        var v1 = gs.Vertices.FirstOrDefault(v => v.Tiles.Contains(t1) && v.Direction != null);
        Assert.NotNull(v1);
        Assert.Equal(VertexDirection.S, v1.Direction);
    }

    private void ValidatePortsForStandardBoard(GameState gs)
    {
        // Make sure each port type is included
        Assert.Equal(9, gs.Ports.Count);
        Assert.Equal(4, gs.Ports.Count(p => p.Type == PortType.ThreeToOne));
        Assert.Single(gs.Ports, p => p.Type == PortType.Wood);
        Assert.Single(gs.Ports, p => p.Type == PortType.Brick);
        Assert.Single(gs.Ports, p => p.Type == PortType.Grain);
        Assert.Single(gs.Ports, p => p.Type == PortType.Wool);
        Assert.Single(gs.Ports, p => p.Type == PortType.Ore);

        var referencedVertexIds = new HashSet<string>();
        
        // Make sure each port has two vertices, that the vertices are on the water,
        // and that no vertex is referenced more than once.
        // TODO: Make sure the ports are evenly spaced.
        foreach (var port in gs.Ports)
        {
            Assert.Equal(2, port.Vertices.Count);
            foreach (var vertex in port.Vertices)
            {
                Assert.DoesNotContain(vertex.Id, referencedVertexIds);
                referencedVertexIds.Add(vertex.Id);
                Assert.True(vertex.Tiles.Count >= 1 && vertex.Tiles.Count <= 2);
            }
        }
    }

    [Fact]
    public void AddPorts_UnsupportedGameType()
    {
        GameState gs = new GameState(new Guid(), GameType.Test);
        Assert.Throws<InvalidOperationException>(() => BoardCreationHelpers.AddPorts(gs));
    }

    [Fact]
    public void AddPorts_Valid()
    {
        GameState gs = BoardCreationHelpers.CreateNewBoard(GameType.Default);
        ValidatePortsForStandardBoard(gs);
    }


    [Fact]
    public void AddPortsForStarter_UnsupportedGameType()
    {
        GameState gs = new GameState(new Guid());
        Assert.Throws<InvalidOperationException>(() => BoardCreationHelpers.AddPortsForStarter(gs));
    }

    [Fact]
    public void AddPortsForStarter_Valid()
    {
        GameState gs = BoardCreationHelpers.CreateNewBoard(GameType.Starter);
        ValidatePortsForStandardBoard(gs);
    }

    [Fact]
    public void CreateListOfPorts_Valid()
    {
        // Act
        List<PortType> ports = BoardCreationHelpers.CreateListOfPorts();

        // Assert
        Assert.Equal(9, ports.Count);
        Assert.Equal(4, ports.Count(p => p == PortType.ThreeToOne));
        Assert.Single(ports, p => p == PortType.Wood);
        Assert.Single(ports, p => p == PortType.Brick);
        Assert.Single(ports, p => p == PortType.Grain);
        Assert.Single(ports, p => p == PortType.Wool);
        Assert.Single(ports, p => p == PortType.Ore);
    }

    private static GameState CreateBoardForPortTesting()
    {
        var gs = new GameState(Guid.NewGuid(), GameType.Starter);

        foreach (var tile in BoardCreationHelpers.CreateTilesForStarterBoard())
            gs.AddTile(tile);

        BoardCreationHelpers.CreateEdgesAndVerticesForBoard(gs);
        BoardCreationHelpers.LinkEdgesAndVertices(gs);

        return gs;
    }

    [Fact]
    public void AddPortsWithStartIndex_StartAtZeroWithSubIndexZero()
    {
        // Arrange
        GameState gs = CreateBoardForPortTesting();

        // Act
        BoardCreationHelpers.AddPortsWithStartIndex(gs, BoardCreationHelpers.CreateListOfPorts(), 0, 0);

        // Assert
        ValidatePortsForStandardBoard(gs);

        // Check to make sure first few ports are where expected
        var port = gs.Ports[0];
        var tile1 = gs.GetTileAt(2, -2);
        var vertex1 = gs.GetVertexFromTileInfo(tile1, null, null, VertexDirection.N);
        var vertex2 = gs.GetVertexFromTileInfo(tile1, null, null, VertexDirection.NE);
        Assert.Equal(PortType.ThreeToOne, port.Type);
        Assert.Contains(port.Vertices, v => v.Id == vertex1.Id);
        Assert.Contains(port.Vertices, v => v.Id == vertex2.Id);

        port = gs.Ports[1];
        tile1 = gs.GetTileAt(3, -1);
        var tile2 = gs.GetTileAt(4, 0);
        vertex1 = gs.GetVertexFromTileInfo(tile1, null, null, VertexDirection.NE);
        vertex2 = gs.GetVertexFromTileInfo(tile1, tile2, null, null);
        Assert.Equal(PortType.Wood, port.Type);
        Assert.Contains(port.Vertices, v => v.Id == vertex1.Id);
        Assert.Contains(port.Vertices, v => v.Id == vertex2.Id);

        port = gs.Ports[2];
        tile1 = gs.GetTileAt(4, 0);
        tile2 = gs.GetTileAt(3, 1);
        vertex1 = gs.GetVertexFromTileInfo(tile1, null, null, VertexDirection.SE);
        vertex2 = gs.GetVertexFromTileInfo(tile1, tile2, null, null);
        Assert.Equal(PortType.Brick, port.Type);
        Assert.Contains(port.Vertices, v => v.Id == vertex1.Id);
        Assert.Contains(port.Vertices, v => v.Id == vertex2.Id);

        port = gs.Ports[3];
        tile1 = gs.GetTileAt(2, 2);
        vertex1 = gs.GetVertexFromTileInfo(tile1, null, null, VertexDirection.SE);
        vertex2 = gs.GetVertexFromTileInfo(tile1, null, null, VertexDirection.S);
        Assert.Equal(PortType.Ore, port.Type);
        Assert.Contains(port.Vertices, v => v.Id == vertex1.Id);
        Assert.Contains(port.Vertices, v => v.Id == vertex2.Id);

        port = gs.Ports[4];
        tile1 = gs.GetTileAt(0, 2);
        tile2 = gs.GetTileAt(-2, 2);
        vertex1 = gs.GetVertexFromTileInfo(tile1, null, null, VertexDirection.S);
        vertex2 = gs.GetVertexFromTileInfo(tile1, tile2, null, null);
        Assert.Equal(PortType.Grain, port.Type);
        Assert.Contains(port.Vertices, v => v.Id == vertex1.Id);
        Assert.Contains(port.Vertices, v => v.Id == vertex2.Id);
    }

    [Fact]
    public void AddPortsWithStartIndex_StartAtZevenWithSubIndexOne()
    {
        // Arrange
        GameState gs = CreateBoardForPortTesting();

        // Act
        BoardCreationHelpers.AddPortsWithStartIndex(gs, BoardCreationHelpers.CreateListOfPorts(), 14, 1);

        // Assert
        ValidatePortsForStandardBoard(gs);
        
        // Check to make sure first few ports are where expected
        var port = gs.Ports[0];
        var tile1 = gs.GetTileAt(-3, -1);
        var tile2 = gs.GetTileAt(-2, -2);
        var vertex1 = gs.GetVertexFromTileInfo(tile1, tile2, null, null);
        var vertex2 = gs.GetVertexFromTileInfo(tile2, null, null, VertexDirection.NW);
        Assert.Equal(PortType.ThreeToOne, port.Type);
        Assert.Contains(port.Vertices, v => v.Id == vertex1.Id);
        Assert.Contains(port.Vertices, v => v.Id == vertex2.Id);

        port = gs.Ports[1];
        tile1 = gs.GetTileAt(-2, -2);
        tile2 = gs.GetTileAt(0, -2);
        vertex1 = gs.GetVertexFromTileInfo(tile1, tile2, null, null);
        vertex2 = gs.GetVertexFromTileInfo(tile2, null, null, VertexDirection.N);
        Assert.Equal(PortType.Wood, port.Type);
        Assert.Contains(port.Vertices, v => v.Id == vertex1.Id);
        Assert.Contains(port.Vertices, v => v.Id == vertex2.Id);

        port = gs.Ports[2];
        tile1 = gs.GetTileAt(2, -2);
        vertex1 = gs.GetVertexFromTileInfo(tile1, null, null, VertexDirection.N);
        vertex2 = gs.GetVertexFromTileInfo(tile1, null, null, VertexDirection.NE);
        Assert.Equal(PortType.Brick, port.Type);
        Assert.Contains(port.Vertices, v => v.Id == vertex1.Id);
        Assert.Contains(port.Vertices, v => v.Id == vertex2.Id);

        port = gs.Ports[3];
        tile1 = gs.GetTileAt(3, -1);
        tile2 = gs.GetTileAt(4, 0);
        vertex1 = gs.GetVertexFromTileInfo(tile1, tile2, null, null);
        vertex2 = gs.GetVertexFromTileInfo(tile2, null, null, VertexDirection.NE);
        Assert.Equal(PortType.Ore, port.Type);
        Assert.Contains(port.Vertices, v => v.Id == vertex1.Id);
        Assert.Contains(port.Vertices, v => v.Id == vertex2.Id);
    }

}