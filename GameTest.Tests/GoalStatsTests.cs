using Xunit;
using GameTest.Models;
using GameTest.DTOs;
using GameTest.Services;
using GameTest.Functions;
using Microsoft.VisualStudio.TestPlatform.Common.ExtensionFramework;

namespace GameTest.Tests;

public class GoalStatsTests
{
    [Fact]
    public void Constructor_InvalidTradeRate_TooLow()
    {
        // Arrange
        var t1 = new Tile(ResourceType.Wood, 8, 0, 0);
        var t2 = new Tile(ResourceType.Brick, 6, 2, 0);
        var vertex = new Vertex(t1, t2, null);
        int tradeRate = 1;
        Dictionary<ResourceType, double> baseStats = new Dictionary<ResourceType, double>();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new GoalStats(vertex, tradeRate, baseStats, 0, null));
    }

    [Fact]
    public void Constructor_InvalidTradeRate_TooHigh()
    {
        // Arrange
        var t1 = new Tile(ResourceType.Wood, 8, 0, 0);
        var t2 = new Tile(ResourceType.Brick, 6, 2, 0);
        var vertex = new Vertex(t1, t2, null);
        int tradeRate = 5;
        Dictionary<ResourceType, double> baseStats = new Dictionary<ResourceType, double>();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new GoalStats(vertex, tradeRate, baseStats, 0, null));
    }

    [Fact]
    public void Constructor_NullVertex()
    {
        // Arrange
        Vertex vertex = null;
        int tradeRate = 3;
        Dictionary<ResourceType, double> baseStats = new Dictionary<ResourceType, double>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new GoalStats(vertex, tradeRate, baseStats, 0, null));
    }

    [Fact]
    public void Constructor_NoBaseStats_PropertiesSet()
    {
        // Arrange
        var t1 = new Tile(ResourceType.Wood, 8, 0, 0);
        var t2 = new Tile(ResourceType.Brick, 6, 2, 0);
        var vertex = new Vertex(t1, t2, null);
        int tradeRate = 3;
        Dictionary<ResourceType, double> baseStats = new Dictionary<ResourceType, double>();

        // Act
        var goalStats = new GoalStats(vertex, tradeRate, baseStats, 0, null);

        // Assert
        Assert.Equal(vertex.Id, goalStats.TargetVertex.Id);
        Assert.Equal(tradeRate, goalStats.TradeRate);
        Assert.NotNull(goalStats.ResourceAcquisitionRates);
        foreach (ResourceType rt in Enum.GetValues(typeof(ResourceType)))
        {
            if (rt == ResourceType.Desert)
                continue;

            Assert.True(goalStats.ResourceAcquisitionRates.ContainsKey(rt));

            if (rt == ResourceType.Wood || rt == ResourceType.Brick)
                Assert.Equal(5.0 / 36.0, goalStats.ResourceAcquisitionRates[rt]);
            else
                Assert.Equal(0.0, goalStats.ResourceAcquisitionRates[rt]);
        }

        Assert.Equal(0, goalStats.RoadsNeeded);
        Assert.Equal(0.0, goalStats.InterceptionRisk);
        Assert.Equal((5.0/36.0) + (5.0/36) *0.9, goalStats.OverallScore);
    }

    [Fact]
    public void Constructor_RoadRequired_ReducesOveralScore()
    {
        // Arrange
        var t1 = new Tile(ResourceType.Wood, 8, 0, 0);
        var t2 = new Tile(ResourceType.Brick, 6, 2, 0);
        var vertex = new Vertex(t1, t2, null);
        int tradeRate = 3;
        var neededEdge = new Edge(t1, t2);
        Dictionary<ResourceType, double> baseStats = new Dictionary<ResourceType, double>();

        // Act
        var goalStats = new GoalStats(vertex, tradeRate, baseStats, 1, neededEdge);

        // Assert
        Assert.Equal(vertex.Id, goalStats.TargetVertex.Id);
        Assert.Equal(tradeRate, goalStats.TradeRate);
        Assert.NotNull(goalStats.ResourceAcquisitionRates);
        foreach (ResourceType rt in Enum.GetValues(typeof(ResourceType)))
        {
            if (rt == ResourceType.Desert)
                continue;

            Assert.True(goalStats.ResourceAcquisitionRates.ContainsKey(rt));

            if (rt == ResourceType.Wood || rt == ResourceType.Brick)
                Assert.Equal(5.0 / 36.0, goalStats.ResourceAcquisitionRates[rt]);
            else
                Assert.Equal(0.0, goalStats.ResourceAcquisitionRates[rt]);
        }

        Assert.Equal(1, goalStats.RoadsNeeded);
        Assert.NotNull(goalStats.NextEdgeToTarget);
        Assert.Equal(neededEdge.Id, goalStats.NextEdgeToTarget.Id);
        Assert.Equal(0.0, goalStats.InterceptionRisk);
        Assert.True((goalStats.OverallScore < (5.0/36.0) + (5.0/36) *0.9) && goalStats.OverallScore > 0.0);
    }

    [Fact]
    public void Constructor_WithBaseStats_PropertiesSet()
    {
        // Arrange
        var t1 = new Tile(ResourceType.Wood, 8, 0, 0);
        var t2 = new Tile(ResourceType.Brick, 6, 2, 0);
        var vertex = new Vertex(t1, t2, null);
        int tradeRate = 3;

        Dictionary<ResourceType, double> baseStats = new Dictionary<ResourceType, double>();
        baseStats[ResourceType.Wood] = 4.0 / 36.0;
        baseStats[ResourceType.Ore] = 1.0 / 36.0;

        // Act
        var goalStats = new GoalStats(vertex, tradeRate, baseStats, 0, null);

        // Assert
        Assert.Equal(vertex.Id, goalStats.TargetVertex.Id);
        Assert.Equal(tradeRate, goalStats.TradeRate);
        Assert.NotNull(goalStats.ResourceAcquisitionRates);
        foreach (ResourceType rt in Enum.GetValues(typeof(ResourceType)))
        {
            if (rt == ResourceType.Desert)
                continue;

            Assert.True(goalStats.ResourceAcquisitionRates.ContainsKey(rt));

            if (rt == ResourceType.Wood)
                Assert.Equal((5.0 + 4.0) / 36.0, goalStats.ResourceAcquisitionRates[rt]);
            else if (rt == ResourceType.Brick)
                Assert.Equal(5.0 / 36.0, goalStats.ResourceAcquisitionRates[rt]);
            else if (rt == ResourceType.Ore)
                Assert.Equal(1.0 / 36.0, goalStats.ResourceAcquisitionRates[rt]);
            else
                Assert.Equal(0.0, goalStats.ResourceAcquisitionRates[rt]);
        }

        Assert.Equal(0, goalStats.RoadsNeeded);
        Assert.Equal(0.0, goalStats.InterceptionRisk);
        Assert.Equal((9.0/36.0) * .9 + (5.0/36.0) + (1.0/36.0) * 1.2, goalStats.OverallScore);
    }

    [Fact]
    public void Constructor_RoadsNeededButNextEdgeNull_Exception()
    {
        // Arrange
        var t1 = new Tile(ResourceType.Wood, 8, 0, 0);
        var t2 = new Tile(ResourceType.Brick, 6, 2, 0);
        var vertex = new Vertex(t1, t2, null);
        int tradeRate = 3;
        Dictionary<ResourceType, double> baseStats = new Dictionary<ResourceType, double>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new GoalStats(vertex, tradeRate, baseStats, 1, null));
    }
}