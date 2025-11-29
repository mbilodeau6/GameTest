using System.Text.Json.Serialization;
using GameTest.Models;
using GameTest.Tests;
using Microsoft.Extensions.Logging;

namespace GameTest.DTOs;

public class GamePhaseDTO
{
    public string? CurrentPlayerId { get; }
    public string PhaseState { get; } = GameStates.SettingUpBoard.ToString();
    public string? EndPlayerId { get; }
    public string? PreviousState { get; } = null;
    public string? OriginalRobberTileId { get; } = null;


    // JsonConstructor lets System.Text.Json bind constructor parameters to JSON properties.
    [JsonConstructor]
    public GamePhaseDTO(string phaseState, string? currentPlayerId = null, string? endPlayerId = null, 
        string? previousState = null, string? originalRobberTileId = null)
    {
        CurrentPlayerId = currentPlayerId;
        PhaseState = phaseState;
        EndPlayerId = endPlayerId;
        PreviousState = previousState;
        OriginalRobberTileId = originalRobberTileId;
    }

    public GamePhaseDTO(GamePhase gamePhase)
    {
        PhaseState = gamePhase.PhaseState.ToString();

        if (gamePhase.CurrentPlayer != null) 
            CurrentPlayerId = gamePhase.CurrentPlayer.Id;
        if (gamePhase.EndPlayer != null)
            EndPlayerId = gamePhase.EndPlayer.Id;
        if (gamePhase.PreviousState != null)
            PreviousState = gamePhase.PreviousState.ToString();
        if (gamePhase.OriginalRobberTile != null)
            OriginalRobberTileId = gamePhase.OriginalRobberTile.Id.ToString();
    }
}

