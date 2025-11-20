using GameTest.Models;
using GameTest.Tests;

namespace GameTest.Services;

public static class BoardCreationHelpers
{
    public static Tile GetTileAt(this IEnumerable<Tile> tiles, int x, int y)
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
            new Tile(ResourceType.Wool, 11, 1, -1),
            new Tile(ResourceType.Brick, 5, -2, 0),
            new Tile(ResourceType.Grain, 9, 0, 0),
            new Tile(ResourceType.Ore, 3, 2, 0),
            new Tile(ResourceType.Wool, 2, -1, 1),
            new Tile(ResourceType.Wood, 6, 1, 1),
        };

        return tiles;
    }

    public static void CreateEdgesAndVerticesForBoard(GameState gameState)
    {
        var stack = new Stack<Tile>();
        stack.Push(GetTileAt(gameState.Tiles, 0, 0));

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
        LinkEdgesAndVertices(gameState);
        BoardCreationHelpers.AddPlayers(gameState);

        switch(gameType)
        {
            case GameType.Default:
                BoardCreationHelpers.AddPorts(gameState);
                break;
            case GameType.Starter:
                BoardCreationHelpers.AddPortsForStarter(gameState); 
                break;           
        }

        if (gameType == GameType.Test)
        {
            // Add some roads/settlements for testing purposes
            var BrickTile = GetTileAt(gameState.Tiles, -2, 0);
            var GrainTile = GetTileAt(gameState.Tiles, 0, 0);
            var OreTile = GetTileAt(gameState.Tiles, 2, 0);
            var LowerWoolTile = GetTileAt(gameState.Tiles, -1, 1);
            var WoodTile = GetTileAt(gameState.Tiles, 1, 1);

            var redEdge = gameState.Edges.First(e => e.Tiles.Contains(BrickTile) && e.Tiles.Contains(LowerWoolTile));
            redEdge.BuildRoad(gameState.Players[0]);

            var redVertex = gameState.Vertices.First(v => v.Tiles.Contains(BrickTile) && v.Tiles.Contains(GrainTile) && v.Tiles.Contains(LowerWoolTile));
            redVertex.BuildSettlement(gameState.Players[0]);

            var blueEdge = gameState.Edges.First(e => e.Tiles.Contains(GrainTile) && e.Tiles.Contains(WoodTile));
            blueEdge.BuildRoad(gameState.Players[1]);

            var blueVertex = gameState.Vertices.First(v => v.Tiles.Contains(OreTile) && v.Tiles.Contains(GrainTile) && v.Tiles.Contains(WoodTile));
            blueVertex.BuildSettlement(gameState.Players[1]);
        }

        return gameState;
    }

    public static Edge GetEdgeFromTileInfo(List<Edge> edges, Tile t1, Tile? t2, HexDirection? dir)
    {
        if (t2 != null)
            return edges.First(e => e.Tiles.Contains(t1) && e.Tiles.Contains(t2));

        return edges.First(e => e.Tiles.Contains(t1) && e.Direction == dir);
    }

    public static Vertex GetVertexFromTileInfo(List<Vertex> vertices, Tile t1, Tile? t2, Tile? t3, VertexDirection? dir)
    {
        if (t1 != null && t2 != null && t3 != null)
            return vertices.First(v => v.Tiles.Contains(t1) && v.Tiles.Contains(t2) && v.Tiles.Contains(t3));

        if (t1 != null && t2 != null && t3 == null)
            return vertices.First(v => v.Tiles.Count() == 2 && v.Tiles.Contains(t1) && v.Tiles.Contains(t2));

        if (t1 != null && t2 == null && t3 != null)
            return vertices.First(v => v.Tiles.Count() == 2 && v.Tiles.Contains(t1) && v.Tiles.Contains(t3));

        return vertices.First(v => v.Tiles.Count() == 1 && v.Tiles.Contains(t1) && v.Direction == dir);
    }

    public static void LinkEdgesAndVertices(GameState gs)
    {
        var stack = new Stack<Tile>();
        var visitedTiles = new HashSet<string>();

        stack.Push(BoardCreationHelpers.GetTileAt(gs.Tiles, 0, 0));

        while (stack.Count > 0)
        {
            var tile = stack.Pop();
            visitedTiles.Add(tile.Id);

            foreach (HexDirection dir in Enum.GetValues(typeof(HexDirection)))
            {
                // Get neighboring tiles (if they exist)
                var neighborCoordinates = HexProximity.GetCoordinates((tile.X, tile.Y), dir);
                var neighborTile = gs.Tiles.FirstOrDefault(t => t.X == neighborCoordinates.Item1 && t.Y == neighborCoordinates.Item2);
                if (neighborTile != null && !visitedTiles.Contains(neighborTile.Id))
                    stack.Push(neighborTile);

                var preDir = HexProximity.getPrecedingDirection(dir);
                var neighbor2Coordinates = HexProximity.GetCoordinates((tile.X, tile.Y), preDir);
                var neighbor2Tile = gs.Tiles.FirstOrDefault(t => t.X == neighbor2Coordinates.Item1 && t.Y == neighbor2Coordinates.Item2);

                // Find corresponding vertex and edges along the tile that go to that vertex (if any)
                var forwardEdge = GetEdgeFromTileInfo(gs.Edges, tile, neighborTile, dir);
                var backEdge = GetEdgeFromTileInfo(gs.Edges, tile, neighbor2Tile, preDir);
                var vertexDir = HexProximity.GetVertexDirectionForEdgeDirection(dir);
                var vertex = GetVertexFromTileInfo(gs.Vertices, tile, neighborTile, neighbor2Tile, vertexDir);

                // Add forward & back edges to vertex (if not added already)
                vertex.AddEdgeReference(forwardEdge);
                vertex.AddEdgeReference(backEdge);

                // Add vertex to forward & back edges (if not added already)
                forwardEdge.AddVertexReference(vertex);
                backEdge.AddVertexReference(vertex);
            }
        }
    }
    
    public static void AddPlayers(GameState gameState)
    {
        var p1 = new Player("Lisa", PlayerColor.Red);
        var p2 = new Player("Hal", PlayerColor.Blue, true);

        gameState.AddPlayer(p1);
        gameState.AddPlayer(p2);
    }

    public static void AddPortsWithStartIndex(GameState gs, List<PortType> randPorts, int startingIndex, int subIndexOverride)
    {
        var rnd = new Random();

        // Ports are not an equal number of vertexes apart. Instead they are placed at one of 18 starting points. Some
        // of the starting points only have one possible orientation while most have two. The following list provides
        // the starting vertex option(s) for each position
        List<List<Vertex>> portStartLocations = new List<List<Vertex>>();
        portStartLocations.Add(new List<Vertex>() { GetVertexFromTileInfo(gs.Vertices, GetTileAt(gs.Tiles, 2, -2), null, null, VertexDirection.N)});
        portStartLocations.Add(new List<Vertex>() { GetVertexFromTileInfo(gs.Vertices, GetTileAt(gs.Tiles, 2, -2), null, null, VertexDirection.NE), 
                GetVertexFromTileInfo(gs.Vertices, GetTileAt(gs.Tiles, 2, -2), GetTileAt(gs.Tiles, 3, -1), null, null)});
        portStartLocations.Add(new List<Vertex>() { GetVertexFromTileInfo(gs.Vertices, GetTileAt(gs.Tiles, 3, -1), null, null, VertexDirection.NE), 
                GetVertexFromTileInfo(gs.Vertices, GetTileAt(gs.Tiles, 3, -1), GetTileAt(gs.Tiles, 4, 0), null, null)});
        portStartLocations.Add(new List<Vertex>() { GetVertexFromTileInfo(gs.Vertices, GetTileAt(gs.Tiles, 4, 0), null, null, VertexDirection.NE)});
        portStartLocations.Add(new List<Vertex>() { GetVertexFromTileInfo(gs.Vertices, GetTileAt(gs.Tiles, 4, 0), null, null, VertexDirection.SE), 
                GetVertexFromTileInfo(gs.Vertices, GetTileAt(gs.Tiles, 4, 0), GetTileAt(gs.Tiles, 3, 1), null, null)});
        portStartLocations.Add(new List<Vertex>() { GetVertexFromTileInfo(gs.Vertices, GetTileAt(gs.Tiles, 3, 1), null, null, VertexDirection.SE), 
                GetVertexFromTileInfo(gs.Vertices, GetTileAt(gs.Tiles, 3, 1), GetTileAt(gs.Tiles, 2, 2), null, null)});
        portStartLocations.Add(new List<Vertex>() { GetVertexFromTileInfo(gs.Vertices, GetTileAt(gs.Tiles, 2, 2), null, null, VertexDirection.SE)});
        portStartLocations.Add(new List<Vertex>() { GetVertexFromTileInfo(gs.Vertices, GetTileAt(gs.Tiles, 2, 2), null, null, VertexDirection.S), 
                GetVertexFromTileInfo(gs.Vertices, GetTileAt(gs.Tiles, 2, 2), GetTileAt(gs.Tiles, 0, 2), null, null)});
        portStartLocations.Add(new List<Vertex>() { GetVertexFromTileInfo(gs.Vertices, GetTileAt(gs.Tiles, 0, 2), null, null, VertexDirection.S), 
                GetVertexFromTileInfo(gs.Vertices, GetTileAt(gs.Tiles, 0, 2), GetTileAt(gs.Tiles, -2, 2), null, null)});
        portStartLocations.Add(new List<Vertex>() { GetVertexFromTileInfo(gs.Vertices, GetTileAt(gs.Tiles, -2, 2), null, null, VertexDirection.S)});
        portStartLocations.Add(new List<Vertex>() { GetVertexFromTileInfo(gs.Vertices, GetTileAt(gs.Tiles, -2, 2), null, null, VertexDirection.SW), 
                GetVertexFromTileInfo(gs.Vertices, GetTileAt(gs.Tiles, -2, 2), GetTileAt(gs.Tiles, -3, 1), null, null)});
        portStartLocations.Add(new List<Vertex>() { GetVertexFromTileInfo(gs.Vertices, GetTileAt(gs.Tiles, -3, 1), null, null, VertexDirection.SW), 
                GetVertexFromTileInfo(gs.Vertices, GetTileAt(gs.Tiles, -3, 1), GetTileAt(gs.Tiles, -4, 0), null, null)});
        portStartLocations.Add(new List<Vertex>() { GetVertexFromTileInfo(gs.Vertices, GetTileAt(gs.Tiles, -4, 0), null, null, VertexDirection.SW)});
        portStartLocations.Add(new List<Vertex>() { GetVertexFromTileInfo(gs.Vertices, GetTileAt(gs.Tiles, -4, 0), null, null, VertexDirection.NW), 
                GetVertexFromTileInfo(gs.Vertices, GetTileAt(gs.Tiles, -4, 0), GetTileAt(gs.Tiles, -3, -1), null, null)});
        portStartLocations.Add(new List<Vertex>() { GetVertexFromTileInfo(gs.Vertices, GetTileAt(gs.Tiles, -3, -1), null, null, VertexDirection.NW), 
                GetVertexFromTileInfo(gs.Vertices, GetTileAt(gs.Tiles, -3, -1), GetTileAt(gs.Tiles, -2, -2), null, null)});
        portStartLocations.Add(new List<Vertex>() { GetVertexFromTileInfo(gs.Vertices, GetTileAt(gs.Tiles, -2, -2), null, null, VertexDirection.NW)});
        portStartLocations.Add(new List<Vertex>() { GetVertexFromTileInfo(gs.Vertices, GetTileAt(gs.Tiles, -2, -2), null, null, VertexDirection.N), 
                GetVertexFromTileInfo(gs.Vertices, GetTileAt(gs.Tiles, -2, -2), GetTileAt(gs.Tiles, 0, -2), null, null)});
        portStartLocations.Add(new List<Vertex>() { GetVertexFromTileInfo(gs.Vertices, GetTileAt(gs.Tiles, -0, -2), null, null, VertexDirection.N), 
                GetVertexFromTileInfo(gs.Vertices, GetTileAt(gs.Tiles, 0, -2), GetTileAt(gs.Tiles, 2, -2), null, null)});

        // TODO: There should be a more elegant/flexible way to do this that will work on random boards.
        // Need something that will move along the edges of the map.

        // We will pick a random index to start the port placement and move 2 positions until all ports are selected.
        // If a position has two options for starting position, we randomly pick one of the options.
        var index = startingIndex;
        var portIndex = 0;
        while (gs.Ports.Count < 9)
        {
            var subIndex = 0;
            if (portStartLocations[index].Count > 1)
                if (subIndexOverride >= 0)
                    subIndex = subIndexOverride;
                else
                    subIndex = rnd.Next(2);

            var startVertex = portStartLocations[index][subIndex];
            var secondVertex = null as Vertex;
            foreach(var edge in startVertex.Edges)
            {
                foreach(var vertex in edge.Vertices)
                {
                    if (vertex.Id == startVertex.Id || vertex.Tiles.Count > 2)
                        continue;

                    // There are three cases we need to deal with:
                    // - Port on edgeTile of board meaning both port vertices are only adjacent to a single tile
                    // - Port is in between two tiles
                    //   - If the port starts on the vertex that intersects two tiles, the second vertex is the one that is only adjacent to a single tile
                    //     and is not in the portStartLocations[index] list
                    //   - If the port starts on the vertex that is only adjacent to one tile, the second is the vertex that is adjacent to two tiles
                    if (startVertex.Direction != null && vertex.Direction != null)
                    {
                        secondVertex = vertex;
                        break;
                    }

                    if (startVertex.Direction == null && vertex.Direction != null && !portStartLocations[index].Contains(vertex))
                    {
                        secondVertex = vertex;
                        break;
                    }

                    if (startVertex.Direction != null && vertex.Direction == null)
                    {
                        secondVertex = vertex;
                        break;
                    }
                }

                if (secondVertex != null)
                    break;
            }

            if (secondVertex == null)
                throw new InvalidOperationException("Unexpected Error. Couldn't find second vertex for port.");

            gs.Ports.Add(new Port(startVertex, secondVertex, randPorts[portIndex]));
            portIndex++;
            index = (index + 2) % 18;
        }
    }

    public static List<PortType> CreateListOfPorts()
    {
        List<PortType> portList = Enum.GetValues(typeof(PortType)).Cast<PortType>().ToList();
        for (int i = 0; i < 3; i++)
            portList.Add(PortType.ThreeToOne);

        return portList;
    }

    public static void AddPorts(GameState gs)
    {
        if (gs.Settings.Type != GameType.Default)
            throw new InvalidOperationException("AddPorts only works with GameType.Default.");

        List<PortType> ports = new List<PortType>()
        {
            PortType.ThreeToOne,
            PortType.ThreeToOne,
            PortType.ThreeToOne,
            PortType.ThreeToOne,
            PortType.Brick,
            PortType.Wood,
            PortType.Ore,
            PortType.Grain,
            PortType.Wool
        };

        // Shuffle the ports
        var rnd = new Random();
        var randPorts = ports.OrderBy(x => rnd.Next()).ToList();

        // Assign ports
        var startIndex = rnd.Next(18);
        AddPortsWithStartIndex(gs, randPorts, startIndex, -1);
    }

    public static void AddPortsForStarter(GameState gs)
    {
        if (gs.Settings.Type != GameType.Starter)
            throw new InvalidOperationException("AddPorts only works with GameType.Default.");

        var ore10Tile = GetTileAt(gs.Tiles, -2, -2);
        var v1 = GetVertexFromTileInfo(gs.Vertices, ore10Tile, null, null, VertexDirection.N);
        var v2 = GetVertexFromTileInfo(gs.Vertices, ore10Tile, null, null, VertexDirection.NW);
        var port = new Port(v1, v2, PortType.ThreeToOne);
        gs.Ports.Add(port);

        var wool2Tile = GetTileAt(gs.Tiles, 0, -2);
        var wood9Tile = GetTileAt(gs.Tiles, 2, -2);
        v1 = GetVertexFromTileInfo(gs.Vertices, wool2Tile, null, null, VertexDirection.N);
        v2 = GetVertexFromTileInfo(gs.Vertices, wool2Tile, wood9Tile, null, null);
        port = new Port(v1, v2, PortType.Grain);
        gs.Ports.Add(port);

        var brick10Tile = GetTileAt(gs.Tiles, 3, -1);
        v1 = GetVertexFromTileInfo(gs.Vertices, brick10Tile, null, null, VertexDirection.NE);
        v2 = GetVertexFromTileInfo(gs.Vertices, brick10Tile, wood9Tile, null, null);
        port = new Port(v1, v2, PortType.Ore);
        gs.Ports.Add(port);

        var ore8Tile = GetTileAt(gs.Tiles, 4, 0);
        v1 = GetVertexFromTileInfo(gs.Vertices, ore8Tile, null, null, VertexDirection.NE);
        v2 = GetVertexFromTileInfo(gs.Vertices, ore8Tile, null, null, VertexDirection.SE);
        port = new Port(v1, v2, PortType.ThreeToOne);
        gs.Ports.Add(port);

        var wool5Tile = GetTileAt(gs.Tiles, 3, 1);
        var wool11Tile = GetTileAt(gs.Tiles, 2, 2);
        v1 = GetVertexFromTileInfo(gs.Vertices, wool5Tile, null, null, VertexDirection.SE);
        v2 = GetVertexFromTileInfo(gs.Vertices, wool5Tile, wool11Tile, null, null);
        port = new Port(v1, v2, PortType.Wool);
        gs.Ports.Add(port);

        var grain6Tile = GetTileAt(gs.Tiles, 0, 2);
        v1 = GetVertexFromTileInfo(gs.Vertices, grain6Tile, null, null, VertexDirection.S);
        v2 = GetVertexFromTileInfo(gs.Vertices, grain6Tile, wool11Tile, null, null);
        port = new Port(v1, v2, PortType.ThreeToOne);
        gs.Ports.Add(port);

        var brick5Tile = GetTileAt(gs.Tiles, -2, 2);
        v1 = GetVertexFromTileInfo(gs.Vertices, brick5Tile, null, null, VertexDirection.S);
        v2 = GetVertexFromTileInfo(gs.Vertices, brick5Tile, null, null, VertexDirection.SW);
        port = new Port(v1, v2, PortType.ThreeToOne);
        gs.Ports.Add(port);

        var wood8Tile = GetTileAt(gs.Tiles, -3, 1);
        var grain9Tile = GetTileAt(gs.Tiles, -4, 0);
        v1 = GetVertexFromTileInfo(gs.Vertices, wood8Tile, null, null, VertexDirection.SW);
        v2 = GetVertexFromTileInfo(gs.Vertices, wood8Tile, grain9Tile, null, null);
        port = new Port(v1, v2, PortType.Brick);
        gs.Ports.Add(port);

        var grain12Tile = GetTileAt(gs.Tiles, -3, -1);
        v1 = GetVertexFromTileInfo(gs.Vertices, grain12Tile, null, null, VertexDirection.NW);
        v2 = GetVertexFromTileInfo(gs.Vertices, grain12Tile, grain9Tile, null, null);
        port = new Port(v1, v2, PortType.Wood);
        gs.Ports.Add(port);
    }
}