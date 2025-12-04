using GameTest.Models;
using GameTest.Services;

public enum TestTile
{
    T0,
    T1,
    T2,
    T3,
    T4,
    T5,
    T6,
}

public enum TestVertex
{
    V1,
    V2,
    V3,
    V4,
    V5,
    V6,
    V7,
    V8,
    V9,
    V10,
    V11,
    V12,
    V13,
    V14,
    V15,
    V16,
    V17,
    V18,
    V19,
    V20,
    V21,
    V22,
    V23,
    V24,
}

public enum TestEdge
{
    E1,
    E2,
    E3,
    E4,
    E5,
    E6,
    E7,
    E8,
    E9,
    E10,
    E11,
    E12,
    E13,
    E14,
    E15,
    E16,
    E17,
    E18,
    E19,
    E20,
    E21,
    E22,
    E23,
    E24,
    E25,
    E26,
    E27,
    E28,
    E29,
    E30,
}

public enum TestPort
{
    Brick,
    ThreeToOne
}

public class TestGameBoard
{
    public Dictionary<TestTile, Tile> Tiles { get; } = new();
    public Dictionary<TestVertex, Vertex> Vertices { get; } = new();
    public Dictionary<TestEdge, Edge> Edges { get; } = new();
    public Dictionary<TestPort, Port> Ports { get; } = new();

    private GameState GS;

    private void PopulateTiles()
    {
        Tiles.Add(TestTile.T0, GS.GetTileAt(0, 0));
        Tiles.Add(TestTile.T1, GS.GetTileAt(1, -1));
        Tiles.Add(TestTile.T2, GS.GetTileAt(2, 0));
        Tiles.Add(TestTile.T3, GS.GetTileAt(1, 1));
        Tiles.Add(TestTile.T4, GS.GetTileAt(-1, 1));
        Tiles.Add(TestTile.T5, GS.GetTileAt(-2, 0));
        Tiles.Add(TestTile.T6, GS.GetTileAt(-1, -1));
    }

