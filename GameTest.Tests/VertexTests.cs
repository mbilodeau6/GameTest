using Xunit;
using GameTest.Models;

namespace GameTest.Tests;

public class VertexTests
{
    [Fact]
    public void Constructor_ValidParameters_CreatesExpectedVertex()
    {
        // Arrange

        // Act
        var vertex = new Vertex();

        // Assert
        Assert.True(vertex.Id >= 1);
        Assert.Equal(BuildingType.None, vertex.Building);
        Assert.Null(vertex.Owner);
        Assert.Equal(2, vertex.Edges.Length);
        Assert.All(vertex.Edges, e => Assert.Null(e));
        Assert.Equal(3, vertex.Tiles.Length);
        Assert.All(vertex.Tiles, t => Assert.Null(t));
    }

    [Fact]
    public void BuildSettlement_NoExistingBuilding_SetsBuildingToSettlement()
    {
        // Arrange
        var vertex = new Vertex();

        // Act
        vertex.BuildSettlement();

        // Assert
        Assert.Equal(BuildingType.Settlement, vertex.Building);
    }

    [Fact]
    public void BuildSettlement_ExistingSettlement_ThrowsInvalidOperationException()
    {
        // Arrange
        var vertex = new Vertex();
        vertex.BuildSettlement();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => vertex.BuildSettlement());
        Assert.Equal("A building already exists on this vertex.", exception.Message);
    }

    [Fact]
    public void BuildSettlement_ExistingCity_ThrowsInvalidOperationException()
    {
        // Arrange
        var vertex = new Vertex();
        vertex.BuildSettlement();
        vertex.UpgradeToCity();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => vertex.BuildSettlement());
        Assert.Equal("A building already exists on this vertex.", exception.Message);
    }

    [Fact]
    public void UpgradeToCity_ExistingSettlement_UpgradesBuildingToCity()
    {
        // Arrange
        var vertex = new Vertex();
        vertex.BuildSettlement();

        // Act
        vertex.UpgradeToCity();

        // Assert
        Assert.Equal(BuildingType.City, vertex.Building);
    }

    [Fact]
    public void UpgradeToCity_NoExistingSettlement_ThrowsInvalidOperationException()
    {
        // Arrange
        var vertex = new Vertex();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => vertex.UpgradeToCity());
        Assert.Equal("Only a settlement can be upgraded to a city.", exception.Message);
    }

    [Fact]
    public void UpgradeToCity_ExistingCity_ThrowsInvalidOperationException()
    {
        // Arrange
        var vertex = new Vertex();
        vertex.BuildSettlement();
        vertex.UpgradeToCity();

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => vertex.UpgradeToCity());
        Assert.Equal("Only a settlement can be upgraded to a city.", exception.Message);
    }

    [Fact]
    public void RemoveBuilding_ExistingSettlement_SetsBuildingToNone()
    {
        // Arrange
        var vertex = new Vertex();
        vertex.BuildSettlement();

        // Act
        vertex.RemoveBuilding();

        // Assert
        Assert.Equal(BuildingType.None, vertex.Building);
    }

    [Fact]
    public void RemoveBuilding_ExistingCity_SetsBuildingToNone()
    {
        // Arrange
        var vertex = new Vertex();
        vertex.BuildSettlement();
        vertex.UpgradeToCity();

        // Act
        vertex.RemoveBuilding();

        // Assert
        Assert.Equal(BuildingType.None, vertex.Building);
    }

    [Fact]
    public void RemoveBuilding_NoBuilding_SetsBuildingToNone()
    {
        // Arrange
        var vertex = new Vertex();

        // Act
        vertex.RemoveBuilding();

        // Assert
        Assert.Equal(BuildingType.None, vertex.Building);
    }
    
    [Fact]
    public void ToString_NoParameters_ReturnsNonEmptyString()
    {
        // Arrange
        var vertex = new Vertex();

        // Act
        var result = vertex.ToString();

        // Assert
        Assert.NotNull(result);
        Assert.IsType<string>(result);
        Assert.NotEqual(string.Empty, result);
    }
}