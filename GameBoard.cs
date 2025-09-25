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

        gameState.AddPlayer(new Player("Alice", PlayerColor.Red));
        gameState.AddPlayer(new Player("Bob", PlayerColor.Blue));

        gameState.AddTile(new Tile(ResourceType.Grain, 10, 0, 0));
        gameState.AddTile(new Tile(ResourceType.Desert, 0, -1, -1));
        gameState.AddTile(new Tile(ResourceType.Wool, 8, 1, -1));
        gameState.AddTile(new Tile(ResourceType.Brick, 5, -1, 0));
        gameState.AddTile(new Tile(ResourceType.Ore, 3, 1, 0));
        gameState.AddTile(new Tile(ResourceType.Wool, 2, -1, 1));
        gameState.AddTile(new Tile(ResourceType.Wood, 6, 1, 1));

        // TODO: Add edges, vertices, ports, etc.

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