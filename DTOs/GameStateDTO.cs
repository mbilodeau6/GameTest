using System.Text.Json.Serialization;
using GameTest.Models;
using GameTest.Tests;
using Microsoft.Extensions.Logging;

namespace GameTest.DTOs;

public class GameStateDTO
{
    public string Id { get; private set; }
    public GameSettingsDTO Settings { get; }

    public GamePhaseDTO? Phase { get;  }
    public string HasLongestRoadPlayerId { get; } = string.Empty;
    public string HasLargestArmyPlayerId { get; } = string.Empty;
    public string RobberTileId { get; } = string.Empty;
    public GameDice Dice { get; } = new GameDice(true);
    public List<PlayerDTO> Players { get; } = new();
    public List<TileDTO> Tiles { get; } = new();
    public List<EdgeDTO> Edges { get; } = new();
    public List<VertexDTO> Vertices { get; } = new();
    public List<PortDTO> Ports {get; } = new();


    // JsonConstructor lets System.Text.Json bind constructor parameters to JSON properties.
    [JsonConstructor]
    public GameStateDTO(string id, GameSettingsDTO settings, string? robberTileId = null,
        GameDice? dice = null, GamePhaseDTO? phase = null,
        string? hasLongestRoadPlayerId = null, string? hasLargestArmyPlayerId = null,
        List<PlayerDTO>? players = null, List<TileDTO>? tiles = null,
        List<EdgeDTO>? edges = null, List<VertexDTO>? vertices = null,
        List<PortDTO>? ports = null)
    {
        Id = id ?? string.Empty;
        Settings = settings;
        RobberTileId = robberTileId ?? string.Empty;
        Dice = dice ?? new GameDice(true);

        foreach (var player in players ?? Enumerable.Empty<PlayerDTO>())
            Players.Add(player);

        foreach (var tile in tiles ?? Enumerable.Empty<TileDTO>())
            Tiles.Add(tile);

        foreach (var edge in edges ?? Enumerable.Empty<EdgeDTO>())
            Edges.Add(edge);

        foreach (var vertex in vertices ?? Enumerable.Empty<VertexDTO>())
            Vertices.Add(vertex);

        foreach (var port in ports ?? Enumerable.Empty<PortDTO>())
            Ports.Add(port);

        Phase = phase;
        HasLongestRoadPlayerId = hasLongestRoadPlayerId ?? string.Empty;
        HasLargestArmyPlayerId = hasLargestArmyPlayerId ?? string.Empty;
    }

    private GameStateDTO(GameStateDTO dto)
    {
        Id = dto.Id;
        Settings = new GameSettingsDTO(dto.Settings);

        Phase = dto.Phase;
        HasLongestRoadPlayerId = dto.HasLongestRoadPlayerId;
        HasLargestArmyPlayerId = dto.HasLargestArmyPlayerId;
        RobberTileId = dto.RobberTileId;

        foreach (var player in dto.Players)
            Players.Add(player);

        foreach (var tile in dto.Tiles)
            Tiles.Add(tile);

        foreach (var edge in dto.Edges)
            Edges.Add(edge);

        foreach (var vertex in dto.Vertices)
            Vertices.Add(vertex);

        foreach (var port in dto.Ports)
            Ports.Add(port);

        Dice = dto.Dice;
    }

    public GameStateDTO(GameState gameState)
    {
        Id = gameState.Id.ToString();
        Settings = new GameSettingsDTO(gameState.Settings);

        foreach (var player in gameState.Players)
            Players.Add(new PlayerDTO(player, false));

        foreach (var tile in gameState.Tiles)
            Tiles.Add(new TileDTO(tile));

        foreach (var edge in gameState.Edges)
            Edges.Add(new EdgeDTO(edge));

        foreach (var vertex in gameState.Vertices)
            Vertices.Add(new VertexDTO(vertex));

        foreach (var port in gameState.Ports)
            Ports.Add(new PortDTO(port));

        if (gameState.RobberTile != null)
            RobberTileId = gameState.RobberTile.Id;

        Phase = new GamePhaseDTO(gameState.Phase);

        if (gameState.PlayerWithLongestRoad != null)
            HasLongestRoadPlayerId = gameState.PlayerWithLongestRoad.Id;

        if (gameState.PlayerWithLargestArmy != null)
            HasLargestArmyPlayerId = gameState.PlayerWithLargestArmy.Id;

        Dice = gameState.Dice;
    }

    public GameStateDTO GetStateForPlayer(Player player)
    {
        var dto = new GameStateDTO(this);

        // TODO: Change to remove Resource/Development details for every player
        // other than the player identified. This version ignores the player
        // identified and removes everything for Bot players, leaving the human
        // player's details visible.
        List<PlayerDTO> copyOfPlayers = new List<PlayerDTO>();

        foreach (var p in dto.Players)
            copyOfPlayers.Add(p);

        dto.Players.Clear();

        foreach (var p in copyOfPlayers)
            if (p.IsBot)
                dto.Players.Add(new PlayerDTO(new Player(p), true));
            else
                dto.Players.Add(p);

        return dto;
    }
}