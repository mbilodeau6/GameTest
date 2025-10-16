using Microsoft.Identity.Client;

public class GameSettings
{
    public GameType Type { get; set; } = GameType.Default;
    public int MaxPlayers { get; set; } = 2;
    public int VictoryPointsToWin { get; set; } = 10;
    public int RoadsPerPlayer { get; set; } = 15;
    public int SettlementsPerPlayer { get; set; } = 5;
    public int CitiesPerPlayer { get; set; } = 4;
}