using GameTest.Models;
using GameTest.Tests;

namespace GameTest.Services;

public static class BoardCreationHelpers
{
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

    public static List<Tile> CreateTilesForPresidio1Board()
    {
        List<Tile> tiles = new List<Tile>
        {
            new Tile(ResourceType.Wood, 4, 0, -2),

            new Tile(ResourceType.Brick, 2, -1, -1),
            new Tile(ResourceType.Brick, 10, 1, -1),

            new Tile(ResourceType.Ore, 3, -4, 0),
            new Tile(ResourceType.Grain, 8, -2, 0),
            new Tile(ResourceType.Brick, 6, 0, 0),
            new Tile(ResourceType.Grain, 11, 2, 0),
            new Tile(ResourceType.Ore, 9, 4, 0),

            new Tile(ResourceType.Wood, 4, -5, 1),
            new Tile(ResourceType.Desert, 4, -3, 1),
            new Tile(ResourceType.Wool, 11, -1, 1),
            new Tile(ResourceType.Wood, 3, 1, 1),
            new Tile(ResourceType.Desert, 4, 3, 1),
            new Tile(ResourceType.Wool, 10, 5, 1),

            new Tile(ResourceType.Ore, 5, -4, 2),
            new Tile(ResourceType.Grain, 3, -2, 2),
            new Tile(ResourceType.Brick, 8, 0, 2),
            new Tile(ResourceType.Grain, 6, 2, 2),
            new Tile(ResourceType.Ore, 11, 4, 2),

            new Tile(ResourceType.Brick, 4, -1, 3),
            new Tile(ResourceType.Brick, 12, 1, 3),

            new Tile(ResourceType.Wool, 10, 0, 4)
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

    public static void CreateEdgesAndVerticesForBoard(GameState gs)
    {
        var stack = new Stack<Tile>();
        stack.Push(gs.GetTileAt(0, 0));

        while (stack.Count > 0)
        {
            var tile = stack.Pop();

            foreach (HexDirection dir in Enum.GetValues(typeof(HexDirection)))
            {
                var neighborCoordinates = HexProximity.GetCoordinates((tile.X, tile.Y), dir);
                var neighborTile = gs.Tiles.FirstOrDefault(t => t.X == neighborCoordinates.Item1 && t.Y == neighborCoordinates.Item2);
                if (neighborTile != null)
                {
                    if (!gs.Edges.Any(e => e.ConnectsTiles(tile.Id, neighborTile.Id)))
                    {
                        var edge = new Edge(tile, neighborTile);
                        gs.AddEdge(edge);
                        stack.Push(neighborTile);
                    }
                }
                else
                {
                    if (!gs.Edges.Any(e => e.Tiles[0].Id == tile.Id && e.Direction == dir))
                    {
                        var edge = new Edge(tile, dir);
                        gs.AddEdge(edge);
                    }
                }

                var neighbor2Coordinates = HexProximity.GetCoordinates((tile.X, tile.Y), HexProximity.getPrecedingDirection(dir));
                var neighbor2Tile = gs.Tiles.FirstOrDefault(t => t.X == neighbor2Coordinates.Item1 && t.Y == neighbor2Coordinates.Item2);
                var vertexDir = HexProximity.GetVertexDirectionForEdgeDirection(dir);

                // If no neighboring tiles, create vertex with just this tile and direction
                if (neighborTile == null && neighbor2Tile == null)
                {
                    if (!gs.Vertices.Any(v => v.Tiles[0].Id == tile.Id && v.Direction == vertexDir))
                    {
                        var vertex = new Vertex(tile, vertexDir);
                        gs.AddVertex(vertex);
                    }
                }
                // Otherwise, create vertex with this tile and any neighboring tiles
                else if (neighborTile != null && neighbor2Tile != null)
                {
                    if (!gs.Vertices.Any(v => v.ConnectsTiles(tile, neighborTile, neighbor2Tile)))
                    {
                        var vertex = new Vertex(tile, neighborTile, neighbor2Tile);
                        gs.AddVertex(vertex);
                    }
                }
                else if (neighborTile != null)
                {
                    if (!gs.Vertices.Any(v => v.ConnectsTiles(tile, neighborTile)))
                    {
                        var vertex = new Vertex(tile, neighborTile);
                        gs.AddVertex(vertex);
                    }
                }
                else if (neighbor2Tile != null)
                {
                    if (!gs.Vertices.Any(v => v.ConnectsTiles(tile, neighbor2Tile)))
                    {
                        var vertex = new Vertex(tile, neighbor2Tile);
                        gs.AddVertex(vertex);
                    }
                }
            }
        }
    }

    public static GameState CreateNewBoard(GameType gameType, string creator)
    {
        var gameState = new GameState(Guid.NewGuid(), creator, gameType);

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
            case GameType.Presidio1:
                foreach (var tile in CreateTilesForPresidio1Board())
                    gameState.AddTile(tile);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(gameType), $"Unhandled game type: {gameType}");
        }

        gameState.PlaceRobberOnDesert();
        CreateEdgesAndVerticesForBoard(gameState);
        LinkEdgesAndVertices(gameState);

        switch(gameType)
        {
            case GameType.Default:
                BoardCreationHelpers.AddPorts(gameState);
                break;
            case GameType.Starter:
                BoardCreationHelpers.AddPortsForStarter(gameState); 
                break;
            case GameType.Presidio1:
                BoardCreationHelpers.AddPortsForPresidio1(gameState);
                break;           
        }

        return gameState;
    }

