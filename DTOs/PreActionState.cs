using System.Text.Json.Serialization;
using GameTest.Models;

public class PreActionState
{
    public int EventRecordId {get; private set; }
    public GameStates State {get; init; }
    public string CurrentPlayerId {get; init; }
    public string EndPlayerId {get; init; }
    public string? HasLongestRoadPlayerId { get; init; } = null;
    public string? HasLargestArmyPlayerId { get; init; } = null;

    public PreActionState(int eventRecordId, GameStates state, Player currentPlayer, 
        Player endPlayer, Player? longestRoadPlayer, Player? largestArmyPlayer )
    {
        EventRecordId = eventRecordId;
        State = state;
        CurrentPlayerId = currentPlayer.Id;
        EndPlayerId = endPlayer.Id;

        if (largestArmyPlayer != null)
            HasLargestArmyPlayerId = largestArmyPlayer.Id;

        if (longestRoadPlayer != null)
            HasLongestRoadPlayerId = longestRoadPlayer.Id;
    }
    
    // JsonConstructor lets System.Text.Json bind constructor parameters to JSON properties.
    public PreActionState(int eventRecordId, GameStates state, string currentPlayerId, 
        string endPlayerId, string? longestRoadPlayerId, string? largestArmyPlayerId )
    {
        EventRecordId = eventRecordId;
        State = state;
        CurrentPlayerId = currentPlayerId;
        EndPlayerId = endPlayerId;
        HasLargestArmyPlayerId = largestArmyPlayerId;
        HasLongestRoadPlayerId = longestRoadPlayerId;
    }

    public void SetEventRecordId(int eventRecordId)
    {
        EventRecordId = eventRecordId;
    }
}
