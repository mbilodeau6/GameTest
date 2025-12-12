using Xunit;
using GameTest.Models;
using GameTest.DTOs;

namespace GameTest.Tests;

public class PortDTOTests
{
    [Fact]
    public void Constructor_DTO()
    {
        // Arrange
        // Arrange
        var vertices = new List<Vertex>();
        var tile1 = new Tile(ResourceType.Wood, 8, 0, 0);
        var tile2 = new Tile(ResourceType.Brick, 9, 2, 0);
        var vertex1 = new Vertex(tile1, tile2);
        var vertex2 = new Vertex(tile2, VertexDirection.N);
        vertices.Add(vertex1);
        vertices.Add(vertex2);

        var expectedPort = new Port(vertex1, vertex2, PortType.Wood);

        // Act
        var portDto = new PortDTO(expectedPort);

        // Assert
        Assert.Equal(expectedPort.Id, portDto.Id);
        Assert.Equal(expectedPort.Vertices.Count, portDto.Vertices.Count);
        Assert.Contains(vertex1.Id, portDto.Vertices);
        Assert.Contains(vertex2.Id, portDto.Vertices);
        Assert.Equal(expectedPort.Type, portDto.Type);
    }
}