    public static void LinkEdgesAndVertices(GameState gs)
    {
        var stack = new Stack<Tile>();
        var visitedTiles = new HashSet<string>();

        stack.Push(gs.GetTileAt(0, 0));

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
                var forwardEdge = gs.GetEdgeFromTileInfo(tile, neighborTile, dir);
                var backEdge = gs.GetEdgeFromTileInfo(tile, neighbor2Tile, preDir);
                var vertexDir = HexProximity.GetVertexDirectionForEdgeDirection(dir);
                var vertex = gs.GetVertexFromTileInfo(tile, neighborTile, neighbor2Tile, vertexDir);

                // Add forward & back edges to vertex (if not added already)
                vertex.AddEdgeReference(forwardEdge);
                vertex.AddEdgeReference(backEdge);

                // Add vertex to forward & back edges (if not added already)
                forwardEdge.AddVertexReference(vertex);
                backEdge.AddVertexReference(vertex);
            }
        }
    }
    
    public static void AddPortsWithStartIndex(GameState gs, List<PortType> randPorts, int startingIndex, int subIndexOverride)
    {
        var rnd = new Random();

        // Ports are not an equal number of vertexes apart. Instead they are placed at one of 18 starting points. Some
        // of the starting points only have one possible orientation while most have two. The following list provides
        // the starting vertex option(s) for each position
        List<List<Vertex>> portStartLocations = new List<List<Vertex>>();
        portStartLocations.Add(new List<Vertex>() { gs.GetVertexFromTileInfo(gs.GetTileAt(2, -2), null, null, VertexDirection.N)});
        portStartLocations.Add(new List<Vertex>() { gs.GetVertexFromTileInfo(gs.GetTileAt(2, -2), null, null, VertexDirection.NE), 
                gs.GetVertexFromTileInfo(gs.GetTileAt(2, -2), gs.GetTileAt(3, -1), null, null)});
        portStartLocations.Add(new List<Vertex>() { gs.GetVertexFromTileInfo(gs.GetTileAt(3, -1), null, null, VertexDirection.NE), 
                gs.GetVertexFromTileInfo(gs.GetTileAt(3, -1), gs.GetTileAt(4, 0), null, null)});
        portStartLocations.Add(new List<Vertex>() { gs.GetVertexFromTileInfo(gs.GetTileAt(4, 0), null, null, VertexDirection.NE)});
        portStartLocations.Add(new List<Vertex>() { gs.GetVertexFromTileInfo(gs.GetTileAt(4, 0), null, null, VertexDirection.SE), 
                gs.GetVertexFromTileInfo(gs.GetTileAt(4, 0), gs.GetTileAt(3, 1), null, null)});
        portStartLocations.Add(new List<Vertex>() { gs.GetVertexFromTileInfo(gs.GetTileAt(3, 1), null, null, VertexDirection.SE), 
                gs.GetVertexFromTileInfo(gs.GetTileAt(3, 1), gs.GetTileAt(2, 2), null, null)});
        portStartLocations.Add(new List<Vertex>() { gs.GetVertexFromTileInfo(gs.GetTileAt(2, 2), null, null, VertexDirection.SE)});
        portStartLocations.Add(new List<Vertex>() { gs.GetVertexFromTileInfo(gs.GetTileAt(2, 2), null, null, VertexDirection.S), 
                gs.GetVertexFromTileInfo(gs.GetTileAt(2, 2), gs.GetTileAt(0, 2), null, null)});
        portStartLocations.Add(new List<Vertex>() { gs.GetVertexFromTileInfo(gs.GetTileAt(0, 2), null, null, VertexDirection.S), 
                gs.GetVertexFromTileInfo(gs.GetTileAt(0, 2), gs.GetTileAt(-2, 2), null, null)});
        portStartLocations.Add(new List<Vertex>() { gs.GetVertexFromTileInfo(gs.GetTileAt(-2, 2), null, null, VertexDirection.S)});
        portStartLocations.Add(new List<Vertex>() { gs.GetVertexFromTileInfo(gs.GetTileAt(-2, 2), null, null, VertexDirection.SW), 
                gs.GetVertexFromTileInfo(gs.GetTileAt(-2, 2), gs.GetTileAt(-3, 1), null, null)});
        portStartLocations.Add(new List<Vertex>() { gs.GetVertexFromTileInfo(gs.GetTileAt(-3, 1), null, null, VertexDirection.SW), 
                gs.GetVertexFromTileInfo(gs.GetTileAt(-3, 1), gs.GetTileAt(-4, 0), null, null)});
        portStartLocations.Add(new List<Vertex>() { gs.GetVertexFromTileInfo(gs.GetTileAt(-4, 0), null, null, VertexDirection.SW)});
        portStartLocations.Add(new List<Vertex>() { gs.GetVertexFromTileInfo(gs.GetTileAt(-4, 0), null, null, VertexDirection.NW), 
                gs.GetVertexFromTileInfo(gs.GetTileAt(-4, 0), gs.GetTileAt(-3, -1), null, null)});
        portStartLocations.Add(new List<Vertex>() { gs.GetVertexFromTileInfo(gs.GetTileAt(-3, -1), null, null, VertexDirection.NW), 
                gs.GetVertexFromTileInfo(gs.GetTileAt(-3, -1), gs.GetTileAt(-2, -2), null, null)});
        portStartLocations.Add(new List<Vertex>() { gs.GetVertexFromTileInfo(gs.GetTileAt(-2, -2), null, null, VertexDirection.NW)});
        portStartLocations.Add(new List<Vertex>() { gs.GetVertexFromTileInfo(gs.GetTileAt(-2, -2), null, null, VertexDirection.N), 
                gs.GetVertexFromTileInfo(gs.GetTileAt(-2, -2), gs.GetTileAt(0, -2), null, null)});
        portStartLocations.Add(new List<Vertex>() { gs.GetVertexFromTileInfo(gs.GetTileAt(-0, -2), null, null, VertexDirection.N), 
                gs.GetVertexFromTileInfo(gs.GetTileAt(0, -2), gs.GetTileAt(2, -2), null, null)});

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
            throw new InvalidOperationException("AddPortsForStarter only works with GameType.Starter.");

        var ore10Tile = gs.GetTileAt(-2, -2);
        var v1 = gs.GetVertexFromTileInfo(ore10Tile, null, null, VertexDirection.N);
        var v2 = gs.GetVertexFromTileInfo(ore10Tile, null, null, VertexDirection.NW);
        var port = new Port(v1, v2, PortType.ThreeToOne);
        gs.Ports.Add(port);

        var wool2Tile = gs.GetTileAt(0, -2);
        var wood9Tile = gs.GetTileAt(2, -2);
        v1 = gs.GetVertexFromTileInfo(wool2Tile, null, null, VertexDirection.N);
        v2 = gs.GetVertexFromTileInfo(wool2Tile, wood9Tile, null, null);
        port = new Port(v1, v2, PortType.Grain);
        gs.Ports.Add(port);

        var brick10Tile = gs.GetTileAt(3, -1);
        v1 = gs.GetVertexFromTileInfo(brick10Tile, null, null, VertexDirection.NE);
        v2 = gs.GetVertexFromTileInfo(brick10Tile, wood9Tile, null, null);
        port = new Port(v1, v2, PortType.Ore);
        gs.Ports.Add(port);

        var ore8Tile = gs.GetTileAt(4, 0);
        v1 = gs.GetVertexFromTileInfo(ore8Tile, null, null, VertexDirection.NE);
        v2 = gs.GetVertexFromTileInfo(ore8Tile, null, null, VertexDirection.SE);
        port = new Port(v1, v2, PortType.ThreeToOne);
        gs.Ports.Add(port);

        var wool5Tile = gs.GetTileAt(3, 1);
        var wool11Tile = gs.GetTileAt(2, 2);
        v1 = gs.GetVertexFromTileInfo(wool5Tile, null, null, VertexDirection.SE);
        v2 = gs.GetVertexFromTileInfo(wool5Tile, wool11Tile, null, null);
        port = new Port(v1, v2, PortType.Wool);
        gs.Ports.Add(port);

        var grain6Tile = gs.GetTileAt(0, 2);
        v1 = gs.GetVertexFromTileInfo(grain6Tile, null, null, VertexDirection.S);
        v2 = gs.GetVertexFromTileInfo(grain6Tile, wool11Tile, null, null);
        port = new Port(v1, v2, PortType.ThreeToOne);
        gs.Ports.Add(port);

        var brick5Tile = gs.GetTileAt(-2, 2);
        v1 = gs.GetVertexFromTileInfo(brick5Tile, null, null, VertexDirection.S);
        v2 = gs.GetVertexFromTileInfo(brick5Tile, null, null, VertexDirection.SW);
        port = new Port(v1, v2, PortType.ThreeToOne);
        gs.Ports.Add(port);

        var wood8Tile = gs.GetTileAt(-3, 1);
        var grain9Tile = gs.GetTileAt(-4, 0);
        v1 = gs.GetVertexFromTileInfo(wood8Tile, null, null, VertexDirection.SW);
        v2 = gs.GetVertexFromTileInfo(wood8Tile, grain9Tile, null, null);
        port = new Port(v1, v2, PortType.Brick);
        gs.Ports.Add(port);

        var grain12Tile = gs.GetTileAt(-3, -1);
        v1 = gs.GetVertexFromTileInfo(grain12Tile, null, null, VertexDirection.NW);
        v2 = gs.GetVertexFromTileInfo(grain12Tile, grain9Tile, null, null);
        port = new Port(v1, v2, PortType.Wood);
        gs.Ports.Add(port);
    }

    public static void AddPortsForPresidio1(GameState gs)
    {
        if (gs.Settings.Type != GameType.Presidio1)
            throw new InvalidOperationException("AddPortsForPresidio1 only works with GameType.Presidio1.");

        var woodN = gs.GetTileAt(0, -2);
        var v1 = gs.GetVertexFromTileInfo(woodN, null, null, VertexDirection.NW);
        var v2 = gs.GetVertexFromTileInfo(woodN, null, null, VertexDirection.N);
        var v3 = gs.GetVertexFromTileInfo(woodN, null, null, VertexDirection.NE);
        var port = new Port(v1, v2, PortType.Wood);
        gs.Ports.Add(port);
        port = new Port(v2, v3, PortType.Grain);
        gs.Ports.Add(port);

        var woolS = gs.GetTileAt(0, 4);
        v1 = gs.GetVertexFromTileInfo(woolS, null, null, VertexDirection.SW);
        v2 = gs.GetVertexFromTileInfo(woolS, null, null, VertexDirection.S);
        v3 = gs.GetVertexFromTileInfo(woolS, null, null, VertexDirection.SE);
        port = new Port(v1, v2, PortType.Wool);
        gs.Ports.Add(port);
        port = new Port(v2, v3, PortType.Ore);
        gs.Ports.Add(port);

        var woodW = gs.GetTileAt(-5, 1);
        var oreN = gs.GetTileAt(-4, 0);
        var oreS = gs.GetTileAt(-4, 2);
        v1 = gs.GetVertexFromTileInfo(woodW, oreN, null, null);
        v2 = gs.GetVertexFromTileInfo(woodW, null, null, VertexDirection.NW);
        port = new Port(v1, v2, PortType.Brick);
        gs.Ports.Add(port);

        v1 = gs.GetVertexFromTileInfo(woodW, oreS, null, null);
        v2 = gs.GetVertexFromTileInfo(woodW, null, null, VertexDirection.SW);
        port = new Port(v1, v2, PortType.ThreeToOne);
        gs.Ports.Add(port);

        var woolE = gs.GetTileAt(5, 1);
        oreN = gs.GetTileAt(4, 0);
        oreS = gs.GetTileAt(4, 2);
        v1 = gs.GetVertexFromTileInfo(woolE, oreN, null, null);
        v2 = gs.GetVertexFromTileInfo(woolE, null, null, VertexDirection.NE);
        port = new Port(v1, v2, PortType.ThreeToOne);
        gs.Ports.Add(port);

        v1 = gs.GetVertexFromTileInfo(woolE, oreS, null, null);
        v2 = gs.GetVertexFromTileInfo(woolE, null, null, VertexDirection.SE);
        port = new Port(v1, v2, PortType.Brick);
        gs.Ports.Add(port);
    }
}