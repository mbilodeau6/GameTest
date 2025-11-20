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
    public void CreateTilesForStarterBoard_CorrectTiles()
    {
        // Act
        var tiles = BoardCreationHelpers.CreateTilesForStarterBoard();

        // Assert
        ValidateGeneralRulesForDefaultBoard(tiles);
        Assert.Equal(ResourceType.Desert, tiles.GetTileAt(0, 0).Resource);
        Assert.Equal(ResourceType.Brick, tiles.GetTileAt(-1, -1).Resource);
        Assert.Equal(6, tiles.GetTileAt(-1, -1).DiceNumber);
        Assert.Equal(ResourceType.Grain, tiles.GetTileAt(0, 2).Resource);
        Assert.Equal(6, tiles.GetTileAt(0, 2).DiceNumber);
        Assert.Equal(ResourceType.Ore, tiles.GetTileAt(4, 0).Resource);
        Assert.Equal(8, tiles.GetTileAt(4, 0).DiceNumber);
        Assert.Equal(ResourceType.Wood, tiles.GetTileAt(-3, 1).Resource);
        Assert.Equal(8, tiles.GetTileAt(-3, 1).DiceNumber);
        Assert.Equal(ResourceType.Wool, tiles.GetTileAt(0, -2).Resource);
        Assert.Equal(2, tiles.GetTileAt(0, -2).DiceNumber);
    }

    [Fact]
    public void CreateTilesForTestBoard_CorrectTiles()
    {
        // Act
        var tiles = BoardCreationHelpers.CreateTilesForTestBoard();

        // Assert
        Assert.Equal(7, tiles.Count);
        Assert.Equal(ResourceType.Desert, tiles.GetTileAt(-1, -1).Resource);
        Assert.Equal(ResourceType.Wool, tiles.GetTileAt(1, -1).Resource);
        Assert.Equal(11, tiles.GetTileAt(1, -1).DiceNumber);
        Assert.Equal(ResourceType.Brick, tiles.GetTileAt(-2, 0).Resource);
        Assert.Equal(5, tiles.GetTileAt(-2, 0).DiceNumber);
        Assert.Equal(ResourceType.Grain, tiles.GetTileAt(0, 0).Resource);
        Assert.Equal(9, tiles.GetTileAt(0, 0).DiceNumber);
        Assert.Equal(ResourceType.Ore, tiles.GetTileAt(2, 0).Resource);
        Assert.Equal(3, tiles.GetTileAt(2, 0).DiceNumber);
        Assert.Equal(ResourceType.Wool, tiles.GetTileAt(-1, 1).Resource);
        Assert.Equal(2, tiles.GetTileAt(-1, 1).DiceNumber);
        Assert.Equal(ResourceType.Wood, tiles.GetTileAt(1, 1).Resource);
        Assert.Equal(6, tiles.GetTileAt(1, 1).DiceNumber);
    }

    [Fact]
    public void CreateEdgesAndVerticesForTestBoard_AllCreated()
    {
        // Act
        var gameState = BoardCreationHelpers.CreateNewBoard(GameType.Test);

        gameState.Players.Add(new Player("Alice", PlayerColor.Red));
        gameState.Players.Add(new Player("Bob", PlayerColor.Blue));

        // Assert
        Assert.Equal(30, gameState.Edges.Count);
        Assert.Equal(24, gameState.Vertices.Count);
        Assert.True(TestHelpers.IsGameStateValid(gameState));

        // Check specific edges and vertices
        // Check that inner tile at (0,0) has edges and vertices connected correctly
        var t1 = BoardCreationHelpers.GetTileAt(gameState.Tiles, 0, 0);
        var t2 = BoardCreationHelpers.GetTileAt(gameState.Tiles, 2, 0);
        var t3 = BoardCreationHelpers.GetTileAt(gameState.Tiles, 1, -1);
        Assert.NotNull(gameState.Edges.FirstOrDefault(e => e.Tiles.Contains(t1) && e.Tiles.Contains(t2)));
        Assert.NotNull(gameState.Vertices.FirstOrDefault(v => v.Tiles.Contains(t1) && v.Tiles.Contains(t2) && v.Tiles.Contains(t3)));

        // Check that corner tile at (-2,0) has edges and vertices connected correctly
        var t4 = BoardCreationHelpers.GetTileAt(gameState.Tiles, -2, 0);
        Assert.Equal(6, gameState.Edges.Count(e => e.Tiles.Contains(t4)));
        Assert.Equal(3, gameState.Edges.Count(e => e.Tiles.Contains(t4) && e.Direction != null));
        Assert.Equal(6, gameState.Vertices.Count(v => v.Tiles.Contains(t4)));
        Assert.Equal(2, gameState.Vertices.Count(v => v.Tiles.Contains(t4) && v.Direction != null));
        Assert.NotNull(gameState.Edges.FirstOrDefault(e => e.Tiles.Contains(t4) && e.Direction == HexDirection.W));
    }

    [Fact]
    public void CreateEdgesAndVerticesForStarterBoard_AllCreated()
    {
        // Act
        var gameState = BoardCreationHelpers.CreateNewBoard(GameType.Starter);

        gameState.Players.Add(new Player("Alice", PlayerColor.Red));
        gameState.Players.Add(new Player("Bob", PlayerColor.Blue));

        // Assert
        Assert.Equal(72, gameState.Edges.Count);
        Assert.Equal(54, gameState.Vertices.Count);
        Assert.True(TestHelpers.IsGameStateValid(gameState));

        // Check that corner tile at (0, 2) has edges and vertices connected correctly
        var t1 = BoardCreationHelpers.GetTileAt(gameState.Tiles, 0, 2);
        Assert.Equal(6, gameState.Edges.Count(e => e.Tiles.Contains(t1)));
        Assert.Equal(2, gameState.Edges.Count(e => e.Tiles.Contains(t1) && e.Direction != null));
        Assert.Equal(6, gameState.Vertices.Count(v => v.Tiles.Contains(t1)));
        var v1 = gameState.Vertices.FirstOrDefault(v => v.Tiles.Contains(t1) && v.Direction != null);
        Assert.NotNull(v1);
        Assert.Equal(VertexDirection.S, v1.Direction);
    }

    [Fact]
    public void CreateEdgesAndVerticesForDefaultBoard_AllCreated()
    {
        // Act
        var gameState = BoardCreationHelpers.CreateNewBoard(GameType.Default);

        gameState.Players.Add(new Player("Alice", PlayerColor.Red));
        gameState.Players.Add(new Player("Bob", PlayerColor.Blue));

        // Assert
        Assert.Equal(72, gameState.Edges.Count);
        Assert.Equal(54, gameState.Vertices.Count);
        Assert.True(TestHelpers.IsGameStateValid(gameState));

        // Check that corner tile at (0, 2) has edges and vertices connected correctly
        var t1 = BoardCreationHelpers.GetTileAt(gameState.Tiles, 0, 2);
        Assert.Equal(6, gameState.Edges.Count(e => e.Tiles.Contains(t1)));
        Assert.Equal(2, gameState.Edges.Count(e => e.Tiles.Contains(t1) && e.Direction != null));
        Assert.Equal(6, gameState.Vertices.Count(v => v.Tiles.Contains(t1)));
        var v1 = gameState.Vertices.FirstOrDefault(v => v.Tiles.Contains(t1) && v.Direction != null);
        Assert.NotNull(v1);
        Assert.Equal(VertexDirection.S, v1.Direction);
    }

    [Fact]
    public void AddPlayers_AddTwoPlayers()
    {
        // Arrange
        var gameState = new GameState(Guid.NewGuid(), GameType.Default);

        // Act
        BoardCreationHelpers.AddPlayers(gameState);

        // Assert
        Assert.Equal(2, gameState.Players.Count);
        Assert.Contains(gameState.Players, p => p.Name == "Lisa" && p.Color == PlayerColor.Red);
        Assert.Contains(gameState.Players, p => p.Name == "Hal" && p.Color == PlayerColor.Blue);
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
}