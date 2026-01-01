using Xunit;
using GameTest.Models;
using GameTest.Services;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace GameTest.Tests;

public class CandidatePathTests
{
    private Tile t19 = new Tile(ResourceType.Desert, 0, 0, 0);
    private Tile t18 = new Tile(ResourceType.Wool, 4, 1, -1);
    private Tile t13 = new Tile(ResourceType.Brick, 6, -1, -1);

    [Fact]
    public void Constructor_EdgeOwned_NotFirstEdge()
    {
        Edge edge = new Edge(t19, t18);
        edge.BuildRoad(Player.CreateTestPlayer("Tim", PlayerColor.Red));

        var candidate = new CandidatePath(edge);

        Assert.Equal(edge.Id, candidate.NextEdge.Id);
        Assert.Null(candidate.FirstEdge);
        Assert.Equal(0, candidate.NewBuildRequired);
    }

    [Fact]
    public void Constructor_EdgeNotOwned_FirstEdge()
    {
        Edge edge = new Edge(t19, t18);

        var candidate = new CandidatePath(edge);

        Assert.Equal(edge.Id, candidate.NextEdge.Id);
        Assert.NotNull(candidate.FirstEdge);
        Assert.Equal(edge.Id, candidate.FirstEdge.Id);
        Assert.Equal(1, candidate.NewBuildRequired);
    }

    [Fact]
    public void SetNextEdge_EdgeNotOwned_FirstEdge()
    {
        // Arrange
        var e1 = new Edge(t19, t18);
        e1.BuildRoad(Player.CreateTestPlayer("Time", PlayerColor.Red));
        var candidate = new CandidatePath(e1);

        var e2 = new Edge(t18, t13);

        // Act
        candidate.SetNextEdge(e2);

        // Assert
        Assert.Equal(e2.Id, candidate.NextEdge.Id);
        Assert.NotNull(candidate.FirstEdge);
        Assert.Equal(e2.Id, candidate.FirstEdge.Id);
        Assert.Equal(1, candidate.NewBuildRequired);
    }

    [Fact]
    public void SetNextEdge_EdgeNotOwned_NotFirstEdge()
    {
        // Arrange
        var e1 = new Edge(t19, t18);
        var candidate = new CandidatePath(e1);

        var e2 = new Edge(t18, t13);

        // Act
        candidate.SetNextEdge(e2);

        // Assert
        Assert.Equal(e2.Id, candidate.NextEdge.Id);
        Assert.NotNull(candidate.FirstEdge);
        Assert.Equal(e1.Id, candidate.FirstEdge.Id);
        Assert.Equal(2, candidate.NewBuildRequired);
    }

    [Fact]
    public void SetNextEdge_EdgeOwned_AfterFirstEdgeSet()
    {
        // Arrange
        var e1 = new Edge(t19, t18);
        var candidate = new CandidatePath(e1);

        var e2 = new Edge(t18, t13);
        e2.BuildRoad(Player.CreateTestPlayer("Tim", PlayerColor.Red));

        // Act
        candidate.SetNextEdge(e2);

        // Assert
        Assert.Equal(e2.Id, candidate.NextEdge.Id);
        Assert.NotNull(candidate.FirstEdge);
        Assert.Equal(e1.Id, candidate.FirstEdge.Id);
        Assert.Equal(1, candidate.NewBuildRequired);
    }

    [Fact]
    public void SetNextEdge_EdgeOwned_FirstEdgeNotSet()
    {
        // Arrange
        var player1 = Player.CreateTestPlayer("Tim", PlayerColor.Red);
        var e1 = new Edge(t19, t18);
        e1.BuildRoad(player1);
        var candidate = new CandidatePath(e1);

        var e2 = new Edge(t18, t13);
        e2.BuildRoad(player1);

        // Act
        candidate.SetNextEdge(e2);

        // Assert
        Assert.Equal(e2.Id, candidate.NextEdge.Id);
        Assert.Null(candidate.FirstEdge);
        Assert.Equal(0, candidate.NewBuildRequired);
    }

    [Fact]
    public void SetNextEdge_EdgeOwnerDifferent_Exception()
    {
        // Arrange
        var e1 = new Edge(t19, t18);
        e1.BuildRoad(Player.CreateTestPlayer("Tim", PlayerColor.Red));
        var candidate = new CandidatePath(e1);

        var e2 = new Edge(t18, t13);
        e2.BuildRoad(Player.CreateTestPlayer("Jill", PlayerColor.Blue));

        // Act
        Assert.Throws<InvalidOperationException>(() => candidate.SetNextEdge(e2));
    }

