using Xunit;
using GameTest.Models;
using GameTest.DTOs;

namespace GameTest.Tests;

public class VertexDTOTests
{
    [Fact]
    public void Constructor_WithoutOwner3Neighbors()
    {
        // Arrange
        var tile1 = new Tile(ResourceType.Brick, 8, 0, 0);
        var tile2 = new Tile(ResourceType.Wool, 5, 1, -1);
        var tile3 = new Tile(ResourceType.Ore, 10, 2, 0);

        var vertex = new Vertex(tile1, tile2, tile3);

        // Act
        var dto = new VertexDTO(vertex);

        // Assert
        Assert.Equal(vertex.Id, dto.Id);
        Assert.Equal(3, dto.TileIds.Count);
        Assert.Contains(tile1.Id, dto.TileIds);
        Assert.Contains(tile2.Id, dto.TileIds);
        Assert.Contains(tile3.Id, dto.TileIds);
        Assert.Null(dto.Building);
        Assert.Null(dto.PlayerId);
        Assert.Null(dto.Direction);
    }

    [Fact]
    public void Constructor_WithoutOwner2Neighbors()
    {
        // Arrange
        var tile1 = new Tile(ResourceType.Brick, 8, 0, 0);
        var tile2 = new Tile(ResourceType.Wool, 5, 1, -1);

        var vertex = new Vertex(tile1, tile2);

        // Act
        var dto = new VertexDTO(vertex);

        // Assert
        Assert.Equal(vertex.Id, dto.Id);
        Assert.Equal(2, dto.TileIds.Count);
        Assert.Contains(tile1.Id, dto.TileIds);
        Assert.Contains(tile2.Id, dto.TileIds);
        Assert.Null(dto.Building);
        Assert.Null(dto.PlayerId);
        Assert.Null(dto.Direction);
    }

    [Fact]
    public void Constructor_WithoutOwner1Neighbors()
    {
        // Arrange
        var tile1 = new Tile(ResourceType.Brick, 8, 0, 0);

        var vertex = new Vertex(tile1, VertexDirection.N);

        // Act
        var dto = new VertexDTO(vertex);

        // Assert
        Assert.Equal(vertex.Id, dto.Id);
        Assert.Single(dto.TileIds);
        Assert.Contains(tile1.Id, dto.TileIds);
        Assert.Null(dto.Building);
        Assert.Null(dto.PlayerId);
        Assert.Equal(VertexDirection.N, dto.Direction);
    }

    [Fact]
    public void Constructor_WithOwner3Neighbors()
    {
        // Arrange
        var tile1 = new Tile(ResourceType.Brick, 8, 0, 0);
        var tile2 = new Tile(ResourceType.Wool, 5, 1, -1);
        var tile3 = new Tile(ResourceType.Ore, 10, 2, 0);

        var vertex = new Vertex(tile1, tile2, tile3);
        var owner = new Player("Alice", PlayerColor.Red);
        vertex.BuildSettlement(owner);

        // Act
        var dto = new VertexDTO(vertex);

        // Assert
        Assert.Equal(vertex.Id, dto.Id);
        Assert.Equal(3, dto.TileIds.Count);
        Assert.Contains(tile1.Id, dto.TileIds);
        Assert.Contains(tile2.Id, dto.TileIds);
        Assert.Contains(tile3.Id, dto.TileIds);
        Assert.Equal(BuildingType.Settlement, dto.Building);
        Assert.Equal(owner.Id, dto.PlayerId);
        Assert.Null(dto.Direction);
    }

        [Fact]
    public void Constructor_WithOwner2Neighbors()
    {
        // Arrange
        var tile1 = new Tile(ResourceType.Brick, 8, 0, 0);
        var tile2 = new Tile(ResourceType.Wool, 5, 1, -1);

        var vertex = new Vertex(tile1, tile2);
        var owner = new Player("Alice", PlayerColor.Red);
        vertex.BuildSettlement(owner);
        vertex.UpgradeToCity();

        // Act
        var dto = new VertexDTO(vertex);

        // Assert
        Assert.Equal(vertex.Id, dto.Id);
        Assert.Equal(2, dto.TileIds.Count);
        Assert.Contains(tile1.Id, dto.TileIds);
        Assert.Contains(tile2.Id, dto.TileIds);
        Assert.Equal(BuildingType.City, dto.Building);
        Assert.Equal(owner.Id, dto.PlayerId);
        Assert.Null(dto.Direction);
    }

    [Fact]
    public void Constructor_WithOwner1Neighbors()
    {
        // Arrange
        var tile1 = new Tile(ResourceType.Brick, 8, 0, 0);

        var vertex = new Vertex(tile1, VertexDirection.N);
        var owner = new Player("Alice", PlayerColor.Red);
        vertex.BuildSettlement(owner);

        // Act
        var dto = new VertexDTO(vertex);

        // Assert
        Assert.Equal(vertex.Id, dto.Id);
        Assert.Single(dto.TileIds);
        Assert.Contains(tile1.Id, dto.TileIds);
        Assert.Equal(BuildingType.Settlement, dto.Building);
        Assert.Equal(owner.Id, dto.PlayerId);
        Assert.Equal(VertexDirection.N, dto.Direction);
    }
}