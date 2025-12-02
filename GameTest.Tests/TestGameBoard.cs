using GameTest.Models;

public class TestGameBoard
{
    public Dictionary<string, Tile> Tiles { get; } = new();
    public Dictionary<string, Vertex> Vertices { get; } = new();
    public Dictionary<string, Edge> Edges { get; } = new();
    public Dictionary<string, Port> Ports { get; } = new();

    private GameState GS { get; set; }

    private void PopulateTiles()
    {
        
    }

    private void PopulateVertices()
    {
        
    }

    private void PopulateEdges()
    {
        
    }

    private void PoppulatePorts()
    {
        
    }

    private void CreateBoardInGameState()
    {
        GS.Tiles = new List<Tile>
        {
            new Tile(ResourceType.Desert, 0, -1, -1),
            new Tile(ResourceType.Wool, 11, 1, -1),
            new Tile(ResourceType.Brick, 5, -2, 0),
            new Tile(ResourceType.Grain, 9, 0, 0),
            new Tile(ResourceType.Ore, 3, 2, 0),
            new Tile(ResourceType.Wool, 2, -1, 1),
            new Tile(ResourceType.Wood, 6, 1, 1),
        };

        return tiles;
    }


    public TestGameBoard()
    {
         GS = new GameState(new Guid());
    }

    public GameState GetGameState()
    {
        return GS;
    }
}