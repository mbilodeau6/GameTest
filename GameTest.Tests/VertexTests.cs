using Xunit;
using GameTest.Models;
using GameTest.DTOs;

namespace GameTest.Tests;

public class VertexTests
{
    [Fact]
    public void Constructor_VertexOfSingleTile_CreatesExpectedVertex()
    {
        // Arrange
        var tile = new Tile(ResourceType.Brick, 8, 0, 0);

        // Act
        var vertex = new Vertex(tile, VertexDirection.N);

        // Assert
        Assert.True(TestHelpers.ValidateId(vertex.Id, 'V'));
        Assert.Single(vertex.Tiles);
        Assert.All(vertex.Tiles, t => Assert.NotNull(t));
    }

    [Fact]
    public void Constructor_VertexWith2Tiles_CreatesExpectedVertex()
    {
        // Arrange
        var t1 = new Tile(ResourceType.Brick, 8, 0, 0);
        var t2 = new Tile(ResourceType.Ore, 6, 1, 0);

        // Act
        var vertex = new Vertex(t1, t2);

        // Assert
        Assert.True(TestHelpers.ValidateId(vertex.Id, 'V'));
        Assert.Equal(2, vertex.Tiles.Count);
        Assert.All(vertex.Tiles, t => Assert.NotNull(t));
    }

    [Fact]
    public void Constructor_VertexWith3Tiles_CreatesExpectedVertex()
    {
        // Arrange
        var t1 = new Tile(ResourceType.Brick, 2, 0, 0);
        var t2 = new Tile(ResourceType.Ore, 6, 1, 0);
        var t3 = new Tile(ResourceType.Wood, 4, 0, 1);

        // Act
        var vertex = new Vertex(t1, t2, t3);

        // Assert
        Assert.True(TestHelpers.ValidateId(vertex.Id, 'V'));
        Assert.Equal(3, vertex.Tiles.Count);
        Assert.All(vertex.Tiles, t => Assert.NotNull(t));
    }

    [Fact]
    public void Constructor_FromDTO_CreatesValidObject()
    {
        // Arrange
        var t1 = new Tile(ResourceType.Brick, 3, -3, -1);
        var expectedVertex = new Vertex(t1, VertexDirection.S);
        var p1 = new Player("Alice", PlayerColor.Blue);
        var p2 = new Player("Bob", PlayerColor.Red);
        expectedVertex.BuildSettlement(p2);

        var vertexDto = new VertexDTO(expectedVertex);

        // Act
        var vertex = new Vertex(vertexDto, new List<Player>() { p1, p2 }, new List<Tile> { t1 });

        // Assert
        Assert.Equal(expectedVertex.Id, vertex.Id);
        Assert.True(vertex.Tiles.Count == expectedVertex.Tiles.Count);
        Assert.Contains(vertex.Tiles, t => t.Id == t1.Id);
        Assert.Equal(BuildingType.Settlement, vertex.Building);

        Assert.NotNull(vertex.Owner);
        Assert.Equal(p2.Id, vertex.Owner.Id);

        Assert.Equal(VertexDirection.S, vertex.Direction);
    }

    [Fact]
    public void BuildSettlement_EmptyVertex_SetsOwnerAndBuilding()
    {
        // Arrange
        var player = new Player("Alice", PlayerColor.Blue);
        var t1 = new Tile(ResourceType.Brick, 8, 0, 0);
        var t2 = new Tile(ResourceType.Grain, 8, -1, -1);
        var vertex = new Vertex(t1, t2);

        // Act
        vertex.BuildSettlement(player);

        // Assert
        Assert.Equal(player, vertex.Owner);
        Assert.Equal(BuildingType.Settlement, vertex.Building);
    }

    [Fact]
    public void BuildSettlement_VertexHasSettlement_ThrowsException()
    {
        // Arrange
        var p1 = new Player("Alice", PlayerColor.Blue);
        var p2 = new Player("Bob", PlayerColor.Red);

        var t1 = new Tile(ResourceType.Brick, 8, 0, 0);
        var t2 = new Tile(ResourceType.Grain, 8, -1, -1);
        var vertex = new Vertex(t1, t2);
        vertex.BuildSettlement(p1);

        // Act
        var exception = Assert.Throws<InvalidOperationException>(() => vertex.BuildSettlement(p2));
        Assert.Equal("A building already exists on this vertex.", exception.Message);

        // Assert
        Assert.Equal(p1, vertex.Owner);
        Assert.Equal(BuildingType.Settlement, vertex.Building);
    }

