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
    public int RoadsNeeded { get; private set; }
    public double InterceptionRisk { get; set; }
    public double OverallScore { get; private set; }
    // TODO: When ports are supported, TradeRate needs to identify the type of port as well as the rate.
    // Easiest may be to have a Dictionary of ResourceType where all values will be set to 3 for
    // a 3:1 port.
    public int TradeRate { get; }
   
    // TODO: Need to pass in all values (like RoadsNeeded) so that they can be included in OverallScore.
    // For RoadsNeeded, the OverallScore should decrease the further the user is from the target.
    // Especially if an opponent is closer to the target. The RoadsNeeded penalty should be decreased if
    // the user has resources to build roads and a settlement (or trade for those resources).
    public GoalStats(Vertex targetVertex, int tradeRate, Dictionary<ResourceType, double> baseStats, int roadsNeeded)
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
        
        RoadsNeeded = roadsNeeded;
        InterceptionRisk = 0.0;

        // TODO: Incorporate TradeRate, RoadsNeeded, and InterceptionRisk into OverallScore and
        // make weightings configurable.
        OverallScore = ResourceAcquisitionRates[ResourceType.Ore] * 1.2 
            + ResourceAcquisitionRates[ResourceType.Grain] * 1.1 
            + ResourceAcquisitionRates[ResourceType.Brick] 
            + ResourceAcquisitionRates[ResourceType.Wood] * 0.9 
            + ResourceAcquisitionRates[ResourceType.Wool] * 0.8;
    }
}