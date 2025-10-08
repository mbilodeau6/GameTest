using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using GameTest.DTOs;
using GameTest.Services;
using GameTest.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace GameTest.Functions;

public class Games
{
    private readonly ILogger<Games> _logger;
    private readonly GameService _gameService;

    public Games(ILogger<Games> logger, GameService gameService)
    {
        _logger = logger;
        _gameService = gameService;
    }

    [Function("Games")]
    public async Task<HttpResponseData> CreateGame(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "Games")] HttpRequestData req)
    {
        var body = await new StreamReader(req.Body).ReadToEndAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var request = JsonSerializer.Deserialize<CreateGameRequest>(body, options);

        if (request == null || string.IsNullOrWhiteSpace(request.GameType))
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("Request must include non-empty 'gameType'.");
            return bad;
        }

        var gameState = _gameService.CreateGame(request.GameType);
        var dto = new GameStateDTO(gameState);

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(dto);
        return response;
    }

    [Function("GetGameById")]
    public async Task<HttpResponseData> GetGameById(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Games/{id}")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("GetGameById called for id {Id}", id);

        if (!Guid.TryParse(id, out var guid))
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("Invalid GUID.");
            return bad;
        }

        var dto = await _gameService.GetGameAsync(guid);
        if (dto == null)
        {
            var notFound = req.CreateResponse(HttpStatusCode.NotFound);
            await notFound.WriteStringAsync($"Game not found (guid:{guid}).");
            return notFound;
        }

        var ok = req.CreateResponse(HttpStatusCode.OK);
        await ok.WriteAsJsonAsync(dto);
        return ok;
    }
}