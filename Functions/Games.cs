using System.Net;
using System.Text.Json;
using GameTest.DTOs;
using GameTest.Models;
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

    // TODO: Remove once all entry-point returns are modified to use ResponseDTO
    private async Task<HttpResponseData> CreateErrorResponse(HttpRequestData req, HttpStatusCode code, string msg)
    {
        var response = req.CreateResponse(code);
        await response.WriteStringAsync(msg);
        return response;
    }

    private async Task<HttpResponseData> CreateErrorResponse(HttpRequestData req, HttpStatusCode code, int errorCode, string errorMsg)
    {
        var response = req.CreateResponse(code);
        var responseDto = new ResponseDTO(false, errorCode, errorMsg, null);
        await response.WriteAsJsonAsync(responseDto);
        return response;
    }

    private async Task<HttpResponseData> CreateErrorResponse(HttpRequestData req, HttpStatusCode code, ResponseDTO responseDto)
    {
        var response = req.CreateResponse(code);
        await response.WriteAsJsonAsync(responseDto);
        return response;
    }

    private async Task<HttpResponseData> CreateSuccessResponse(HttpRequestData req, ResponseDTO responseDto)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(responseDto);
        return response;
    }


    [Function("Games")]
    public async Task<HttpResponseData> CreateGame(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "Games")] HttpRequestData req)
    {
        var body = await new StreamReader(req.Body).ReadToEndAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var request = JsonSerializer.Deserialize<CreateGameRequest>(body, options);

        if (request == null || string.IsNullOrWhiteSpace(request.GameType))
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Request must include non-empty 'gameType'.");

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
            return await CreateErrorResponse(req, HttpStatusCode.NotFound, "Invalid game id.");

        var request = await req.ReadFromJsonAsync<BuildRoadRequest>();
        if (request == null || string.IsNullOrWhiteSpace(request.PlayerId) || string.IsNullOrWhiteSpace(request.EdgeId))
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Request must include 'playerId' and 'edgeId'.");

        var updated = await _gameService.BuildRoadAsync(guid, request.EdgeId, request.PlayerId);
        if (updated == null)
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Could not build road (game/player/edge missing or edge occupied).");

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
            return await CreateErrorResponse(req, HttpStatusCode.NotFound, "Invalid game id.");

        var request = await req.ReadFromJsonAsync<BuildOnVertexRequest>();
        if (request == null || string.IsNullOrWhiteSpace(request.PlayerId) || string.IsNullOrWhiteSpace(request.VertexId))
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Request must include 'playerId' and 'vertexId'.");

        var updated = await _gameService.BuildSettlementAsync(guid, request.VertexId, request.PlayerId);
        if (updated == null)
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Could not build settlement (game/player/vertex missing or vertex occupied).");

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
            return await CreateErrorResponse(req, HttpStatusCode.NotFound, "Invalid game id.");

        var request = await req.ReadFromJsonAsync<BuildOnVertexRequest>();
        if (request == null || string.IsNullOrWhiteSpace(request.PlayerId) || string.IsNullOrWhiteSpace(request.VertexId))
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Request must include 'playerId' and 'vertexId'.");

        var updated = await _gameService.BuildCityAsync(guid, request.VertexId, request.PlayerId);
        if (updated == null)
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Could not build city (game/player missing or vertex not appropriate for city build).");

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
            return await CreateErrorResponse(req, HttpStatusCode.NotFound, "Invalid game id.");

        var dto = await _gameService.GetGameAsync(guid);
        if (dto == null)
            return await CreateErrorResponse(req, HttpStatusCode.NotFound, $"Game not found (guid:{guid}).");

        var ok = req.CreateResponse(HttpStatusCode.OK);
        await ok.WriteAsJsonAsync(dto);
        return ok;
    }

    [Function("GetGameSummaryById")]
    public async Task<HttpResponseData> GetGameSummaryById(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Games/{id}/Summary")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("GetGameSummaryById called for id {Id}", id);

        if (!Guid.TryParse(id, out var guid))
            return await CreateErrorResponse(req, HttpStatusCode.NotFound, "Invalid game id.");

        var dto = await _gameService.GetGameSummaryAsync(guid);
        if (dto == null)
            return await CreateErrorResponse(req, HttpStatusCode.NotFound, $"Game not found (guid:{guid}).");

        var ok = req.CreateResponse(HttpStatusCode.OK);
        await ok.WriteAsJsonAsync(dto);
        return ok;
    }

    [Function("RollDice")]
    public async Task<HttpResponseData> RollDice(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "Games/{id}/roll")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("RollDice called for game {GameId}", id);

        if (!Guid.TryParse(id, out var guid))
            return await CreateErrorResponse(req, HttpStatusCode.NotFound, "Invalid game id.");

        var diceRolled = await _gameService.RollDiceAsync(guid);
        if (diceRolled == null)
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Could not roll dice.");

        var ok = req.CreateResponse(HttpStatusCode.OK);
        await ok.WriteAsJsonAsync(diceRolled);
        return ok;
    }

    // TODO: Need to get player from authorization. In other entry points I've been talking player
    // as a paramter in the meantime but it is useful to be able to end any players turn for testing.
    // The current player is selected in GameService.EndTurnAsync.
    [Function("EndTurn")]
    public async Task<HttpResponseData> EndTurn(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "Games/{id}/end-turn")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("End-Turn called for game {GameId}", id);

        if (!Guid.TryParse(id, out var guid))
            return await CreateErrorResponse(req, HttpStatusCode.NotFound, 1000, $"GameId: {id}");

        var response = await _gameService.EndTurnAsync(guid);

        if (!response.Success)
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, response);

        return await CreateSuccessResponse(req, response);
    }

    // TODO: Need to get player from authorization. In other entry points I've been talking player
    // as a paramter. Right now I'm just implementing a single human player against a bot. StartGameAsync
    // will just start the game when it is called (regardless of which player is hitting start).
    [Function("StartGame")]
    public async Task<HttpResponseData> StartGame(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "Games/{id}/start")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("Start called for game {GameId}", id);

        if (!Guid.TryParse(id, out var guid))
            return await CreateErrorResponse(req, HttpStatusCode.NotFound, "Invalid game id.");

        var success = await _gameService.StartGameAsync(guid);

        if (!success)
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Unable to start game. See log for details.");

        var ok = req.CreateResponse(HttpStatusCode.OK);
        await ok.WriteAsJsonAsync("Game started.");
        return ok;
    }

    [Function("BankTrade")]
    public async Task<HttpResponseData> BankTrade(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "Games/{id}/trades/bank")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("BankTrade called for game {GameId}", id);

        if (!Guid.TryParse(id, out var guid))
            return await CreateErrorResponse(req, HttpStatusCode.NotFound, "Invalid game id.");

        var request = await req.ReadFromJsonAsync<TradeRequestDTO>();
        if (request == null || string.IsNullOrWhiteSpace(request.PlayerId) || request.Request.Count == 0 || request.Offer.Count == 0)
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Request must include 'playerId', 'request', and 'offer'.");

        var updated = await _gameService.BankTradeAsync(guid, request);
        if (updated == null)
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Could not execute requested bank trade.");

        var ok = req.CreateResponse(HttpStatusCode.OK);
        await ok.WriteAsJsonAsync(updated);
        return ok;
    }

}