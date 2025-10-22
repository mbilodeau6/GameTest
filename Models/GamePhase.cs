namespace GameTest.Models;
using System.Text.Json.Serialization;
using GameTest.DTOs;

public class GamePhase
{
    public GameStates PhaseState { get; set; } = GameStates.SettingUpBoard;
    public Player? CurrentPlayer { get; set; }
    public Player? EndPlayer { get; set; }

    public GamePhase(GameStates state, Player? current = null, Player? end = null)
    {
        PhaseState = state;
        CurrentPlayer = current ?? null;
        EndPlayer = end ?? null;
    }
}