    private void PopulateVertices()
    {
        Vertices.Add(TestVertex.V1, BoardCreationHelpers.GetVertexFromTileInfo(GS.Vertices, Tiles[TestTile.T6], Tiles[TestTile.T1], Tiles[TestTile.T0], null));
        Vertices.Add(TestVertex.V2, BoardCreationHelpers.GetVertexFromTileInfo(GS.Vertices, Tiles[TestTile.T2], Tiles[TestTile.T1], Tiles[TestTile.T0], null));
        Vertices.Add(TestVertex.V3, BoardCreationHelpers.GetVertexFromTileInfo(GS.Vertices, Tiles[TestTile.T2], Tiles[TestTile.T3], Tiles[TestTile.T0], null));
        Vertices.Add(TestVertex.V4, BoardCreationHelpers.GetVertexFromTileInfo(GS.Vertices, Tiles[TestTile.T3], Tiles[TestTile.T4], Tiles[TestTile.T0], null));
        Vertices.Add(TestVertex.V5, BoardCreationHelpers.GetVertexFromTileInfo(GS.Vertices, Tiles[TestTile.T4], Tiles[TestTile.T5], Tiles[TestTile.T0], null));
        Vertices.Add(TestVertex.V6, BoardCreationHelpers.GetVertexFromTileInfo(GS.Vertices, Tiles[TestTile.T5], Tiles[TestTile.T6], Tiles[TestTile.T0], null));
        Vertices.Add(TestVertex.V7, BoardCreationHelpers.GetVertexFromTileInfo(GS.Vertices, Tiles[TestTile.T6], Tiles[TestTile.T1], null, null));
        Vertices.Add(TestVertex.V8, BoardCreationHelpers.GetVertexFromTileInfo(GS.Vertices, Tiles[TestTile.T1], null, null, VertexDirection.N));
        Vertices.Add(TestVertex.V9, BoardCreationHelpers.GetVertexFromTileInfo(GS.Vertices, Tiles[TestTile.T1], null, null, VertexDirection.NE));
        Vertices.Add(TestVertex.V10, BoardCreationHelpers.GetVertexFromTileInfo(GS.Vertices, Tiles[TestTile.T1], Tiles[TestTile.T2], null, null));
        Vertices.Add(TestVertex.V11, BoardCreationHelpers.GetVertexFromTileInfo(GS.Vertices, Tiles[TestTile.T2], null, null, VertexDirection.NE));
        Vertices.Add(TestVertex.V12, BoardCreationHelpers.GetVertexFromTileInfo(GS.Vertices, Tiles[TestTile.T2], null, null, VertexDirection.SE));
        Vertices.Add(TestVertex.V13, BoardCreationHelpers.GetVertexFromTileInfo(GS.Vertices, Tiles[TestTile.T2], Tiles[TestTile.T3], null, null));
        Vertices.Add(TestVertex.V14, BoardCreationHelpers.GetVertexFromTileInfo(GS.Vertices, Tiles[TestTile.T3], null, null, VertexDirection.SE));
        Vertices.Add(TestVertex.V15, BoardCreationHelpers.GetVertexFromTileInfo(GS.Vertices, Tiles[TestTile.T3], null, null, VertexDirection.S));
        Vertices.Add(TestVertex.V16, BoardCreationHelpers.GetVertexFromTileInfo(GS.Vertices, Tiles[TestTile.T3], Tiles[TestTile.T4], null, null));
        Vertices.Add(TestVertex.V17, BoardCreationHelpers.GetVertexFromTileInfo(GS.Vertices, Tiles[TestTile.T4], null, null, VertexDirection.S));
        Vertices.Add(TestVertex.V18, BoardCreationHelpers.GetVertexFromTileInfo(GS.Vertices, Tiles[TestTile.T4], null, null, VertexDirection.SW));
        Vertices.Add(TestVertex.V19, BoardCreationHelpers.GetVertexFromTileInfo(GS.Vertices, Tiles[TestTile.T4], Tiles[TestTile.T5], null, null));
        Vertices.Add(TestVertex.V20, BoardCreationHelpers.GetVertexFromTileInfo(GS.Vertices, Tiles[TestTile.T5], null, null, VertexDirection.SW));
        Vertices.Add(TestVertex.V21, BoardCreationHelpers.GetVertexFromTileInfo(GS.Vertices, Tiles[TestTile.T5], null, null, VertexDirection.NW));
        Vertices.Add(TestVertex.V22, BoardCreationHelpers.GetVertexFromTileInfo(GS.Vertices, Tiles[TestTile.T6], Tiles[TestTile.T5], null, null));
        Vertices.Add(TestVertex.V23, BoardCreationHelpers.GetVertexFromTileInfo(GS.Vertices, Tiles[TestTile.T6], null, null, VertexDirection.NW));
        Vertices.Add(TestVertex.V24, BoardCreationHelpers.GetVertexFromTileInfo(GS.Vertices, Tiles[TestTile.T6], null, null, VertexDirection.N));
    }

