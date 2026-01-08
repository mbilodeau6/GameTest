using GameTest.DTOs;
using GameTest.Models;

public class GameSettings
{
    public const int DefaultBankTradeRate = 4;

    public GameType Type { get; }
    public int MaxPlayers { get; }
    public int VictoryPointsToWin { get; }
    public int RoadsPerPlayer { get; }
    public int SettlementsPerPlayer { get; }
    public int CitiesPerPlayer { get; }
    public string Creator { get; init; } = string.Empty;

    public GameSettings(GameType type = GameType.Default,
        string creator = "", int maxPlayers = 4,
        int victoryPointsToWin = 10, int roadsPerPlayer = 15,
        int settlementsPerPlayer = 5, int citiesPerPlayer = 4)
    {
        Type = type;
        MaxPlayers = maxPlayers;
        VictoryPointsToWin = victoryPointsToWin;
        RoadsPerPlayer = roadsPerPlayer;
        SettlementsPerPlayer = settlementsPerPlayer;
        CitiesPerPlayer = citiesPerPlayer;
        Creator = creator;
    }
    
    public GameSettings(GameSettingsDTO dto)
    {
        Type = dto.Type;
        MaxPlayers = dto.MaxPlayers;
        VictoryPointsToWin = dto.VictoryPointsToWin;
        RoadsPerPlayer = dto.RoadsPerPlayer;
        SettlementsPerPlayer = dto.SettlementsPerPlayer;
        CitiesPerPlayer = dto.CitiesPerPlayer;
        Creator = dto.Creator;
    }
}