    [Fact]
    public void BuildSettlement_VertexHasCity_ThrowsException()
    {
        // Arrange
        var p1 = new Player("Alice", PlayerColor.Blue);

        var t1 = new Tile(ResourceType.Brick, 8, 0, 0);
        var vertex = new Vertex(t1, VertexDirection.N);
        vertex.BuildSettlement(p1);
        vertex.UpgradeToCity();

        // Act
        var exception = Assert.Throws<InvalidOperationException>(() => vertex.BuildSettlement(p1));
        Assert.Equal("A building already exists on this vertex.", exception.Message);

        // Assert
        Assert.Equal(p1, vertex.Owner);
        Assert.Equal(BuildingType.City, vertex.Building);
    }

    [Fact]
    public void UpgradeToCity_ExistingSettlement_UpgradesBuildingToCity()
    {
        // Arrange
        var player = new Player("Alice", PlayerColor.Blue);
        var tile = new Tile(ResourceType.Brick, 8, 0, 0);
        var vertex = new Vertex(tile, VertexDirection.S);
        vertex.BuildSettlement(player);

        // Act
        vertex.UpgradeToCity();

        // Assert
        Assert.Equal(BuildingType.City, vertex.Building);
    }

    [Fact]
    public void UpgradeToCity_ExistingCity_ThrowsInvalidOperationException()
    {
        // Arrange
        var player = new Player("Alice", PlayerColor.Blue);
        var tile = new Tile(ResourceType.Brick, 8, 0, 0);
        var vertex = new Vertex(tile, VertexDirection.SW);
        vertex.BuildSettlement(player);
        vertex.UpgradeToCity();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => vertex.UpgradeToCity());
        Assert.Equal("Only a settlement can be upgraded to a city.", exception.Message);
    }

    [Fact]
    public void DowngradeToSettlement_ExistingSettlement_ThrowsException()
    {
        // Arrange
        var player = new Player("Alice", PlayerColor.Blue);
        var tile = new Tile(ResourceType.Brick, 8, 0, 0);
        var vertex = new Vertex(tile, VertexDirection.SE);
        vertex.BuildSettlement(player);

        // Act
        var exception = Assert.Throws<InvalidOperationException>(() => vertex.DowngradeToSettlement());
        Assert.Equal("Only a city can be downgraded to a settlement.", exception.Message);
    }

    [Fact]
    public void DowngradeToSettlement_ExistingCity_SetsBuildingToSettlement()
    {
        // Arrange
        var player = new Player("Alice", PlayerColor.Blue);
        var tile = new Tile(ResourceType.Brick, 8, 0, 0);
        var vertex = new Vertex(tile, VertexDirection.S);
        vertex.BuildSettlement(player);
        vertex.UpgradeToCity();

        // Act
        vertex.DowngradeToSettlement();

        // Assert
        Assert.Equal(BuildingType.Settlement, vertex.Building);
    }

    [Fact]
    public void ToString_NoParameters_ReturnsNonEmptyString()
    {
        // Arrange
        var player = new Player("Bob", PlayerColor.Green);
        var tile = new Tile(ResourceType.Brick, 8, 0, 0);
        var vertex = new Vertex(tile, VertexDirection.N);
        vertex.BuildSettlement(player);

        // Act
        var result = vertex.ToString();

        // Assert
        Assert.NotNull(result);

        Assert.NotNull(vertex.Owner);
        string expectedResult = $"Vertex {vertex.Id} (Owner: {vertex.Owner.Name}; Building: ";

        if (vertex.Building == BuildingType.Settlement)
            expectedResult += "Settlement)";

        if (vertex.Building == BuildingType.City)
            expectedResult += "City)";

        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void ConnectsTiles_OnlyTwo()
    {
        // Arrange
        var tile1 = new Tile(ResourceType.Brick, 8, 0, 0);
        var tile2 = new Tile(ResourceType.Ore, 6, 2, 0);
        var vertex = new Vertex(tile1, tile2);

        // Assert
        Assert.True(vertex.ConnectsTiles(tile1, tile2));
        Assert.True(vertex.ConnectsTiles(tile2, tile1));
        Assert.False(vertex.ConnectsTiles(tile1, tile2, new Tile(ResourceType.Wood, 4, 1, -1)));
    }

    [Fact]
    public void ConnectsTiles_AllThree()
    {
        // Arrange
        var tile1 = new Tile(ResourceType.Brick, 8, 0, 0);
        var tile2 = new Tile(ResourceType.Ore, 6, 2, 0);
        var tile3 = new Tile(ResourceType.Wood, 4, 1, -1);
        var vertex = new Vertex(tile1, tile2, tile3);

        // Assert
        Assert.True(vertex.ConnectsTiles(tile1, tile2, tile3));
        Assert.True(vertex.ConnectsTiles(tile1, tile3, tile2));
        Assert.True(vertex.ConnectsTiles(tile2, tile1, tile3));
        Assert.True(vertex.ConnectsTiles(tile2, tile3, tile1));
        Assert.True(vertex.ConnectsTiles(tile3, tile1, tile2));
        Assert.True(vertex.ConnectsTiles(tile3, tile2, tile1));
        Assert.False(vertex.ConnectsTiles(tile1, tile2));
        Assert.False(vertex.ConnectsTiles(tile1, tile3));
    }

[Fact]
    public void AddEdgeReference_Valid()
    {
        var gs = TestHelpers.CreateEdgesAndVertexForRefTests();

        var result = gs.Vertices[0].AddEdgeReference(gs.Edges[0]);
        Assert.True(result);

        result = gs.Vertices[0].AddEdgeReference(gs.Edges[1]);
        Assert.True(result);

        result = gs.Vertices[0].AddEdgeReference(gs.Edges[2]);
        Assert.True(result);

        Assert.Contains(gs.Edges[0], gs.Vertices[0].Edges);
        Assert.Contains(gs.Edges[1], gs.Vertices[0].Edges);
        Assert.Contains(gs.Edges[2], gs.Vertices[0].Edges);
    }

    [Fact]
    public void AddEdgeReference_TooMany()
    {
        var gs = TestHelpers.CreateEdgesAndVertexForRefTests();

        var result = gs.Vertices[0].AddEdgeReference(gs.Edges[0]);
        Assert.True(result);

        result = gs.Vertices[0].AddEdgeReference(gs.Edges[1]);
        Assert.True(result);

        result = gs.Vertices[0].AddEdgeReference(gs.Edges[2]);
        Assert.True(result);

        result = gs.Vertices[0].AddEdgeReference(gs.Edges[3]);
        Assert.False(result);


        Assert.Contains(gs.Edges[0], gs.Vertices[0].Edges);
        Assert.Contains(gs.Edges[1], gs.Vertices[0].Edges);
        Assert.Contains(gs.Edges[2], gs.Vertices[0].Edges);
        Assert.DoesNotContain(gs.Edges[3], gs.Vertices[0].Edges);
    }

    [Fact]
    public void AddEdgeReference_Duplicate()
    {
        var gs = TestHelpers.CreateEdgesAndVertexForRefTests();

        var result = gs.Vertices[0].AddEdgeReference(gs.Edges[0]);
        Assert.True(result);

        result = gs.Vertices[0].AddEdgeReference(gs.Edges[0]);
        Assert.False(result);

        Assert.Contains(gs.Edges[0], gs.Vertices[0].Edges);
    }

    [Fact]
    public void MarkBlocked_Valid()
    {
        var tile1 = new Tile(ResourceType.Wood, 10, 0, 0);
        var tile2 = new Tile(ResourceType.Ore, 3, -2, 0);

        var vertex = new Vertex(tile1, tile2);

        vertex.MarkBlocked();

        Assert.Equal(BuildingType.Blocked, vertex.Building);
    }

    [Fact]
    public void MarkBlocked_SettlementExists()
    {
        var tile1 = new Tile(ResourceType.Wood, 10, 0, 0);
        var tile2 = new Tile(ResourceType.Ore, 3, -2, 0);

        var vertex = new Vertex(tile1, tile2);
        vertex.BuildSettlement(new Player("Mary", PlayerColor.Blue));

        var exception = Assert.Throws<InvalidOperationException>(() => vertex.MarkBlocked());

        Assert.Equal("Can't block a vertex that already has a building on it.", exception.Message);
        Assert.Equal(BuildingType.Settlement, vertex.Building);
    }
    
    [Fact]
    public void MarkBlocked_CityExists()
    {
        var tile1 = new Tile(ResourceType.Wood, 10, 0, 0);
        var tile2 = new Tile(ResourceType.Ore, 3, -2, 0);

        var vertex = new Vertex(tile1, tile2);
        vertex.BuildSettlement(new Player("Mary", PlayerColor.Blue));
        vertex.UpgradeToCity();

        var exception = Assert.Throws<InvalidOperationException>(() => vertex.MarkBlocked());

        Assert.Equal("Can't block a vertex that already has a building on it.", exception.Message);
        Assert.Equal(BuildingType.City, vertex.Building);
    }

}