    private void PopulateEdges()
    {
        Edges.Add(TestEdge.E1, BoardCreationHelpers.GetEdgeFromTileInfo(GS.Edges, Tiles[TestTile.T0], Tiles[TestTile.T1], null));
        Edges.Add(TestEdge.E2, BoardCreationHelpers.GetEdgeFromTileInfo(GS.Edges, Tiles[TestTile.T0], Tiles[TestTile.T2], null));
        Edges.Add(TestEdge.E3, BoardCreationHelpers.GetEdgeFromTileInfo(GS.Edges, Tiles[TestTile.T0], Tiles[TestTile.T3], null));
        Edges.Add(TestEdge.E4, BoardCreationHelpers.GetEdgeFromTileInfo(GS.Edges, Tiles[TestTile.T0], Tiles[TestTile.T4], null));
        Edges.Add(TestEdge.E5, BoardCreationHelpers.GetEdgeFromTileInfo(GS.Edges, Tiles[TestTile.T0], Tiles[TestTile.T5], null));
        Edges.Add(TestEdge.E6, BoardCreationHelpers.GetEdgeFromTileInfo(GS.Edges, Tiles[TestTile.T0], Tiles[TestTile.T6], null));
        Edges.Add(TestEdge.E7, BoardCreationHelpers.GetEdgeFromTileInfo(GS.Edges, Tiles[TestTile.T6], Tiles[TestTile.T1], null));
        Edges.Add(TestEdge.E8, BoardCreationHelpers.GetEdgeFromTileInfo(GS.Edges, Tiles[TestTile.T1], Tiles[TestTile.T2], null));
        Edges.Add(TestEdge.E9, BoardCreationHelpers.GetEdgeFromTileInfo(GS.Edges, Tiles[TestTile.T2], Tiles[TestTile.T3], null));
        Edges.Add(TestEdge.E10, BoardCreationHelpers.GetEdgeFromTileInfo(GS.Edges, Tiles[TestTile.T3], Tiles[TestTile.T4], null));
        Edges.Add(TestEdge.E11, BoardCreationHelpers.GetEdgeFromTileInfo(GS.Edges, Tiles[TestTile.T4], Tiles[TestTile.T5], null));
        Edges.Add(TestEdge.E12, BoardCreationHelpers.GetEdgeFromTileInfo(GS.Edges, Tiles[TestTile.T6], Tiles[TestTile.T5], null));
        Edges.Add(TestEdge.E13, BoardCreationHelpers.GetEdgeFromTileInfo(GS.Edges, Tiles[TestTile.T1], null, HexDirection.NW));
        Edges.Add(TestEdge.E14, BoardCreationHelpers.GetEdgeFromTileInfo(GS.Edges, Tiles[TestTile.T1], null, HexDirection.NE));
        Edges.Add(TestEdge.E15, BoardCreationHelpers.GetEdgeFromTileInfo(GS.Edges, Tiles[TestTile.T1], null, HexDirection.E));
        Edges.Add(TestEdge.E16, BoardCreationHelpers.GetEdgeFromTileInfo(GS.Edges, Tiles[TestTile.T2], null, HexDirection.NE));
        Edges.Add(TestEdge.E17, BoardCreationHelpers.GetEdgeFromTileInfo(GS.Edges, Tiles[TestTile.T2], null, HexDirection.E));
        Edges.Add(TestEdge.E18, BoardCreationHelpers.GetEdgeFromTileInfo(GS.Edges, Tiles[TestTile.T2], null, HexDirection.SE));
        Edges.Add(TestEdge.E19, BoardCreationHelpers.GetEdgeFromTileInfo(GS.Edges, Tiles[TestTile.T3], null, HexDirection.E));
        Edges.Add(TestEdge.E20, BoardCreationHelpers.GetEdgeFromTileInfo(GS.Edges, Tiles[TestTile.T3], null, HexDirection.SE));
        Edges.Add(TestEdge.E21, BoardCreationHelpers.GetEdgeFromTileInfo(GS.Edges, Tiles[TestTile.T3], null, HexDirection.SE));
        Edges.Add(TestEdge.E22, BoardCreationHelpers.GetEdgeFromTileInfo(GS.Edges, Tiles[TestTile.T4], null, HexDirection.SE));
        Edges.Add(TestEdge.E23, BoardCreationHelpers.GetEdgeFromTileInfo(GS.Edges, Tiles[TestTile.T4], null, HexDirection.SW));
        Edges.Add(TestEdge.E24, BoardCreationHelpers.GetEdgeFromTileInfo(GS.Edges, Tiles[TestTile.T4], null, HexDirection.W));
        Edges.Add(TestEdge.E25, BoardCreationHelpers.GetEdgeFromTileInfo(GS.Edges, Tiles[TestTile.T5], null, HexDirection.SW));
        Edges.Add(TestEdge.E26, BoardCreationHelpers.GetEdgeFromTileInfo(GS.Edges, Tiles[TestTile.T5], null, HexDirection.W));
        Edges.Add(TestEdge.E27, BoardCreationHelpers.GetEdgeFromTileInfo(GS.Edges, Tiles[TestTile.T5], null, HexDirection.NW));
        Edges.Add(TestEdge.E28, BoardCreationHelpers.GetEdgeFromTileInfo(GS.Edges, Tiles[TestTile.T6], null, HexDirection.W));
        Edges.Add(TestEdge.E29, BoardCreationHelpers.GetEdgeFromTileInfo(GS.Edges, Tiles[TestTile.T6], null, HexDirection.NW));
        Edges.Add(TestEdge.E30, BoardCreationHelpers.GetEdgeFromTileInfo(GS.Edges, Tiles[TestTile.T6], null, HexDirection.NE));
    }

