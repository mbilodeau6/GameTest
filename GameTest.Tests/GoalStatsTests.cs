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

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new GoalStats(vertex, tradeRate));
    }

    [Fact]
    public void Constructor_InvalidTradeRate_TooHigh()
    {
        // Arrange
        var t1 = new Tile(ResourceType.Wood, 8, 0, 0);
        var t2 = new Tile(ResourceType.Brick, 6, 2, 0);
        var vertex = new Vertex(t1, t2, null);
        int tradeRate = 5;

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new GoalStats(vertex, tradeRate));
    }

    [Fact]
    public void Constructor_NullVertex()
    {
        // Arrange
        Vertex vertex = null;
        int tradeRate = 3;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new GoalStats(vertex, tradeRate));
    }

    [Fact]
    public void Constructor_ValidParameters_PropertiesSet()
    {
        // Arrange
        var t1 = new Tile(ResourceType.Wood, 8, 0, 0);
        var t2 = new Tile(ResourceType.Brick, 6, 2, 0);
        var vertex = new Vertex(t1, t2, null);
        int tradeRate = 3;

        // Act
        var goalStats = new GoalStats(vertex, tradeRate);

        // Assert
        Assert.Equal(vertex.Id, goalStats.TargetVertex.Id);
        Assert.Equal(tradeRate, goalStats.TradeRate);
        Assert.NotNull(goalStats.ResourceAcquisitionRates);
        foreach (ResourceType rt in Enum.GetValues(typeof(ResourceType)))
        {
            if (rt == ResourceType.Desert)
                continue;

            Assert.True(goalStats.ResourceAcquisitionRates.ContainsKey(rt));
            Assert.Equal(0.0, goalStats.ResourceAcquisitionRates[rt]);
        }

        Assert.Equal(0, goalStats.RoadsNeeded);
        Assert.Equal(0.0, goalStats.InterceptionRisk);
        Assert.Equal(0.0, goalStats.OverallScore);
    }
}