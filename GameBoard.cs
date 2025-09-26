using System.Security.Cryptography.Xml;
using GameTest.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GameTest.GameBoard;

public class GameBoard
{
    private readonly ILogger<GameBoard> _logger;

    private static GameState CreateTestGameState()
    {
        GameState gameState = new GameState();

        var p1 = new Player("Alice", PlayerColor.Red);
        gameState.AddPlayer(p1);

        var p2 = new Player("Bob", PlayerColor.Blue);
        gameState.AddPlayer(p2);

        var t1 = new Tile(ResourceType.Grain, 10, 0, 0);
        gameState.AddTile(t1);

        gameState.AddTile(new Tile(ResourceType.Desert, 0, -1, -1));
        gameState.AddTile(new Tile(ResourceType.Wool, 8, 1, -1));

        var t2 = new Tile(ResourceType.Brick, 5, -1, 0);
        gameState.AddTile(t2);

        var t3 = new Tile(ResourceType.Ore, 3, 1, 0);
        gameState.AddTile(t3);

        var t4 = new Tile(ResourceType.Wool, 2, -1, 1);
        gameState.AddTile(t4);

        var t5 = new Tile(ResourceType.Wood, 6, 1, 1);
        gameState.AddTile(t5);

        gameState.AddEdge(new Edge(p1, t2, t4));
        gameState.AddEdge(new Edge(p2, t1, t5));

        gameState.AddVertex(new Vertex(p1, t1, t2, t4));  
        gameState.AddVertex(new Vertex(p2, t1, t3, t5));  

        return gameState;
    }

    public GameBoard(ILogger<GameBoard> logger)
    {
        _logger = logger;
    }

    [Function("GameBoard")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var gameState = CreateTestGameState();

        return new OkObjectResult(gameState);
        // return new OkObjectResult("Hello World3! From GameBoard.");
    }
}