    [Fact]
    public void CreateBranchOfPath_EdgeOwned_NoFirstEdge()
    {
        // Arrange
        var player1 = Player.CreateTestPlayer("Tim", PlayerColor.Red);
        var e1 = new Edge(t19, t18);
        e1.BuildRoad(player1);
        var candidate = new CandidatePath(e1);

        var e2 = new Edge(t18, t13);
        e2.BuildRoad(player1);

        // Act
        var candidate2 = candidate.CreateBranchOfPath(e2);

        // Assert
        Assert.Equal(e1.Id, candidate.NextEdge.Id);
        Assert.Null(candidate.FirstEdge);
        Assert.Equal(0, candidate.NewBuildRequired);

        Assert.Equal(e2.Id, candidate2.NextEdge.Id);
        Assert.Null(candidate2.FirstEdge);
        Assert.Equal(0, candidate2.NewBuildRequired);
    }

    [Fact]
    public void CreateBranchOfPath_EdgeOwned_FirstEdge()
    {
        // Arrange
        var e1 = new Edge(t19, t18);
        var candidate = new CandidatePath(e1);

        var e2 = new Edge(t18, t13);
        e2.BuildRoad(Player.CreateTestPlayer("Tim", PlayerColor.Red));

        // Act
        var candidate2 = candidate.CreateBranchOfPath(e2);

        // Assert
        Assert.Equal(e1.Id, candidate.NextEdge.Id);
        Assert.NotNull(candidate.FirstEdge);
        Assert.Equal(e1.Id, candidate.FirstEdge.Id);
        Assert.Equal(1, candidate.NewBuildRequired);

        Assert.Equal(e2.Id, candidate2.NextEdge.Id);
        Assert.NotNull(candidate2.FirstEdge);
        Assert.Equal(e1.Id, candidate2.FirstEdge.Id);
        Assert.Equal(1, candidate2.NewBuildRequired);
    }

    [Fact]
    public void CreateBranchOfPath_EdgeNotOwned_FirstEdge()
    {
        // Arrange
        var e1 = new Edge(t19, t18);
        e1.BuildRoad(Player.CreateTestPlayer("Tim", PlayerColor.Red));
        var candidate = new CandidatePath(e1);

        var e2 = new Edge(t18, t13);

        // Act
        var candidate2 = candidate.CreateBranchOfPath(e2);

        // Assert
        Assert.Equal(e1.Id, candidate.NextEdge.Id);
        Assert.Null(candidate.FirstEdge);
        Assert.Equal(0, candidate.NewBuildRequired);

        Assert.Equal(e2.Id, candidate2.NextEdge.Id);
        Assert.NotNull(candidate2.FirstEdge);
        Assert.Equal(e2.Id, candidate2.FirstEdge.Id);
        Assert.Equal(1, candidate2.NewBuildRequired);
    }

    [Fact]
    public void CreateBranchOfPath_EdgeNotOwned_NotFirstEdge()
    {
        // Arrange
        var e1 = new Edge(t19, t18);
        var candidate = new CandidatePath(e1);

        var e2 = new Edge(t18, t13);

        // Act
        var candidate2 = candidate.CreateBranchOfPath(e2);

        // Assert
        Assert.Equal(e1.Id, candidate.NextEdge.Id);
        Assert.NotNull(candidate.FirstEdge);
        Assert.Equal(e1.Id, candidate.FirstEdge.Id);
        Assert.Equal(1, candidate.NewBuildRequired);

        Assert.Equal(e2.Id, candidate2.NextEdge.Id);
        Assert.NotNull(candidate2.FirstEdge);
        Assert.Equal(e1.Id, candidate2.FirstEdge.Id);
        Assert.Equal(2, candidate2.NewBuildRequired);
    }

    [Fact]
    public void CreateBranchOfPath_EdgeOwnerDifferent_Exception()
    {
        // Arrange
        var e1 = new Edge(t19, t18);
        e1.BuildRoad(Player.CreateTestPlayer("Tim", PlayerColor.Red));
        var candidate = new CandidatePath(e1);

        var e2 = new Edge(t18, t13);
        e2.BuildRoad(Player.CreateTestPlayer("Jill", PlayerColor.Blue));

        // Act
        Assert.Throws<InvalidOperationException>(() => candidate.CreateBranchOfPath(e2));
    }
}
