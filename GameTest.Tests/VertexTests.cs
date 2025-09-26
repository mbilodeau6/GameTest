using Xunit;
using GameTest.Models;

namespace GameTest.Tests;

public class VertexTests
{
    [Fact]
    public void Constructor_VertexOfSingleTile_CreatesExpectedVertex()
    {
        // Arrange
        var player = new Player("Alice", PlayerColor.Blue);
        var tile = new Tile(ResourceType.Brick, 8, 0, 0);

        // Act
        var vertex = new Vertex(player, tile);

        // Assert
        Assert.True(TestHelpers.ValidateId(vertex.Id, 'V'));
        Assert.Single(vertex.Tiles);
        Assert.All(vertex.Tiles, t => Assert.NotNull(t));
    }

    // TODO: Add tests for constructor with two and three tiles

    [Fact]
    public void UpgradeToCity_ExistingSettlement_UpgradesBuildingToCity()
    {
        // Arrange
        var player = new Player("Alice", PlayerColor.Blue);
        var tile = new Tile(ResourceType.Brick, 8, 0, 0);
        var vertex = new Vertex(player, tile);

        // Act
        vertex.UpgradeToCity();

        // Assert
        Assert.Equal(BuildingType.City, vertex.Building);
    }

    [Fact]
    public void UpgradeToCity_ExistingCity_ThrowsInvalidOperationException()
    {
        // Arrange
        // Arrange
        var player = new Player("Alice", PlayerColor.Blue);
        var tile = new Tile(ResourceType.Brick, 8, 0, 0);
        var vertex = new Vertex(player, tile);
        vertex.UpgradeToCity();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => vertex.UpgradeToCity());
        Assert.Equal("Only a settlement can be upgraded to a city.", exception.Message);
    }

    // TODO: Add tests for DowngradeToSettlement cases
    // [Fact]
    // public void RemoveBuilding_ExistingSettlement_SetsBuildingToNone()
    // {
    //     // Arrange
    //     var vertex = new Vertex();
    //     vertex.BuildSettlement();

    //     // Act
    //     vertex.RemoveBuilding();

    //     // Assert
    //     Assert.Equal(BuildingType.None, vertex.Building);
    // }

    // [Fact]
    // public void RemoveBuilding_ExistingCity_SetsBuildingToNone()
    // {
    //     // Arrange
    //     var vertex = new Vertex();
    //     vertex.BuildSettlement();
    //     vertex.UpgradeToCity();

    //     // Act
    //     vertex.RemoveBuilding();

    //     // Assert
    //     Assert.Equal(BuildingType.None, vertex.Building);
    // }

    [Fact]
    public void ToString_NoParameters_ReturnsNonEmptyString()
    {
        // Arrange
        var player = new Player("Bob", PlayerColor.Green);
        var tile = new Tile(ResourceType.Brick, 8, 0, 0);
        var vertex = new Vertex(player, tile);

        // Act
        var result = vertex.ToString();

        // Assert
        Assert.NotNull(result);

        string expectedResult = $"Vertex {vertex.Id} (Owner: {vertex.Owner.Name}; Building: ";

        if (vertex.Building == BuildingType.Settlement)
            expectedResult += "S)";

        if (vertex.Building == BuildingType.City)
            expectedResult += "C)";

        Assert.Equal(expectedResult, result);
    }
}