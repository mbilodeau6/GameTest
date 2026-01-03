using System.Text.Json.Serialization;
using GameTest.Models;

namespace GameTest.DTOs;

public class PreActionState
{
    public int EventRecordId {get; private set; }
    public GamePhaseDTO Phase {get; init; }
    public string? HasLongestRoadPlayerId { get; init; } = null;
    public string? HasLargestArmyPlayerId { get; init; } = null;

    public PreActionState(int eventRecordId, GamePhase phase, Player? longestRoadPlayer, Player? largestArmyPlayer )
    {
        EventRecordId = eventRecordId;

        Phase = new GamePhaseDTO(phase);

        if (largestArmyPlayer != null)
            HasLargestArmyPlayerId = largestArmyPlayer.Id;

        if (longestRoadPlayer != null)
            HasLongestRoadPlayerId = longestRoadPlayer.Id;
    }
    
    // JsonConstructor lets System.Text.Json bind constructor parameters to JSON properties.
    [JsonConstructor]
    public PreActionState(int eventRecordId, GamePhaseDTO phase, string? hasLongestRoadPlayerId, string? hasLargestArmyPlayerId )
    {
        EventRecordId = eventRecordId;
        Phase = phase;
        HasLargestArmyPlayerId = hasLargestArmyPlayerId;
        HasLongestRoadPlayerId = hasLongestRoadPlayerId;
    }

    public void SetEventRecordId(int eventRecordId)
    {
        EventRecordId = eventRecordId;
    }
}
