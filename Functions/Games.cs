using System.Net;
using System.Text.Json;
using GameTest.DTOs;
using GameTest.Models;
using GameTest.Services;
using Google.Protobuf.Reflection;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
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
        var json = JsonSerializer.Serialize(responseDto, JsonOptions.Default);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        await response.WriteStringAsync(json);
        return response;
    }

    private async Task<HttpResponseData> CreateErrorResponse(HttpRequestData req, HttpStatusCode code, ResponseDTO responseDto)
    {
        var response = req.CreateResponse(code);
        var json = JsonSerializer.Serialize(responseDto, JsonOptions.Default);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        await response.WriteStringAsync(json);
        return response;
    }

    private async Task<HttpResponseData> CreateSuccessResponse(HttpRequestData req, ResponseDTO responseDto)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        var json = JsonSerializer.Serialize(responseDto, JsonOptions.Default);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        await response.WriteStringAsync(json);
        return response;
    }

    private async Task<HttpResponseData> CreateSuccessResponse(HttpRequestData req, GameState gs)
    {
        var responseDto = new ResponseDTO(true, 0, string.Empty, gs);
        var response = req.CreateResponse(HttpStatusCode.OK);
        var json = JsonSerializer.Serialize(responseDto, JsonOptions.Default);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        await response.WriteStringAsync(json);
        return response;
    }

    private static async Task<T?> ReadRequestBodyAsync<T>(HttpRequestData req)
    {
        var body = await new StreamReader(req.Body).ReadToEndAsync();
        return JsonSerializer.Deserialize<T>(body, JsonOptions.Default);
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

        var request = await ReadRequestBodyAsync<BuildRoadRequest>(req);
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

        var request = await ReadRequestBodyAsync<BuildOnVertexRequest>(req);
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

        var request = await ReadRequestBodyAsync<BuildOnVertexRequest>(req);
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
        var json = JsonSerializer.Serialize(summary, JsonOptions.Default);
        ok.Headers.Add("Content-Type", "application/json; charset=utf-8");
        await ok.WriteStringAsync(json);
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

    // TODO: Review whether other players need to have a say on starting the game. Right now I'm allowing
    // any player to do it.
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

        var request = await ReadRequestBodyAsync<TradeRequestDTO>(req);
        if (request == null || string.IsNullOrWhiteSpace(request.PlayerId) || request.Request.Count == 0 || request.Offer.Count == 0)
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, 1026, $"GameId: {id}");

        var response = await _gameService.BankTradeAsync(guid, request);
        if (!response.Success)
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, response);

        return await CreateSuccessResponse(req, response);
    }

    [Function("OpenTrade")]
    public async Task<HttpResponseData>OpenTrade(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "Games/{id}/trades/open")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("OpenTrade called for game {GameId}", id);

        if (!Guid.TryParse(id, out var guid))
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, 1000, $"GameId: {id}");

        var request = await ReadRequestBodyAsync<TradeRequestDTO>(req);
        if (request == null || string.IsNullOrWhiteSpace(request.PlayerId) || request.Request.Count == 0 || request.Offer.Count == 0)
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, 1026, $"GameId: {id}");

        var response = await _gameService.OpenTradeAsync(guid, request);
        if (!response.Success)
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, response);

        return await CreateSuccessResponse(req, response);
    }

    [Function("TradeResponse")]
    public async Task<HttpResponseData>TradeResponse(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "Games/{id}/trades/respond")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("TradeResponse called for game {GameId}", id);

        if (!Guid.TryParse(id, out var guid))
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, 1000, $"GameId: {id}");

        var request = await ReadRequestBodyAsync<TradeResponseDTO>(req);
        if (request == null || string.IsNullOrWhiteSpace(request.PlayerId) || request.ResponseType == TradeResponseType.Original || 
            (request.ResponseType == TradeResponseType.Counter && 
                (request.Request == null || request.Offer == null || request.Request.Count == 0 || request.Offer.Count == 0)))
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, 1046, $"GameId: {id}");

        var response = await _gameService.RespondToTradeAsync(guid, request);
        if (!response.Success)
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, response);

        return await CreateSuccessResponse(req, response);
    }

    [Function("AcceptTrade")]
    public async Task<HttpResponseData>AcceptTrade(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "Games/{id}/trades/accept")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("AcceptTrade called for game {GameId}", id);

        if (!Guid.TryParse(id, out var guid))
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, 1000, $"GameId: {id}");

        var request = await ReadRequestBodyAsync<AcceptTradeDTO>(req);
        if (request == null || string.IsNullOrWhiteSpace(request.PlayerId) || string.IsNullOrWhiteSpace(request.AcceptedPlayerId))
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, 1047, $"GameId: {id}");

        var response = await _gameService.AcceptTradeAsync(guid, request);
        if (!response.Success)
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, response);

        return await CreateSuccessResponse(req, response);
    }

    [Function("RejectAllOffers")]
    public async Task<HttpResponseData>RejectAllOffers(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "Games/{id}/trades/reject-all")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("RejectAllOffers called for game {GameId}", id);

        if (!Guid.TryParse(id, out var guid))
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, 1000, $"GameId: {id}");

        var request = await ReadRequestBodyAsync<BaseRequest>(req);
        if (request == null || string.IsNullOrWhiteSpace(request.PlayerId))
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, 1048, $"GameId: {id}");

        var response = await _gameService.RejectAllOffersAsync(guid, request);
        if (!response.Success)
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, response);

        return await CreateSuccessResponse(req, response);
    }


    [Function("PlaceRobber")]
    public async Task<HttpResponseData> PlaceRobber(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "Games/{id}/place-robber")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("Place-Robber called for game {GameId}", id);

        if (!Guid.TryParse(id, out var guid))
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, 1000, $"GameId: {id}");

        var request = await ReadRequestBodyAsync<PlaceOnTileRequest>(req);
        if (request == null || string.IsNullOrWhiteSpace(request.PlayerId) || string.IsNullOrWhiteSpace(request.TileId))
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, 1031, $"GameId: {id}");

        var response = await _gameService.PlaceRobberAsync(guid, request);
        if (!response.Success)
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, response);

        return await CreateSuccessResponse(req, response);
    }

    [Function("BuyDevCard")]
    public async Task<HttpResponseData> BuyDevCard(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "Games/{id}/dev-card/buy")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("Dev-Card/Buy called for game {GameId}", id);

        if (!Guid.TryParse(id, out var guid))
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, 1000, $"GameId: {id}");

        var request = await ReadRequestBodyAsync<BaseRequest>(req);
        if (request == null || string.IsNullOrWhiteSpace(request.PlayerId))
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, 1034, $"GameId: {id}");

        var response = await _gameService.BuyDevCardAsync(guid, request.PlayerId);
        if (!response.Success)
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, response);

        return await CreateSuccessResponse(req, response);
    }

    [Function("PlayDevCard")]
    public async Task<HttpResponseData> PlayDevCard(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "Games/{id}/dev-card/play")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("Dev-Card/Play called for game {GameId}", id);

        if (!Guid.TryParse(id, out var guid))
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, 1000, $"GameId: {id}");

        var request = await ReadRequestBodyAsync<PlayDevCardRequest>(req);
        if (request == null || string.IsNullOrWhiteSpace(request.PlayerId))
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, 1035, $"GameId: {id}");

        var response = await _gameService.PlayDevCardAsync(guid, request);
        if (!response.Success)
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, response);

        return await CreateSuccessResponse(req, response);
    }

    [Function("DiscardCards")]
    public async Task<HttpResponseData> DiscardCards(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "Games/{id}/discard-cards")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("Dev-Card/Play called for game {GameId}", id);

        if (!Guid.TryParse(id, out var guid))
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, 1000, $"GameId: {id}");

        var request = await ReadRequestBodyAsync<DiscardRequest>(req);
        if (request == null || string.IsNullOrWhiteSpace(request.PlayerId) || request.SelectedResources == null || request.SelectedResources.Count() == 0)
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, 1044, $"GameId: {id}");

        var response = await _gameService.DiscardCardsAsync(guid, request);
        if (!response.Success)
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, response);

        return await CreateSuccessResponse(req, response);
    }

    [Function("AddPlayer")]
    public async Task<HttpResponseData> AddPlayer(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "Games/{id}/players")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("AddPlayer called for game {GameId}", id);

        if (!Guid.TryParse(id, out var guid))
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, 1000, $"GameId: {id}");

        var request = await ReadRequestBodyAsync<AddPlayerRequest>(req);
        if (request == null)
            request = new AddPlayerRequest();

        var response = await _gameService.AddPlayerAsync(guid, request);
        if (!response.Success)
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, response);

        return await CreateSuccessResponse(req, response);
    }

    [Function("SelectTarget")]
    public async Task<HttpResponseData> SelectTarget(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "Games/{id}/select-target")] HttpRequestData req,
        string id)
    {
        _logger.LogInformation("SelectTarget called for game {GameId}", id);

        if (!Guid.TryParse(id, out var guid))
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, 1000, $"GameId: {id}");

        var request = await ReadRequestBodyAsync<SelectTargetRequest>(req);
        if (request == null || string.IsNullOrWhiteSpace(request.PlayerId) || string.IsNullOrWhiteSpace(request.TargetPlayerId))
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, 1065, $"GameId: {id}");

        var response = await _gameService.SelectTargetAsync(guid, request);
        if (!response.Success)
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, response);

        return await CreateSuccessResponse(req, response);
    }

}