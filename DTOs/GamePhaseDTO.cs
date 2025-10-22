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

    // JsonConstructor lets System.Text.Json bind constructor parameters to JSON properties.
    [JsonConstructor]
    public GamePhaseDTO(string currentPlayerId, string phaseState, string? endPlayerId = null)
    {
        CurrentPlayerId = currentPlayerId;
        PhaseState = phaseState;
        EndPlayerId = endPlayerId;
    }

    public GamePhaseDTO(GamePhase gamePhase)
    {
        PhaseState = gamePhase.PhaseState.ToString();

        if (gamePhase.CurrentPlayer != null) 
            CurrentPlayerId = gamePhase.CurrentPlayer.Id;
        if (gamePhase.EndPlayer != null)
            EndPlayerId = gamePhase.EndPlayer.Id;
    }

}

