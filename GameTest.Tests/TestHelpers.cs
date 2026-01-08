using GameTest.Models;
using GameTest.Services;

namespace GameTest.Tests;

public static class TestHelpers
{
    // Validates that the input string starts with the expected letter followed by a positive integer.
    // Example: "T1", "E23", "V5"
    public static bool ValidateId(string input, char expectedLetter)
    {
        if (string.IsNullOrEmpty(input) || input[0] != expectedLetter)
            return false;

        // Check if the rest is a number > 0
        if (input.Length < 2)
            return false;

        string numberPart = input.Substring(1);
        if (int.TryParse(numberPart, out int number))
            return number > 0;

        return false;
    }

    public static bool IsGameStateValid(GameState gameState)
    {
        if (gameState == null)
            throw new ArgumentNullException(nameof(gameState));

        if (gameState.Players == null || gameState.Players.Count < 2 || gameState.Players.Count > 8)
            throw new ArgumentException("Game must have between 2 and 8 players.");

        if (gameState.Tiles == null || gameState.Edges == null || gameState.Vertices == null)
            throw new ArgumentException("GameState must have non-null Tiles, Edges, and Vertices collections.");

        if (gameState.Tiles.Any(t => t == null || t.DiceNumber < 2 || t.DiceNumber > 12))
            throw new ArgumentException("All tiles must be non-null and have a valid DiceNumber (2-12).");

        if (gameState.Edges.Any(e => e == null
            || e.Tiles == null
            || e.Tiles.Count == 0
            || (e.Direction == null && e.Tiles.Count < 2)
            || (e.Direction != null && e.Tiles.Count != 1)))
            throw new ArgumentException("All edges must be non-null and have valid Tiles and Direction.");

        if (gameState.Vertices.Any(v => v == null
            || v.Tiles == null
            || v.Tiles.Count == 0
            || (v.Direction == null && v.Tiles.Count < 2)
            || (v.Direction != null && v.Tiles.Count != 1)
            || (v.Building != null && v.Owner == null)
            || (v.Building == null && v.Owner != null)))
            throw new ArgumentException("All vertices must be non-null and have valid Tiles, Direction, Building, and Owner.");

        if (gameState.RobberTile != null && !gameState.Tiles.Contains(gameState.RobberTile))
            throw new ArgumentException("RobberTile must correspond to an existing tile in the game.");

        return true;
    }

    public static GameState CreateEdgesAndVertexForRefTests()
    {
        var gs = new GameState(new Guid());

        var t1 = new Tile(ResourceType.Wood, 10, 0, 0);
        gs.Tiles.Add(t1);
        var t2 = new Tile(ResourceType.Brick, 2, -1, -1);
        gs.Tiles.Add(t2);
        var t3 = new Tile(ResourceType.Grain, 9, 1, -1);
        gs.Tiles.Add(t3);
        var t4 = new Tile(ResourceType.Wool, 8, -2, 0);
        gs.Tiles.Add(t4);
        var t5 = new Tile(ResourceType.Ore, 5, 0, -2);
        gs.Tiles.Add(t5);

        var v1 = new Vertex(t1, t2, t3);
        gs.Vertices.Add(v1);
        var v2 = new Vertex(t1, t2, t4);
        gs.Vertices.Add(v2);
        var v3 = new Vertex(t2, t2, t5);
        gs.Vertices.Add(v3);

        var e1 = new Edge(t1, t2);
        gs.Edges.Add(e1);
        var e2 = new Edge(t1, t3);
        gs.Edges.Add(e2);
        var e3 = new Edge(t2, t3);
        gs.Edges.Add(e3);
        var e4 = new Edge(t2, t4);
        gs.Edges.Add(e4);

        return gs;
    }

    public static class SetUpPhaseTestReferences
    {
        public static Tile WoodTile = new Tile(ResourceType.Wood, 9, 0, 0);
        public static Tile Wool2Tile = new Tile(ResourceType.Wool, 2, -1, -1);
        public static Tile BrickTile = new Tile(ResourceType.Brick, 3, 1, -1);
        public static Tile OreTile = new Tile(ResourceType.Ore, 4, 2, 0);
        public static Tile Wool5Tile = new Tile(ResourceType.Wool, 5, 1, 1);
        public static Tile GrainTile = new Tile(ResourceType.Grain, 6, -1, 1);
        public static Tile DesertTile = new Tile(ResourceType.Desert, 0, -2, 0);
        public static Player HumanPlayer = Player.CreateTestPlayer("Player1", PlayerColor.Red, isBot: false);
        public static Player BotPlayer = Player.CreateTestPlayer("Player2", PlayerColor.Blue, isBot: true);
    }

