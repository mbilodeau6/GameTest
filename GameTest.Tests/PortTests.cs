using Xunit;
using GameTest.Models;
using GameTest.DTOs;
using Microsoft.Identity.Client;

namespace GameTest.Tests;

public class PortTests
{
    [Fact]
    public void Constructor_Vertex1Null_Exception()
    {
        // Arrange
        var tile1 = new Tile(ResourceType.Wood, 8, 0, 0);
        var tile2 = new Tile(ResourceType.Brick, 9, 2, 0);
        var vertex = new Vertex(tile1, tile2);

        // Act & Assert
         Assert.Throws<ArgumentNullException>(() => new Port(null!, vertex, PortType.Wood));
    }

    [Fact]
    public void Constructor_Vertex2Null_Exception()
    {
        // Arrange
        var tile1 = new Tile(ResourceType.Wood, 8, 0, 0);
        var tile2 = new Tile(ResourceType.Brick, 9, 2, 0);
        var vertex = new Vertex(tile1, tile2);

        // Act & Assert
         Assert.Throws<ArgumentNullException>(() => new Port(vertex, null!, PortType.Wood));
    }

    [Fact]
    public void Constructor_VerticesEqual_Exception()
    {
        // Arrange
        var tile1 = new Tile(ResourceType.Wood, 8, 0, 0);
        var tile2 = new Tile(ResourceType.Brick, 9, 2, 0);
        var vertex = new Vertex(tile1, tile2);

        // Act & Assert
         Assert.Throws<ArgumentException>(() => new Port(vertex, vertex, PortType.Wood));
    }

    [Fact]
    public void Constructor_Valid()
    {
        // Arrange
        var tile1 = new Tile(ResourceType.Wood, 8, 0, 0);
        var tile2 = new Tile(ResourceType.Brick, 9, 2, 0);
        var vertex1 = new Vertex(tile1, tile2);
        var vertex2 = new Vertex(tile2, VertexDirection.N);

        // Act
         var port = new Port(vertex1, vertex2, PortType.Wood);

        // Assert
        Assert.NotNull(port);
        Assert.Equal(2, port.Vertices.Count);
        Assert.Contains(vertex1, port.Vertices);
        Assert.Contains(vertex2, port.Vertices);
        Assert.Equal(PortType.Wood, port.Type);
        Assert.NotEmpty(port.Id);
    }

    [Fact]
    public void Constructor_DTO_Valid()
    {
        // Arrange
        var vertices = new List<Vertex>();
        var tile1 = new Tile(ResourceType.Wood, 8, 0, 0);
        var tile2 = new Tile(ResourceType.Brick, 9, 2, 0);
        var vertex1 = new Vertex(tile1, tile2);
        var vertex2 = new Vertex(tile2, VertexDirection.N);
        vertices.Add(vertex1);
        vertices.Add(vertex2);

        var expectedPort = new Port(vertex1, vertex2, PortType.Wood);
        var portDto = new PortDTO(expectedPort);

        // Act
        var port = new Port(portDto, vertices);

        // Assert
        Assert.Equal(expectedPort.Id, port.Id);
        Assert.Equal(expectedPort.Vertices.Count, port.Vertices.Count);
        Assert.Contains(vertex1, port.Vertices);
        Assert.Contains(vertex2, port.Vertices);
        Assert.Equal(expectedPort.Type, port.Type);
    }
}