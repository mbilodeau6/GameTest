using Xunit;
using GameTest.Models;
using GameTest.DTOs;
using GameTest.Services;
using GameTest.Functions;
using Microsoft.VisualStudio.TestPlatform.Common.ExtensionFramework;

namespace GameTest.Tests;

public class GoalWeightTests
{
    [Fact]
    public void Constructor_Valid()
    {
        double settlementWeight = 0.1;
        double cityWeight = 0.2;
        double roadWeight = 0.3;
        double developmentCardWeight = 0.4;

        GoalWeights goalWeights = new GoalWeights(settlementWeight, cityWeight, roadWeight, developmentCardWeight);

        Assert.Equal(settlementWeight, goalWeights.SettlementWeight);
        Assert.Equal(cityWeight, goalWeights.CityWeight);
        Assert.Equal(roadWeight, goalWeights.RoadWeight);
        Assert.Equal(developmentCardWeight, goalWeights.DevelopmentCardWeight);
    }

    [Fact]
    public void NormalizeWeights_Valid()
    {
        double settlementWeight = 1.0;
        double cityWeight = 2.0;
        double roadWeight = 3.0;
        double developmentCardWeight = 4.0;

        GoalWeights goalWeights = new GoalWeights(settlementWeight, cityWeight, roadWeight, developmentCardWeight);
        goalWeights.NormalizeWeights();

        double totalWeight = settlementWeight + cityWeight + roadWeight + developmentCardWeight;
        Assert.Equal(settlementWeight / totalWeight, goalWeights.SettlementWeight);
        Assert.Equal(cityWeight / totalWeight, goalWeights.CityWeight);
        Assert.Equal(roadWeight / totalWeight, goalWeights.RoadWeight);
        Assert.Equal(developmentCardWeight / totalWeight, goalWeights.DevelopmentCardWeight);
        Assert.Equal(1.0, goalWeights.SettlementWeight + goalWeights.CityWeight + goalWeights.RoadWeight + goalWeights.DevelopmentCardWeight);
    }
}
