using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace GameTest.GameBoard;

public class GameBoard
{
    private readonly ILogger<GameBoard> _logger;

    public GameBoard(ILogger<GameBoard> logger)
    {
        _logger = logger;
    }

    [Function("GameBoard")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        return new OkObjectResult("Hello World2! From GameBoard.");
    }
}