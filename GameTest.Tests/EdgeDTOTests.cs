using Xunit;
using GameTest.Models;
using GameTest.DTOs;

namespace GameTest.Tests;

public class EdgeDTOTests
{
    [Fact]
    public void Constructor_WithoutOwner2Neighbors()
    {
        // Arrange
        var tile1 = new Tile(ResourceType.Brick, 8, 0, 0);
        var tile2 = new Tile(ResourceType.Wool, 5, 1, -1);

        var edge = new Edge(tile1, tile2);

        // Act
        var dto = new EdgeDTO(edge);

        // Assert
        Assert.Equal(edge.Id, dto.Id);
        Assert.Equal(2, dto.TileIds.Count);
        Assert.Contains(tile1.Id, dto.TileIds);
        Assert.Contains(tile2.Id, dto.TileIds);
        Assert.Null(dto.Direction);
        Assert.Null(dto.PlayerId);
    }

    [Fact]
    public void Constructor_WithOwner2Neighbors()
    {
        // Arrange
        var tile1 = new Tile(ResourceType.Brick, 8, 0, 0);
        var tile2 = new Tile(ResourceType.Wool, 5, 1, -1);
        var edge = new Edge(tile1, tile2);
        var owner = new Player("Alice", PlayerColor.Red);
        var result = edge.BuildRoad(owner);
        Assert.True(result);

        // Act
        var dto = new EdgeDTO(edge);

        // Assert
        Assert.Equal(edge.Id, dto.Id);
        Assert.Equal(2, dto.TileIds.Count);
        Assert.Equal(owner.Id, dto.PlayerId);
        Assert.Contains(tile1.Id, dto.TileIds);
        Assert.Contains(tile2.Id, dto.TileIds);
        Assert.Null(dto.Direction);
    }

    [Fact]
    public void Constructor_WithOwner1Neighbor()
    {
        // Arrange
        var tile1 = new Tile(ResourceType.Brick, 8, 0, 0);
        var edge = new Edge(tile1, HexDirection.NE);
        var owner = new Player("Alice", PlayerColor.Red);
        var result = edge.BuildRoad(owner);
        Assert.True(result);

        // Act
        var dto = new EdgeDTO(edge);

        // Assert
        Assert.Equal(edge.Id, dto.Id);
        Assert.Single(dto.TileIds);
        Assert.Equal(owner.Id, dto.PlayerId);
        Assert.Contains(tile1.Id, dto.TileIds);
        Assert.Equal(HexDirection.NE, dto.Direction);
    }

    [Fact]
    public void Constructor_WithoutOwner1Neighbor()
    {
        // Arrange
        var tile1 = new Tile(ResourceType.Brick, 8, 0, 0);
        var edge = new Edge(tile1, HexDirection.NE);

        // Act
        var dto = new EdgeDTO(edge);

        // Assert
        Assert.Equal(edge.Id, dto.Id);
        Assert.Single(dto.TileIds);
        Assert.Null(dto.PlayerId);
        Assert.Contains(tile1.Id, dto.TileIds);
        Assert.Equal(HexDirection.NE, dto.Direction);
    }

}