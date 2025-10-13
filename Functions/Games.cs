using System.Net;
using System.Text.Json;
using GameTest.DTOs;
using GameTest.Services;
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

    [Function("BuildRoad")]
    public async Task<HttpResponseData> BuildRoad(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "Games/{id}/build/road")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("BuildRoad called for game {GameId}", id);

        if (!Guid.TryParse(id, out var guid))
        {
            var notFound = req.CreateResponse(HttpStatusCode.NotFound);
            await notFound.WriteStringAsync("Invalid game id.");
            return notFound;
        }

        var request = await req.ReadFromJsonAsync<BuildRoadRequest>();
        if (request == null || string.IsNullOrWhiteSpace(request.PlayerId) || string.IsNullOrWhiteSpace(request.EdgeId))
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("Request must include 'playerId' and 'edgeId'.");
            return bad;
        }

        var updated = await _gameService.BuildRoadAsync(guid, request.EdgeId, request.PlayerId);
        if (updated == null)
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("Could not build road (game/player/edge missing or edge occupied).");
            return bad;
        }

        var ok = req.CreateResponse(HttpStatusCode.OK);
        await ok.WriteAsJsonAsync(updated);
        return ok;
    }

    [Function("BuildSettlement")]
    public async Task<HttpResponseData> BuildSettlement(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "Games/{id}/build/settlement")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("BuildSettlement called for game {GameId}", id);

        if (!Guid.TryParse(id, out var guid))
        {
            var notFound = req.CreateResponse(HttpStatusCode.NotFound);
            await notFound.WriteStringAsync("Invalid game id.");
            return notFound;
        }

        var request = await req.ReadFromJsonAsync<BuildOnVertexRequest>();
        if (request == null || string.IsNullOrWhiteSpace(request.PlayerId) || string.IsNullOrWhiteSpace(request.VertexId))
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("Request must include 'playerId' and 'vertexId'.");
            return bad;
        }

        var updated = await _gameService.BuildSettlementAsync(guid, request.VertexId, request.PlayerId);
        if (updated == null)
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("Could not build settlement (game/player/vertex missing or vertex occupied).");
            return bad;
        }

        var ok = req.CreateResponse(HttpStatusCode.OK);
        await ok.WriteAsJsonAsync(updated);
        return ok;
    }

    [Function("BuildCity")]
    public async Task<HttpResponseData> BuildCity(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "Games/{id}/build/city")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("BuildCity called for game {GameId}", id);

        if (!Guid.TryParse(id, out var guid))
        {
            var notFound = req.CreateResponse(HttpStatusCode.NotFound);
            await notFound.WriteStringAsync("Invalid game id.");
            return notFound;
        }

        var request = await req.ReadFromJsonAsync<BuildOnVertexRequest>();
        if (request == null || string.IsNullOrWhiteSpace(request.PlayerId) || string.IsNullOrWhiteSpace(request.VertexId))
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("Request must include 'playerId' and 'vertexId'.");
            return bad;
        }

        var updated = await _gameService.BuildCityAsync(guid, request.VertexId, request.PlayerId);
        if (updated == null)
        {
            var bad = req.CreateResponse(HttpStatusCode.BadRequest);
            await bad.WriteStringAsync("Could not build city (game/player missing or vertex not appropriate for city build).");
            return bad;
        }

        var ok = req.CreateResponse(HttpStatusCode.OK);
        await ok.WriteAsJsonAsync(updated);
        return ok;
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