    private void PoppulatePorts()
    {
        Ports.Add(TestPort.Brick, GS.Ports.First(r => r.Type == PortType.Brick));
        Ports.Add(TestPort.ThreeToOne, GS.Ports.First(r => r.Type == PortType.ThreeToOne));
    }

    private void CreateBoardInGameState(List<ResourceType> resources, List<int> diceValues)
    {
        if (resources == null || resources.Count != 7 || diceValues == null || diceValues.Count != 7)
            throw new InvalidOperationException("TestGameBoard requires 7 resources and dice values.");

        List<Tile> tiles = new List<Tile>
        {
            new Tile(resources[0], diceValues[0], -1, -1),
            new Tile(resources[1], diceValues[1], 1, -1),
            new Tile(resources[2], diceValues[2], -2, 0),
            new Tile(resources[3], diceValues[3], 0, 0),
            new Tile(resources[4], diceValues[4], 2, 0),
            new Tile(resources[5], diceValues[5], -1, 1),
            new Tile(resources[6], diceValues[6], 1, 1),
            
        };

        GS.Tiles.AddRange(tiles);

        BoardCreationHelpers.CreateEdgesAndVerticesForBoard(GS);
        BoardCreationHelpers.LinkEdgesAndVertices(GS);

        PopulateTiles();
        PopulateVertices();
        PopulateEdges();

        GS.Ports.Add(new Port(Vertices[TestVertex.V10], Vertices[TestVertex.V11], PortType.Brick));
        GS.Ports.Add(new Port(Vertices[TestVertex.V14], Vertices[TestVertex.V15], PortType.ThreeToOne));

        PoppulatePorts();
    }


    public TestGameBoard(List<ResourceType> resources, List<int> diceValues, bool bluePlayerBot = false)
    {
         GS = new GameState(new Guid(), GameType.Test);
         CreateBoardInGameState(resources, diceValues);

         GS.Players.Add(new Player("PlayerA", PlayerColor.Red));
         GS.Players.Add(new Player("PlayerB", PlayerColor.Blue, bluePlayerBot));
    }

    public GameState GetGameState()
    {
        return GS;
    }

    public Player GetRedPlayer()
    {
        return GS.Players.First(p => p.Color == PlayerColor.Red);
    }

    public Player GetBluePlayer()
    {
        return GS.Players.First(p => p.Color == PlayerColor.Blue);
    }

    public void SetRobberTile(Tile tile)
    {
        GS.SetRobberTile(tile);
    }

    public Tile GetTile(TestTile tileRef)
    {
        return Tiles[tileRef];
    }

    public Vertex GetVertex(TestVertex vertexRef)
    {
        return Vertices[vertexRef];
    }

    public Edge GetEdge(TestEdge edgeRef)
    {
        return Edges[edgeRef];
    }

    public bool InExpectedState(GameStates state, Player currentPlayer, Player? endPlayer = null)
    {
        return GS.Phase != null &&
            GS.Phase.PhaseState == state && 
            GS.Phase.CurrentPlayer != null &&
            GS.Phase.CurrentPlayer.Id == currentPlayer.Id &&
            (endPlayer == null || (GS.Phase.EndPlayer != null && GS.Phase.EndPlayer.Id == endPlayer.Id));
    }

    public Player GetCurrentPlayer()
    {
        if (GS.Phase == null || GS.Phase.CurrentPlayer == null)
            throw new InvalidOperationException("Unexpected Error. Current player is not set.");

        return GS.Phase.CurrentPlayer;
    }
}