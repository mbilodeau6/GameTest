namespace GameTest.Models;
using System.Text.Json.Serialization;
using GameTest.DTOs;
using GameTest.Services;

public class GamePhase
{
    public GameStates PhaseState { get; set; } = GameStates.SettingUpBoard;
    public Player? CurrentPlayer { get; set; }
    public Player? EndPlayer { get; set; }
    public GameStates? PreviousState {get; private set; } = null;
    public Tile OriginalRobberTile { get; private set; } = null;
    public int? RoadsPreRoadBuilding { get; private set; } = null;


    // TODO: Can all callers to this version be changed to use the DTO version?
    public GamePhase(GameStates state, Player? current = null, Player? end = null)
    {
        PhaseState = state;
        CurrentPlayer = current ?? null;
        EndPlayer = end ?? null;
        
    }

    public GamePhase(GameState gs, GamePhaseDTO dto)
    {
        PhaseState = Enum.Parse<GameStates>(dto.PhaseState);
        if (dto.CurrentPlayerId != null)
            CurrentPlayer = GamePlayHelpers.GetPlayerFromPlayerId(gs, dto.CurrentPlayerId);

        if (dto.EndPlayerId != null)
            EndPlayer = GamePlayHelpers.GetPlayerFromPlayerId(gs, dto.EndPlayerId);

        if (!string.IsNullOrEmpty(dto.PreviousState))
            PreviousState = Enum.Parse<GameStates>(dto.PreviousState);

        if (dto.OriginalRobberTileId != null)
            OriginalRobberTile = gs.Tiles.First(t => t.Id == dto.OriginalRobberTileId);

        RoadsPreRoadBuilding = dto.RoadsPreRoadBuilding;
    }
    
    public void SetStateToReturnTo(GameStates state, Tile originalTile)
    {
        if (PreviousState != null)
            throw new InvalidOperationException("Unexpected Error. Call to SetPreRobberState when it is already set.");
            
        PreviousState = state;
        OriginalRobberTile = originalTile;
    }

    public void ClearRobberState()
    {
        PreviousState = null;
        OriginalRobberTile = null;
    }

    public void ClearRoadBuildingState()
    {
        PreviousState = null;
        RoadsPreRoadBuilding = null;
    }

    public void StoreStateDevCardRoadBuilding(GameStates currentState, int roadCount)
    {
        RoadsPreRoadBuilding = roadCount;
        PreviousState = currentState;
    }
}