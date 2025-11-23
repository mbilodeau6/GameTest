using Azure.Storage.Blobs.Models;
using GameTest.DTOs;
using GameTest.Services;

namespace GameTest.Models;

public class GoalStats
{
    public Vertex TargetVertex { get; }
    public Edge? NextEdgeToTarget { get; }
    public Dictionary<ResourceType, double> ResourceAcquisitionRates { get; }
    // TODO: Consider changing to or adding EstRoundsToAchieve and EstRoundsToIntercept.
    // Something that identifies how likely achieving this goal is.
    public int RoadsNeeded { get; private set; }
    public double InterceptionRisk { get; set; }
    public double OverallScore { get; private set; }
   
    public GoalStats(Vertex targetVertex, Player player, Dictionary<ResourceType, double> baseStats, int roadsNeeded, Edge? nextEdgeToTarget)
    {
        if (targetVertex == null)
            throw new ArgumentNullException("targetVertex");

        if (roadsNeeded > 0 && nextEdgeToTarget == null)
            throw new ArgumentNullException("nextEdgeToTarget");

        TargetVertex = targetVertex;
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
        NextEdgeToTarget = nextEdgeToTarget;

        OverallScore = 0.0;

        foreach (ResourceType rt in Enum.GetValues(typeof(ResourceType)))
        {
            if (rt == ResourceType.Desert)
                continue;

            OverallScore += AIHelpers.GetAIResourceAcquisitionScore(ResourceAcquisitionRates[rt], rt, Bank.GetTradeRate(player, rt) == 2);
        }

        if (player.Ports.Contains(PortType.ThreeToOne))
            OverallScore += AIHelpers.GetAIWeight(AIHelpers.AIWeights.VertexValueAddForThreeToOnePort);

        // TODO: Also need to incorporate information on how close an opponent is to the vertex
        // to increase impact of RoadsNeededif the opponent is closers to taking the spot than
        // the bot.
        for(int i = 0; i < RoadsNeeded; i++)
            OverallScore *= AIHelpers.GetAIWeight(AIHelpers.AIWeights.VertexValuePenaltyForDistanceToClosestBuild);
    }
}