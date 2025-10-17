using System.Collections.Generic;
using GameTest.DTOs;

namespace GameTest.Models;

public class GameState
{
    public Guid Id { get; init; }
    public GameSettings Settings { get; private set; } = new GameSettings();
    public List<Player> Players { get; } = new();
    public List<Tile> Tiles { get; } = new();
    public List<Edge> Edges { get; } = new();
    public List<Vertex> Vertices { get; } = new();
    public Tile RobberTile { get; private set; } = null!;
    public Player PlayerWithLongestRoad { get; private set; } = null!;
    public Player PlayerWithLargestArmy { get; private set; } = null!;

    public Dictionary<ResourceType, int> Resources { get; } = new()
    {
        { ResourceType.Brick, 19 },
        { ResourceType.Wood, 19 },
        { ResourceType.Ore, 19 },
        { ResourceType.Grain, 19 },
        { ResourceType.Wool, 19 }
    };

    public List<DevelopmentCardType> DevelopmentCards { get; private set; } = new List<DevelopmentCardType>();

    public Player CurrentPlayer { get; private set; } = null!;
    public GameStates CurrentState { get; private set; } = GameStates.PrePlay;

    // Future: Add collections for Ports

    private void InitializeDevelopmentCards()
    {
        List<DevelopmentCardType> developmentCards = new List<DevelopmentCardType>();

        for (int i = 0; i < 2; i++)
        {
            developmentCards.Add(DevelopmentCardType.Monopoly);
            developmentCards.Add(DevelopmentCardType.RoadBuilding);
            developmentCards.Add(DevelopmentCardType.YearOfPlenty);
        }

        for (int i = 0; i < 14; i++)
            developmentCards.Add(DevelopmentCardType.Knight);

        for (int i = 0; i < 5; i++)
            developmentCards.Add(DevelopmentCardType.VictoryPoint);

        // Shuffle the development cards
        var rnd = new Random();
        DevelopmentCards = developmentCards.OrderBy(x => rnd.Next()).ToList();
    }

    public GameState(Guid guid, GameType? type = GameType.Default)
    {
        Id = guid;
        Settings.Type = type ?? GameType.Default;

        InitializeDevelopmentCards();
    }

    public GameState(GameStateDTO dto)
    {
        Id = Guid.Parse(dto.Id);

        foreach (var playerDto in dto.Players)
            Players.Add(new Player(playerDto));

        foreach (var tileDto in dto.Tiles)
        {
            Tile tile = new Tile(tileDto);
            Tiles.Add(new Tile(tileDto));
            if (tile.Id == dto.RobberTileId)
                RobberTile = tile;
        }

        foreach (var edgeDto in dto.Edges)
            Edges.Add(new Edge(edgeDto, Players, Tiles));

        foreach (var vertexDto in dto.Vertices)
            Vertices.Add(new Vertex(vertexDto, Players, Tiles));
    }

    public void AddPlayer(Player player)
    {
        Players.Add(player);
    }

    public void AddTile(Tile tile)
    {
        Tiles.Add(tile);
    }

    public void AddEdge(Edge edge)
    {
        Edges.Add(edge);
    }

    public void AddVertex(Vertex vertex)
    {
        Vertices.Add(vertex);
    }

    public void SetRobberTile(Tile tile)
    {
        if (tile.Equals(RobberTile))
            throw new ArgumentException("Robber is already on the specified tile.");

        if (!Tiles.Contains(tile))
            throw new ArgumentException("The specified tile does not exist in the game.");

        RobberTile = tile;
    }
    
    public void PlaceRobberOnDesert()
    {
        var desertTile = Tiles.FirstOrDefault(t => t.Resource == ResourceType.Desert);
        if (desertTile == null)
            throw new InvalidOperationException("No desert tile found in the game.");

        RobberTile = desertTile;
    }
}