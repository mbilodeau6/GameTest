using GameTest.DTOs;
using Microsoft.Identity.Client;

public class GameSettings
{
    public GameType Type { get; }
    public int MaxPlayers { get; }
    public int VictoryPointsToWin { get; }
    public int RoadsPerPlayer { get; }
    public int SettlementsPerPlayer { get; }
    public int CitiesPerPlayer { get; }

    public GameSettings(GameType type = GameType.Default, int maxPlayers = 2,
        int victoryPointsToWin = 10, int roadsPerPlayer = 15,
        int settlementsPerPlayer = 5, int citiesPerPlayer = 4)
    {
        Type = type;
        MaxPlayers = maxPlayers;
        VictoryPointsToWin = victoryPointsToWin;
        RoadsPerPlayer = roadsPerPlayer;
        SettlementsPerPlayer = settlementsPerPlayer;
        CitiesPerPlayer = citiesPerPlayer;
    }
    
    public GameSettings(GameSettingsDTO dto)
    {
        Type = Enum.Parse<GameType>(dto.Type);
        MaxPlayers = dto.MaxPlayers;
        VictoryPointsToWin = dto.VictoryPointsToWin;
        RoadsPerPlayer = dto.RoadsPerPlayer;
        SettlementsPerPlayer = dto.SettlementsPerPlayer;
        CitiesPerPlayer = dto.CitiesPerPlayer;
    }
}