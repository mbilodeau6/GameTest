using GameTest.Models;

namespace GameTest.Services;

public static class BoardCreationHelpers
{
    public static Tile GetRequiredTileAt(this IEnumerable<Tile> tiles, int x, int y)
       => tiles.First(t => t.X == x && t.Y == y);

    public static List<Tile> CreateTilesForRandomBoard()
    {
        var resourceValues = new List<ResourceType>();
        resourceValues.AddRange(Enumerable.Repeat(ResourceType.Brick, 3));
        resourceValues.AddRange(Enumerable.Repeat(ResourceType.Wood, 4));
        resourceValues.AddRange(Enumerable.Repeat(ResourceType.Wool, 4));
        resourceValues.AddRange(Enumerable.Repeat(ResourceType.Grain, 4));
        resourceValues.AddRange(Enumerable.Repeat(ResourceType.Ore, 3));
        resourceValues.Add(ResourceType.Desert);

        var rnd = new Random();
        for (int i = resourceValues.Count - 1; i > 0; i--)
        {
            int j = rnd.Next(i + 1);
            var tmp = resourceValues[i];
            resourceValues[i] = resourceValues[j];
            resourceValues[j] = tmp;
        }

        var diceValues = new List<int>
        {
            5, 2, 6, 3, 8, 10, 9, 12, 11, 4, 8, 10, 9, 4, 5, 6, 3, 11
        };

        var xCoordinates = new List<int>
        {
            -2, -3, -4, -3, -2, 0, 2, 3, 4, 3, 2, 0, -1, -2, -1, 1, 2, 1, 0
        };

        var yCoordinates = new List<int>
        {
            -2, -1, 0, 1, 2, 2, 2, 1, 0, -1, -2, -2, -1, 0, 1, 1, 0, -1, 0
        };

        int diceIndex = 0;
        int coordIndex = 0;

        List<Tile> tiles = new List<Tile>();

        foreach (var resourceType in resourceValues)
        {
            if (resourceType != ResourceType.Desert)
            {
                tiles.Add(new Tile(resourceType, diceValues[diceIndex], xCoordinates[coordIndex], yCoordinates[coordIndex]));
                diceIndex++;
            }
            else
            {
                tiles.Add(new Tile(resourceType, 7, xCoordinates[coordIndex], yCoordinates[coordIndex]));
            }
            coordIndex++;
        }

        return tiles;
    }

    public static List<Tile> CreateTilesForStarterBoard()
    {
        List<Tile> tiles = new List<Tile>
        {
            new Tile(ResourceType.Ore, 10, -2, -2),
            new Tile(ResourceType.Grain, 12, -3, -1),
            new Tile(ResourceType.Grain, 9, -4, 0),
            new Tile(ResourceType.Wood, 8, -3, 1),
            new Tile(ResourceType.Brick, 5, -2, 2),
            new Tile(ResourceType.Grain, 6, 0, 2),
            new Tile(ResourceType.Wool, 11, 2, 2),
            new Tile(ResourceType.Wool, 5, 3, 1),
            new Tile(ResourceType.Ore, 8, 4, 0),
            new Tile(ResourceType.Brick, 10, 3, -1),
            new Tile(ResourceType.Wood, 9, 2, -2),
            new Tile(ResourceType.Wool, 2, 0, -2),
            new Tile(ResourceType.Brick, 6, -1, -1),
            new Tile(ResourceType.Wood, 11, -2, 0),
            new Tile(ResourceType.Ore, 3, -1, 1),
            new Tile(ResourceType.Grain, 4, 1, 1),
            new Tile(ResourceType.Wood, 3, 2, 0),
            new Tile(ResourceType.Wool, 4, 1, -1),
            new Tile(ResourceType.Desert, 7, 0, 0), // Desert tile
        };

        return tiles;
    }

    public static List<Tile> CreateTilesForTestBoard()
    {
        List<Tile> tiles = new List<Tile>
        {
            new Tile(ResourceType.Desert, 0, -1, -1),
            new Tile(ResourceType.Wool, 8, 1, -1),
            new Tile(ResourceType.Brick, 5, -2, 0),
            new Tile(ResourceType.Grain, 10, 0, 0),
            new Tile(ResourceType.Ore, 3, 2, 0),
            new Tile(ResourceType.Wool, 2, -1, 1),
            new Tile(ResourceType.Wood, 6, 1, 1),
        };

        return tiles;
    }

