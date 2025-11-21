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

    private async Task<HttpResponseData> CreateErrorResponse(HttpRequestData req, HttpStatusCode code, int errorCode, string errorMsg)
    {
        var response = req.CreateResponse(code);
        var responseDto = new ResponseDTO(false, errorCode, errorMsg, null as GameStateDTO);
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

    private async Task<HttpResponseData> CreateSuccessResponse(HttpRequestData req, GameState gs)
    {
        var responseDto = new ResponseDTO(true, 0, string.Empty, gs);
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
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, 1004, string.Empty);

        var gameState = _gameService.CreateGame(request.GameType);

        return await CreateSuccessResponse(req, gameState);
    }

    [Function("BuildRoad")]
    public async Task<HttpResponseData> BuildRoad(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "Games/{id}/build/road")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("BuildRoad called for game {GameId}", id);

        if (!Guid.TryParse(id, out var guid))
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, 1000, $"GameId: {id}");

        var request = await req.ReadFromJsonAsync<BuildRoadRequest>();
        if (request == null || string.IsNullOrWhiteSpace(request.PlayerId) || string.IsNullOrWhiteSpace(request.EdgeId))
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, 1005, $"GameId: {id}");

        var response = await _gameService.BuildRoadAsync(guid, request.EdgeId, request.PlayerId);
        if (!response.Success)
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, response);

        return await CreateSuccessResponse(req, response);
    }

    [Function("BuildSettlement")]
    public async Task<HttpResponseData> BuildSettlement(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "Games/{id}/build/settlement")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("BuildSettlement called for game {GameId}", id);

        if (!Guid.TryParse(id, out var guid))
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, 1000, $"GameId: {id}");

        var request = await req.ReadFromJsonAsync<BuildOnVertexRequest>();
        if (request == null || string.IsNullOrWhiteSpace(request.PlayerId) || string.IsNullOrWhiteSpace(request.VertexId))
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, 1024, $"GameId: {id}");

        var response = await _gameService.BuildSettlementAsync(guid, request.VertexId, request.PlayerId);
        if (!response.Success)
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, response);

        return await CreateSuccessResponse(req, response);
    }

    [Function("BuildCity")]
    public async Task<HttpResponseData> BuildCity(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "Games/{id}/build/city")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("BuildCity called for game {GameId}", id);

        if (!Guid.TryParse(id, out var guid))
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, 1000, $"GameId: {id}");

        var request = await req.ReadFromJsonAsync<BuildOnVertexRequest>();
        if (request == null || string.IsNullOrWhiteSpace(request.PlayerId) || string.IsNullOrWhiteSpace(request.VertexId))
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, 1025, $"GameId: {id}");

        var response = await _gameService.BuildCityAsync(guid, request.VertexId, request.PlayerId);
        if (!response.Success)
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, response);

        return await CreateSuccessResponse(req, response);
    }

    [Function("GetGameById")]
    public async Task<HttpResponseData> GetGameById(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Games/{id}")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("GetGameById called for id {Id}", id);

        if (!Guid.TryParse(id, out var guid))
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, 1000, $"GameId: {id}");

        var dto = await _gameService.GetGameAsync(guid);
        if (dto == null)
            return await CreateErrorResponse(req, HttpStatusCode.NotFound, 1002, $"GameId: {id}");

        return await CreateSuccessResponse(req, dto);
    }

    [Function("GetGameSummaryById")]
    public async Task<HttpResponseData> GetGameSummaryById(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "Games/{id}/Summary")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("GetGameSummaryById called for id {Id}", id);

        if (!Guid.TryParse(id, out var guid))
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, 1000, $"GameId: {id}");

        var summary = await _gameService.GetGameSummaryAsync(guid);
        if (summary == null)
            return await CreateErrorResponse(req, HttpStatusCode.NotFound, 1002, $"GameId: {id}");

        var ok = req.CreateResponse(HttpStatusCode.OK);
        await ok.WriteAsJsonAsync(summary);
        return ok;
    }

    [Function("RollDice")]
    public async Task<HttpResponseData> RollDice(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "Games/{id}/roll")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("RollDice called for game {GameId}", id);

        if (!Guid.TryParse(id, out var guid))
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, 1000, $"GameId: {id}");

        var response = await _gameService.RollDiceAsync(guid);
        if (!response.Success)
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, response);

        return await CreateSuccessResponse(req, response);
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
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, 1000, $"GameId: {id}");

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
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, 1000, $"GameId: {id}");

        var response = await _gameService.StartGameAsync(guid);

        if (!response.Success)
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, response);

        return await CreateSuccessResponse(req, response);
    }

    [Function("BankTrade")]
    public async Task<HttpResponseData> BankTrade(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "Games/{id}/trades/bank")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("BankTrade called for game {GameId}", id);

        if (!Guid.TryParse(id, out var guid))
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, 1000, $"GameId: {id}");

        var request = await req.ReadFromJsonAsync<TradeRequestDTO>();
        if (request == null || string.IsNullOrWhiteSpace(request.PlayerId) || request.Request.Count == 0 || request.Offer.Count == 0)
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, 1026, $"GameId: {id}");

        var response = await _gameService.BankTradeAsync(guid, request);
        if (!response.Success)
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, response);

        return await CreateSuccessResponse(req, response);
    }

}