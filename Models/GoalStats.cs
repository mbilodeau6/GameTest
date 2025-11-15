using Azure.Storage.Blobs.Models;
using GameTest.DTOs;
using GameTest.Services;

namespace GameTest.Models;

public class GoalStats
{
    public Vertex TargetVertex { get; }
    public Dictionary<ResourceType, double> ResourceAcquisitionRates { get; }
    // TODO: Consider changing to or adding EstRoundsToAchieve and EstRoundsToIntercept.
    // Something that identifies how likely achieving this goal is.
    public int RoadsNeeded { get; set; }
    public double InterceptionRisk { get; set; }
    public double OverallScore { get; set; }
    public int TradeRate { get; }
   

    public GoalStats(Vertex targetVertex, int tradeRate, Dictionary<ResourceType, double>? baseStats = null)
    {
        if (targetVertex == null)
            throw new ArgumentNullException("targetVertex");

        if (tradeRate < 2 || tradeRate > 4)
            throw new ArgumentOutOfRangeException("tradeRate", "Trade rate must be between 2 and 4.");

        TargetVertex = targetVertex;
        TradeRate = tradeRate;
        ResourceAcquisitionRates = new Dictionary<ResourceType, double>();
        foreach (ResourceType rt in Enum.GetValues(typeof(ResourceType)))
        {
            if (rt == ResourceType.Desert)
                continue;

            ResourceAcquisitionRates[rt] = 0.0;
        }

        foreach (var tile in targetVertex.Tiles)
        {
            if (tile.Resource == ResourceType.Desert)
                continue;

            ResourceAcquisitionRates[tile.Resource] += AIHelpers.GetProbabilityForDiceRoll(tile.DiceNumber);
        }
        
        if (baseStats != null)
            foreach(var rt in baseStats.Keys.ToList())
                if (rt != ResourceType.Desert)
                    ResourceAcquisitionRates[rt] += baseStats[rt];
        
        RoadsNeeded = 0;
        InterceptionRisk = 0.0;
        OverallScore = 0.0;
    }
}