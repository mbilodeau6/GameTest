using System.Text.Json.Serialization;
using GameTest.Models;

public class PreActionState
{
    public int EventRecordId {get; private set; }
    public GamePhase Phase {get; init; }
    public string? HasLongestRoadPlayerId { get; init; } = null;
    public string? HasLargestArmyPlayerId { get; init; } = null;

    public PreActionState(int eventRecordId, GamePhase phase, Player? longestRoadPlayer, Player? largestArmyPlayer )
    {
        EventRecordId = eventRecordId;

        Phase = new GamePhase(phase);

        if (largestArmyPlayer != null)
            HasLargestArmyPlayerId = largestArmyPlayer.Id;

        if (longestRoadPlayer != null)
            HasLongestRoadPlayerId = longestRoadPlayer.Id;
    }
    
    // JsonConstructor lets System.Text.Json bind constructor parameters to JSON properties.
    public PreActionState(int eventRecordId, GamePhase phase, string? longestRoadPlayerId, string? largestArmyPlayerId )
    {
        EventRecordId = eventRecordId;
        Phase = new GamePhase(phase);
        HasLargestArmyPlayerId = largestArmyPlayerId;
        HasLongestRoadPlayerId = longestRoadPlayerId;
    }

    public void SetEventRecordId(int eventRecordId)
    {
        EventRecordId = eventRecordId;
    }
}
