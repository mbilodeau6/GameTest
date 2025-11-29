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
    public GameStates? PreRobberState {get; private set; } = null;
    public Tile OriginalRobberTile { get; private set; } = null;

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
    
    public GameSettings(List<Tile> tiles, GameSettingsDTO dto)
    {
        Type = Enum.Parse<GameType>(dto.Type);
        MaxPlayers = dto.MaxPlayers;
        VictoryPointsToWin = dto.VictoryPointsToWin;
        RoadsPerPlayer = dto.RoadsPerPlayer;
        SettlementsPerPlayer = dto.SettlementsPerPlayer;
        CitiesPerPlayer = dto.CitiesPerPlayer;
        if (!string.IsNullOrEmpty(dto.PreRobberState))
            PreRobberState = Enum.Parse<GameStates>(dto.PreRobberState);

        if (dto.OriginalRobberTileId != null)
            OriginalRobberTile = tiles.First(t => t.Id == dto.OriginalRobberTileId);
    }

    public void SetPreRobberState(GameStates state, Tile originalTile)
    {
        if (PreRobberState != null)
            throw new InvalidOperationException("Unexpected Error. Call to SetPreRobberState when it is already set.");
            
        PreRobberState = state;
        OriginalRobberTile = originalTile;
    }

    public void ClearRobberState()
    {
        PreRobberState = null;
        TODO: OriginalRobberTile = null;
    }
}