    public static void CreateEdgesAndVerticesForBoard(GameState gameState)
    {
        var stack = new Stack<Tile>();
        stack.Push(GetRequiredTileAt(gameState.Tiles, 0, 0));

        while (stack.Count > 0)
        {
            var tile = stack.Pop();

            foreach (HexDirection dir in Enum.GetValues(typeof(HexDirection)))
            {
                var neighborCoordinates = HexProximity.GetCoordinates((tile.X, tile.Y), dir);
                var neighborTile = gameState.Tiles.FirstOrDefault(t => t.X == neighborCoordinates.Item1 && t.Y == neighborCoordinates.Item2);
                if (neighborTile != null)
                {
                    if (!gameState.Edges.Any(e => e.ConnectsTiles(tile.Id, neighborTile.Id)))
                    {
                        var edge = new Edge(tile, neighborTile);
                        gameState.AddEdge(edge);
                        stack.Push(neighborTile);
                    }
                }
                else
                {
                    if (!gameState.Edges.Any(e => e.Tiles[0].Id == tile.Id && e.Direction == dir))
                    {
                        var edge = new Edge(tile, dir);
                        gameState.AddEdge(edge);
                    }
                }

                var neighbor2Coordinates = HexProximity.GetCoordinates((tile.X, tile.Y), HexProximity.getPrecedingDirection(dir));
                var neighbor2Tile = gameState.Tiles.FirstOrDefault(t => t.X == neighbor2Coordinates.Item1 && t.Y == neighbor2Coordinates.Item2);
                var vertexDir = HexProximity.GetVertexDirectionForEdgeDirection(dir);

                // If no neighboring tiles, create vertex with just this tile and direction
                if (neighborTile == null && neighbor2Tile == null)
                {
                    if (!gameState.Vertices.Any(v => v.Tiles[0].Id == tile.Id && v.Direction == vertexDir))
                    {
                        var vertex = new Vertex(tile, vertexDir);
                        gameState.AddVertex(vertex);
                    }
                }
                // Otherwise, create vertex with this tile and any neighboring tiles
                else if (neighborTile != null && neighbor2Tile != null)
                {
                    if (!gameState.Vertices.Any(v => v.ConnectsTiles(tile, neighborTile, neighbor2Tile)))
                    {
                        var vertex = new Vertex(tile, neighborTile, neighbor2Tile);
                        gameState.AddVertex(vertex);
                    }
                }
                else if (neighborTile != null)
                {
                    if (!gameState.Vertices.Any(v => v.ConnectsTiles(tile, neighborTile)))
                    {
                        var vertex = new Vertex(tile, neighborTile);
                        gameState.AddVertex(vertex);
                    }
                }
                else if (neighbor2Tile != null)
                {
                    if (!gameState.Vertices.Any(v => v.ConnectsTiles(tile, neighbor2Tile)))
                    {
                        var vertex = new Vertex(tile, neighbor2Tile);
                        gameState.AddVertex(vertex);
                    }
                }
            }
        }

    }

    public static GameState CreateNewBoard(GameType gameType)
    {
        var gameState = new GameState(Guid.NewGuid(), gameType);

        switch (gameType)
        {
            case GameType.Default:
                foreach (var tile in CreateTilesForRandomBoard())
                    gameState.AddTile(tile);
                break;
            case GameType.Expansion6:
                throw new NotImplementedException("Default and Expansion6 board types are not implemented yet.");
            case GameType.Expansion8:
                throw new NotImplementedException("Expansion8 board type is not implemented yet.");
            case GameType.Starter:
                foreach (var tile in CreateTilesForStarterBoard())
                    gameState.AddTile(tile);
                break;
            case GameType.Test:
                foreach (var tile in CreateTilesForTestBoard())
                    gameState.AddTile(tile);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(gameType), $"Unhandled game type: {gameType}");
        }

        gameState.PlaceRobberOnDesert();
        CreateEdgesAndVerticesForBoard(gameState);

        if (gameType == GameType.Test)
        {
            // Add some roads/settlements for testing purposes
            var player1 = new Player("Julie", PlayerColor.Red);
            var player2 = new Player("John", PlayerColor.Blue);
            gameState.AddPlayer(player1);
            gameState.AddPlayer(player2);

            var BrickTile = GetRequiredTileAt(gameState.Tiles, -2, 0);
            var GrainTile = GetRequiredTileAt(gameState.Tiles, 0, 0);
            var OreTile = GetRequiredTileAt(gameState.Tiles, 2, 0);
            var LowerWoolTile = GetRequiredTileAt(gameState.Tiles, -1, 1);
            var WoodTile = GetRequiredTileAt(gameState.Tiles, 1, 1);

            var redEdge = gameState.Edges.First(e => e.Tiles.Contains(BrickTile) && e.Tiles.Contains(LowerWoolTile));
            redEdge.BuildRoad(player1);

            var redVertex = gameState.Vertices.First(v => v.Tiles.Contains(BrickTile) && v.Tiles.Contains(GrainTile) && v.Tiles.Contains(LowerWoolTile));
            redVertex.BuildSettlement(player1);

            var blueEdge = gameState.Edges.First(e => e.Tiles.Contains(GrainTile) && e.Tiles.Contains(WoodTile));
            blueEdge.BuildRoad(player2);

            var blueVertex = gameState.Vertices.First(v => v.Tiles.Contains(OreTile) && v.Tiles.Contains(GrainTile) && v.Tiles.Contains(WoodTile));
            blueVertex.BuildSettlement(player2);
        }
        else
        {
            BoardCreationHelpers.AddPlayers(gameState);
        }

        return gameState;
    }
    
    public static void AddPlayers(GameState gameState)
    {
        var p1 = new Player("Bob", PlayerColor.Blue);
        var p2 = new Player("Mary", PlayerColor.Red);

        gameState.AddPlayer(p1);
        gameState.AddPlayer(p2);
    }
}