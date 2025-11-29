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

    [JsonConstructor]
    public GameSettingsDTO(string type,
        int maxPlayers = 0, int victoryPointsToWin = 0, int roadsPerPlayer = 0,
        int settlementsPerPlayer = 0, int citiesPerPlayer = 0) 
    {
        Type = type ?? string.Empty;
        MaxPlayers = maxPlayers;
        VictoryPointsToWin = victoryPointsToWin;
        RoadsPerPlayer = roadsPerPlayer;
        SettlementsPerPlayer = settlementsPerPlayer;
        CitiesPerPlayer = citiesPerPlayer;
    }

    public GameSettingsDTO(GameSettings settings)
    {
        Type = settings.Type.ToString();
        MaxPlayers = settings.MaxPlayers;
        VictoryPointsToWin = settings.VictoryPointsToWin;
        RoadsPerPlayer = settings.RoadsPerPlayer;
        SettlementsPerPlayer = settings.SettlementsPerPlayer;
        CitiesPerPlayer = settings.CitiesPerPlayer;
    }

    public GameSettingsDTO(GameSettingsDTO dto)
    {
        Type = dto.Type;
        MaxPlayers = dto.MaxPlayers;
        VictoryPointsToWin = dto.VictoryPointsToWin;
        RoadsPerPlayer = dto.RoadsPerPlayer;
        SettlementsPerPlayer = dto.SettlementsPerPlayer;
        CitiesPerPlayer = dto.CitiesPerPlayer;
    }
}