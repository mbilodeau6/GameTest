using GameTest.Models;
using Microsoft.Identity.Client.Extensibility;
using System.Text.Json.Serialization;

namespace GameTest.DTOs;

public class GameSettingsDTO
{
    public string Type { get; }
    public int MaxPlayers { get; }
    public int VictoryPointsToWin { get; }
    public int RoadsPerPlayer { get; }
    public int SettlementsPerPlayer { get; }
    public int CitiesPerPlayer { get; }
    public string? PreRobberState { get; } = null;
    public string? OriginalRobberTileId { get; } = null;


    [JsonConstructor]
    public GameSettingsDTO(string type,
        int maxPlayers = 0, int victoryPointsToWin = 0, int roadsPerPlayer = 0,
        int settlementsPerPlayer = 0, int citiesPerPlayer = 0, string? preRobberState = null,
        string? originalRobberTileId = null) 
    {
        Type = type ?? string.Empty;
        MaxPlayers = maxPlayers;
        VictoryPointsToWin = victoryPointsToWin;
        RoadsPerPlayer = roadsPerPlayer;
        SettlementsPerPlayer = settlementsPerPlayer;
        CitiesPerPlayer = citiesPerPlayer;
        PreRobberState = preRobberState;
        OriginalRobberTileId = originalRobberTileId;
    }

    public GameSettingsDTO(GameSettings settings)
    {
        Type = settings.Type.ToString();
        MaxPlayers = settings.MaxPlayers;
        VictoryPointsToWin = settings.VictoryPointsToWin;
        RoadsPerPlayer = settings.RoadsPerPlayer;
        SettlementsPerPlayer = settings.SettlementsPerPlayer;
        CitiesPerPlayer = settings.CitiesPerPlayer;
        if (settings.PreRobberState != null)
            PreRobberState = settings.PreRobberState.ToString();
        if (settings.OriginalRobberTile != null)
            OriginalRobberTileId = settings.OriginalRobberTile.Id.ToString();
    }

    public GameSettingsDTO(GameSettingsDTO dto)
    {
        Type = dto.Type;
        MaxPlayers = dto.MaxPlayers;
        VictoryPointsToWin = dto.VictoryPointsToWin;
        RoadsPerPlayer = dto.RoadsPerPlayer;
        SettlementsPerPlayer = dto.SettlementsPerPlayer;
        CitiesPerPlayer = dto.CitiesPerPlayer;
        PreRobberState = dto.PreRobberState;
        OriginalRobberTileId = dto.OriginalRobberTileId;
    }
}