    public static GameState CreateGameStateForSetUpPhase()
    {
        var gs = new GameState(new Guid());
        gs.Phase.PhaseState = GameStates.PlaceFirstSettlement;
        gs.Phase.CurrentPlayer = SetUpPhaseTestReferences.BotPlayer;

        gs.Tiles.AddRange(new List<Tile>() {
            SetUpPhaseTestReferences.WoodTile,
            SetUpPhaseTestReferences.Wool2Tile,
            SetUpPhaseTestReferences.BrickTile,
            SetUpPhaseTestReferences.OreTile,
            SetUpPhaseTestReferences.Wool5Tile,
            SetUpPhaseTestReferences. GrainTile,
            SetUpPhaseTestReferences.DesertTile
        });

        BoardCreationHelpers.CreateEdgesAndVerticesForBoard(gs);

        gs.AddPlayer(SetUpPhaseTestReferences.HumanPlayer);
        gs.AddPlayer(SetUpPhaseTestReferences.BotPlayer);

        return gs;
    }

    public static TestGameBoard CreateOriginalTestBoard(bool bluePlayerBot = false)
    {
        var board = new TestGameBoard(
            new List<ResourceType>() {ResourceType.Desert, ResourceType.Wool, ResourceType.Brick, ResourceType.Grain, ResourceType.Ore, ResourceType.Wool, ResourceType.Wood}, 
            new List<int>() {0, 11, 5, 9, 3, 2, 6},
            bluePlayerBot
        );

        board.SetRobberTile(board.GetTile(TestTile.T6));

        return board;
    }

    public static TestGameBoard CreateOriginalTestBoardWithSettlements(bool bluePlayerBot = false)
    {
        var board = CreateOriginalTestBoard(bluePlayerBot);
        board.GetVertex(TestVertex.V3).BuildSettlement(board.GetBluePlayer());
        board.GetEdge(TestEdge.E3).BuildRoad(board.GetBluePlayer());
        board.GetVertex(TestVertex.V5).BuildSettlement(board.GetRedPlayer());
        board.GetEdge(TestEdge.E11).BuildRoad(board.GetRedPlayer());
        board.GetGameState().UpdatePlayerVictoryPoints(board.GetBluePlayer());
        board.GetGameState().UpdatePlayerVictoryPoints(board.GetRedPlayer());
        GamePlayHelpers.MarkBlockedVertices(board.GetGameState());
        
        return board;
    }

    // TODO: Should be able to get rid of this helper when all of the tests are refactored to use
    // Test Boards.
    public static void AddPlayers(GameState gameState)
    {
        var p1 = Player.CreateTestPlayer("Lisa", PlayerColor.Red);
        var p2 = Player.CreateTestPlayer("Hal", PlayerColor.Blue, true);

        gameState.AddPlayer(p1);
        gameState.AddPlayer(p2);
    }

    public static Player CreatePlayerWithSufficientResources()
    {
        var player = Player.CreateTestPlayer("Mary", PlayerColor.Red);

        player.Resources[ResourceType.Wood] = 1;
        player.Resources[ResourceType.Brick] = 1;
        player.Resources[ResourceType.Grain] = 2;
        player.Resources[ResourceType.Wool] = 1;
        player.Resources[ResourceType.Ore] = 3;

        return player;
    }

    public static Player CreatePlayerWithInsufficientResources()
    {
        var player = Player.CreateTestPlayer("Mary", PlayerColor.Red);

        player.Resources[ResourceType.Wood] = 1;
        player.Resources[ResourceType.Brick] = 0;
        player.Resources[ResourceType.Grain] = 2;
        player.Resources[ResourceType.Wool] = 0;
        player.Resources[ResourceType.Ore] = 2;

        return player;
    }
}