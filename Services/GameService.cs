using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Storage.Blobs;
using GameTest.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using GameTest.DTOs;
using Azure;

namespace GameTest.Services;

public class GameService
{
    private readonly BlobContainerClient? _container;
    private readonly ILogger<GameService> _logger;

    // parameterless ctor kept for tests / direct instantiation
    public GameService() : this(NullLogger<GameService>.Instance) { }

    // preferred ctor for DI so logger is available
    public GameService(ILogger<GameService> logger)
    {
        _logger = logger ?? NullLogger<GameService>.Instance;

        var conn = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(conn))
        {
            _logger.LogInformation("AZURE_STORAGE_CONNECTION_STRING not set; blob storage disabled (local or cloud).");
            _container = null;
            return;
        }

        try
        {
            _logger.LogInformation("Initializing BlobServiceClient using AZURE_STORAGE_CONNECTION_STRING.");
            var service = new BlobServiceClient(conn);
            _container = service.GetBlobContainerClient("gamestates");
            _container.CreateIfNotExists();
            _logger.LogInformation("Blob container 'gamestates' is ready.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize BlobContainerClient. Blob storage will be disabled for this instance.");
            _container = null;
        }
    }

    public GameState CreateGame(string gameTypeString)
    {
        GameType gameType = Enum.Parse<GameType>(gameTypeString, ignoreCase: true);
        var gs = BoardCreationHelpers.CreateNewBoard(gameType);

        // Try to persist a DTO representation to blob storage (best-effort).
        try
        {
            if (_container != null)
            {
                var dto = new DTOs.GameStateDTO(gs);

                var json = JsonSerializer.Serialize(dto, JsonOptions.Default);
                var blob = _container.GetBlobClient($"{gs.Id.ToString()}.json");

                using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
                // synchronous wait on async upload to keep CreateGame signature unchanged
                blob.Upload(ms, overwrite: true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store GameState for GUID {GameId}.", gs.Id);
        }

        // TODO: initialize tiles/players based on gameType

        return gs;
    }

    private async Task<ResponseDTO> GetGameDTO(string id)
    {
        if (_container == null)
        {
            _logger.LogInformation("Blob container not configured; cannot retrieve game {GameId}.", id);
            return new ResponseDTO(false, 1001, $"GameId: {id}", null as GameStateDTO);

        }

        try
        {
            var blob = _container.GetBlobClient($"{id}.json");
            var exists = await blob.ExistsAsync();
            if (!exists.Value)
            {
                _logger.LogInformation("Game blob not found for {GameId}.", id);
                return new ResponseDTO(false, 1002, $"GameId: {id}", null as GameStateDTO);
            }

            var download = await blob.DownloadContentAsync();
            string json = download.Value.Content.ToString();

            var dto = JsonSerializer.Deserialize<DTOs.GameStateDTO>(json, JsonOptions.Default);
            return new ResponseDTO(true, 0, string.Empty, dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve GameState for GUID {GameId}.", id);
            return new ResponseDTO(false, 9999, $"Action: GetGameDTO; GameId: {id}; Exception: {ex.Message}", null as GameStateDTO);
        }
    }

    public async Task<ResponseDTO> GetGameAsync(Guid id)
    {
        var response = await GetGameDTO(id.ToString());

        return response;
    }

    public async Task<string?> GetGameSummaryAsync(Guid id)
    {
        var response = await GetGameDTO(id.ToString());

        if (!response.Success)
            return response.ErrorCode + " - " + response.ErrorMessage;

        var stringBuilder = new StringBuilder();
        
        stringBuilder.Append($"Phase: {response.GameState.Phase.PhaseState} ## Current Player: {response.GameState.Phase.CurrentPlayerId} ## ");
        stringBuilder.Append($"Dice: {response.GameState.Dice.Die1.Value}, {response.GameState.Dice.Die2.Value} ## ");

        // Display current resources each player has
        for (int i = 0; i < response.GameState.Players.Count; i++)
            stringBuilder.Append($"{response.GameState.Players[i].Id}: Wood={response.GameState.Players[i].Resources[ResourceType.Wood]}, Brick={response.GameState.Players[i].Resources[ResourceType.Brick]}, Wool={response.GameState.Players[i].Resources[ResourceType.Wool]}, Grain={response.GameState.Players[i].Resources[ResourceType.Grain]}, Ore={response.GameState.Players[i].Resources[ResourceType.Ore]} ## ");

        if (response.GameState.EventRecord != null && response.GameState.EventRecord.Count > 0)
        {
            int i = response.GameState.EventRecord.Count - 1;
            var er = response.GameState.EventRecord[i];
            
            // Find actions performed by other players
            while (response.GameState.Phase.CurrentPlayerId != null && er.PlayerId != response.GameState.Phase.CurrentPlayerId)
            {
                if (i == 0)
                    break;

                er = response.GameState.EventRecord[--i];
            }

            // Disply actions by other players
            for (; i < response.GameState.EventRecord.Count; i++)
            {
                er = response.GameState.EventRecord[i];

                stringBuilder.Append($"{er.PlayerId} {er.Action} {er.VertexId ?? ""}{er.EdgeId ?? ""}{er.DevelopmentCard.ToString() ?? ""}{er.TileId ?? ""}{er.TargetPlayerId ?? ""}{er.DiceRoll.ToString() ?? "" }");
                if (er.ResourcesUsed != null && er.ResourcesUsed.Count > 0)
                {
                    stringBuilder.Append("{");
                    foreach (var resource in er.ResourcesUsed)
                        stringBuilder.Append($"{resource.Key}:{resource.Value},");
                    stringBuilder.Append("} ");
                }

                if (er.ResourcesReceived != null && er.ResourcesReceived.Count > 0)
                {
                    stringBuilder.Append("{");
                    foreach (var resource in er.ResourcesReceived)
                        stringBuilder.Append($"{resource.Key}:{resource.Value},");
                    stringBuilder.Append("} ");
                }

                stringBuilder.Append("; ");
            }
        }
            
        return stringBuilder.ToString();
    }

    public async Task<ResponseDTO> BuildRoadAsync(Guid gameId, string edgeId, string playerId)
    {
        if (_container == null)
        {
            _logger.LogInformation("Blob container not configured; cannot retrieve game {GameId}.", gameId);
            return new ResponseDTO(false, 1001, $"GameId: {gameId}", null as GameStateDTO);
        }

        try
        {
            var response = await GetGameDTO(gameId.ToString());
            if (!response.Success)
                return new ResponseDTO(false, 1002, $"GameId: {gameId}", null as GameStateDTO);

            var gs = GamePlayHelpers.LoadAndPrepareGameStateDTO(response.GameState);
            var buildResponse = GamePlayHelpers.BuildRoadRequestFromUser(gs, playerId, edgeId);

            if (!buildResponse.Success)
                return buildResponse;

            var json = JsonSerializer.Serialize(buildResponse.GameState, JsonOptions.Default);
            var blob = _container.GetBlobClient($"{gs.Id.ToString()}.json");

            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            // synchronous wait on async upload to keep CreateGame signature unchanged
            blob.Upload(ms, overwrite: true);

            _logger.LogInformation("Built road on edge {EdgeId} for player {PlayerId} in game {GameId}.", edgeId, playerId, gameId);
            return buildResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build road on edge {EdgeId} for game {GameId}.", edgeId, gameId);
            return new ResponseDTO(false, 9999, $"Action: BuildRoad; GameId: {gameId}; Exception: {ex.Message}", null as GameStateDTO);
        }
    }

    public async Task<ResponseDTO> BuildSettlementAsync(Guid gameId, string vertexId, string playerId)
    {
        if (_container == null)
        {
            _logger.LogInformation("Blob container not configured; cannot retrieve game {GameId}.", gameId);
            return new ResponseDTO(false, 1001, $"GameId: {gameId}", null as GameStateDTO);
        }

        try
        {
            var response = await GetGameDTO(gameId.ToString());
            if (!response.Success)
                return new ResponseDTO(false, 1002, $"GameId: {gameId}", null as GameStateDTO);


            var gs = GamePlayHelpers.LoadAndPrepareGameStateDTO(response.GameState);
            var buildResponse = GamePlayHelpers.BuildSettlementRequestFromUser(gs, playerId, vertexId);

            if (!buildResponse.Success)
                return buildResponse;

            var json = JsonSerializer.Serialize(buildResponse.GameState, JsonOptions.Default);
            var blob = _container.GetBlobClient($"{gs.Id.ToString()}.json");

            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            // synchronous wait on async upload to keep CreateGame signature unchanged
            blob.Upload(ms, overwrite: true);

            _logger.LogInformation("Built settlement on vertex {VertexId} for player {PlayerId} in game {GameId}.", vertexId, playerId, gameId);
            return buildResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build settlement on vertex {VertexId} for game {GameId}.", vertexId, gameId);
            return new ResponseDTO(false, 9999, $"Action: BuildSettlement; GameId: {gameId}; Exception: {ex.Message}", null as GameStateDTO);
        }
    }

    public async Task<ResponseDTO> BuildCityAsync(Guid gameId, string vertexId, string playerId)
    {
        if (_container == null)
        {
            _logger.LogInformation("Blob container not configured; cannot retrieve game {GameId}.", gameId);
            return new ResponseDTO(false, 1001, $"GameId: {gameId}", null as GameStateDTO);
        }

        try
        {
            var response = await GetGameDTO(gameId.ToString());
            if (!response.Success)
                return new ResponseDTO(false, 1002, $"GameId: {gameId}", null as GameStateDTO);

            var gs = GamePlayHelpers.LoadAndPrepareGameStateDTO(response.GameState);
            var buildResponse = GamePlayHelpers.UpgradeToCityRequestFromUser(gs, playerId, vertexId);

            if (!buildResponse.Success)
                return buildResponse;

            var json = JsonSerializer.Serialize(buildResponse.GameState, JsonOptions.Default);
            var blob = _container.GetBlobClient($"{gs.Id.ToString()}.json");

            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            // synchronous wait on async upload to keep CreateGame signature unchanged
            blob.Upload(ms, overwrite: true);

            _logger.LogInformation("Built city on vertex {VertexId} for player {PlayerId} in game {GameId}.", vertexId, playerId, gameId);
            return buildResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build city on vertex {VertexId} for game {GameId}.", vertexId, gameId);
            return new ResponseDTO(false, 9999, $"Action: BuildCity; GameId: {gameId}; Exception: {ex.Message}", null as GameStateDTO);
        }
    }

    public async Task<ResponseDTO> RollDiceAsync(Guid gameId)
    {
        if (_container == null)
        {
            _logger.LogInformation("Blob container not configured; cannot retrieve game {GameId}.", gameId);
            return new ResponseDTO(false, 1001, $"GameId: {gameId}", null as GameStateDTO);
        }

        try
        {
            var response = await GetGameDTO(gameId.ToString());
            if (!response.Success)
            {
                _logger.LogError("Unable to retrieve game {GameId}.", gameId);
                return new ResponseDTO(false, 1002, $"GameId: {gameId}", null as GameStateDTO);
            }

            var gs = new GameState(response.GameState);

            if (gs.Phase.CurrentPlayer == null || gs.Phase.PhaseState != GameStates.RollOrUseDevCard)
            {
                _logger.LogError("Game isn't in a state where RollDice is valid.");
                return new ResponseDTO(false, 1003, $"Action: RollDice; GameId: {gameId}; Player: {gs.Phase.CurrentPlayer}; State: {gs.Phase.PhaseState}", null as GameStateDTO);
            }

            GamePlayHelpers.RollDice(gs);

            var updatedDto = new DTOs.GameStateDTO(gs);

            var json = JsonSerializer.Serialize(updatedDto, JsonOptions.Default);
            var blob = _container.GetBlobClient($"{gs.Id.ToString()}.json");

            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            // synchronous wait on async upload to keep CreateGame signature unchanged
            blob.Upload(ms, overwrite: true);

            _logger.LogInformation("Rolled: {die1}, {die2}.", gs.Dice.Die1.Value, gs.Dice.Die2.Value);

            return new ResponseDTO(true, 0, string.Empty, updatedDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to rolle dice for game {GameId}.", gameId);
            return new ResponseDTO(false, 9999, $"Action: RollDice; GameId: {gameId}; Exception: {ex.Message}", null as GameStateDTO);
        }
    }

    public async Task<ResponseDTO> EndTurnAsync(Guid gameId)
    {
        if (_container == null)
        {
            _logger.LogInformation("Blob container not configured; cannot retrieve game {GameId}.", gameId);
            return new ResponseDTO(false, 1001, $"GameId: {gameId}", null as GameStateDTO);
        }

        try
        {
            var response = await GetGameDTO(gameId.ToString());
            if (!response.Success)
            {
                _logger.LogError("Unable to retrieve game {GameId}.", gameId);
                return new ResponseDTO(false, 1002, $"GameId: {gameId}", null as GameStateDTO);
            }

            var gs = GamePlayHelpers.LoadAndPrepareGameStateDTO(response.GameState);

            // TODO: Need to get player from authorization. Using CurrentPlayer for now.
            if (gs.Phase.CurrentPlayer == null || gs.Phase.PhaseState != GameStates.BuildOrTrade)
            {
                _logger.LogError("Game isn't in a state where EndTurn is valid.");
                return new ResponseDTO(false, 1003, $"Action: EndTurn; GameId: {gameId}; Player: {gs.Phase.CurrentPlayer}; State: {gs.Phase.PhaseState}", null as GameStateDTO);
            }
            var player = gs.Phase.CurrentPlayer;
            GamePlayHelpers.EndTurn(player, gs);

            var updatedDto = new DTOs.GameStateDTO(gs);

            var json = JsonSerializer.Serialize(updatedDto, JsonOptions.Default);
            var blob = _container.GetBlobClient($"{gs.Id.ToString()}.json");

            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            // synchronous wait on async upload to keep CreateGame signature unchanged
            blob.Upload(ms, overwrite: true);

            _logger.LogInformation("Ended turn for Player {playerId}.", player);

            return new ResponseDTO(true, 0, string.Empty, gs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to end turn in {GameId}.", gameId);
            return new ResponseDTO(false, 1003, $"Failed to end turn in {gameId}.", null as GameStateDTO);
        }
    }

    public async Task<ResponseDTO> StartGameAsync(Guid gameId)
    {
        if (_container == null)
        {
            _logger.LogInformation("Blob container not configured; cannot retrieve game {GameId}.", gameId);
            return new ResponseDTO(false, 1001, $"GameId: {gameId}", null as GameStateDTO);
        }

        try
        {
            var response = await GetGameDTO(gameId.ToString());
            if (!response.Success)
            {
                _logger.LogError("Unable to retrieve game {GameId}.", gameId);
                return new ResponseDTO(false, 1002, $"GameId: {gameId}", null as GameStateDTO);
            }

            var gs = GamePlayHelpers.LoadAndPrepareGameStateDTO(response.GameState);

            if (gs.Phase.PhaseState != GameStates.SettingUpBoard)
            {
                _logger.LogError("Game isn't in a state where StartGame is valid.");
                return new ResponseDTO(false, 1003, $"Action: StartGame; GameId: {gs.Id}; Player: {gs.Phase.CurrentPlayer}; State: {gs.Phase.PhaseState}", null as GameStateDTO);
            }

            // TODO: In the future, will need every human player to hit start before a game starts.
            // Current version only needs one start call and it starts the game for everyone.
            GamePlayHelpers.StartGame(gs);
            GamePlayHelpers.AssignResourcesBasedOnLastDiceRoll(gs);

            var updatedDto = new DTOs.GameStateDTO(gs);

            var json = JsonSerializer.Serialize(updatedDto, JsonOptions.Default);
            var blob = _container.GetBlobClient($"{gs.Id.ToString()}.json");

            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            // synchronous wait on async upload to keep CreateGame signature unchanged
            blob.Upload(ms, overwrite: true);

            _logger.LogInformation("Started game.");

            return new ResponseDTO(true, 0, string.Empty, updatedDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start game {GameId}.", gameId);
            return new ResponseDTO(false, 9999, $"Action: StartGame; GameId: {gameId}; Exception: {ex.Message}", null as GameStateDTO);
        }
    }

    public async Task<ResponseDTO> BankTradeAsync(Guid gameId, TradeRequestDTO request)
    {
        if (_container == null)
        {
            _logger.LogInformation("Blob container not configured; cannot retrieve game {GameId}.", gameId);
            return new ResponseDTO(false, 1001, $"GameId: {gameId}", null as GameStateDTO);
        }

        try
        {
            var response = await GetGameDTO(gameId.ToString());
            if (!response.Success)
                return new ResponseDTO(false, 1002, $"GameId: {gameId}", null as GameStateDTO);

            var gs = GamePlayHelpers.LoadAndPrepareGameStateDTO(response.GameState);
            var tradeResponse = GamePlayHelpers.BankTradeFromUser(gs, request);

            if (!tradeResponse.Success)
                return tradeResponse;

            var json = JsonSerializer.Serialize(tradeResponse.GameState, JsonOptions.Default);
            var blob = _container.GetBlobClient($"{gs.Id.ToString()}.json");

            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            // synchronous wait on async upload to keep CreateGame signature unchanged
            blob.Upload(ms, overwrite: true);

            _logger.LogInformation("Completed bank trade for player {PlayerId} in game {GameId}.", request.PlayerId, gameId);
            return tradeResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bank trade for player {playerId} for game {GameId} failed.", request.PlayerId, gameId);
            return new ResponseDTO(false, 9999, $"Action: BankTrade; GameId: {gameId}; Exception: {ex.Message}", null as GameStateDTO);
        }
    }

    public async Task<ResponseDTO> OpenTradeAsync(Guid gameId, TradeRequestDTO request)
    {
        if (_container == null)
        {
            _logger.LogInformation("Blob container not configured; cannot retrieve game {GameId}.", gameId);
            return new ResponseDTO(false, 1001, $"GameId: {gameId}", null as GameStateDTO);
        }

        try
        {
            var response = await GetGameDTO(gameId.ToString());
            if (!response.Success)
                return new ResponseDTO(false, 1002, $"GameId: {gameId}", null as GameStateDTO);

            var gs = GamePlayHelpers.LoadAndPrepareGameStateDTO(response.GameState);
            var tradeResponse = GamePlayHelpers.OpenTradeFromUser(gs, request);

            if (!tradeResponse.Success)
                return tradeResponse;

            var json = JsonSerializer.Serialize(tradeResponse.GameState, JsonOptions.Default);
            var blob = _container.GetBlobClient($"{gs.Id.ToString()}.json");

            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            // synchronous wait on async upload to keep CreateGame signature unchanged
            blob.Upload(ms, overwrite: true);

            _logger.LogInformation("Completed open trade for player {PlayerId} in game {GameId}.", request.PlayerId, gameId);
            return tradeResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Open trade for player {playerId} for game {GameId} failed.", request.PlayerId, gameId);
            return new ResponseDTO(false, 9999, $"Action: OpenTrade; GameId: {gameId}; Exception: {ex.Message}", null as GameStateDTO);
        }
    }

    public async Task<ResponseDTO> RespondToTradeAsync(Guid gameId, TradeResponseDTO request)
    {
        if (_container == null)
        {
            _logger.LogInformation("Blob container not configured; cannot retrieve game {GameId}.", gameId);
            return new ResponseDTO(false, 1001, $"GameId: {gameId}", null as GameStateDTO);
        }

        try
        {
            var response = await GetGameDTO(gameId.ToString());
            if (!response.Success)
                return new ResponseDTO(false, 1002, $"GameId: {gameId}", null as GameStateDTO);

            var gs = GamePlayHelpers.LoadAndPrepareGameStateDTO(response.GameState);
            var tradeResponse = GamePlayHelpers.RespondToTradeFromUser(gs, request);

            if (!tradeResponse.Success)
                return tradeResponse;

            var json = JsonSerializer.Serialize(tradeResponse.GameState, JsonOptions.Default);
            var blob = _container.GetBlobClient($"{gs.Id.ToString()}.json");

            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            // synchronous wait on async upload to keep CreateGame signature unchanged
            blob.Upload(ms, overwrite: true);

            _logger.LogInformation("Completed respond to trade for player {PlayerId} in game {GameId}.", request.PlayerId, gameId);
            return tradeResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Respond to trade for player {playerId} for game {GameId} failed.", request.PlayerId, gameId);
            return new ResponseDTO(false, 9999, $"Action: RespondToTrade; GameId: {gameId}; Exception: {ex.Message}", null as GameStateDTO);
        }
    }

public async Task<ResponseDTO> AcceptTradeAsync(Guid gameId, AcceptTradeDTO request)
    {
        if (_container == null)
        {
            _logger.LogInformation("Blob container not configured; cannot retrieve game {GameId}.", gameId);
            return new ResponseDTO(false, 1001, $"GameId: {gameId}", null as GameStateDTO);
        }

        try
        {
            var response = await GetGameDTO(gameId.ToString());
            if (!response.Success)
                return new ResponseDTO(false, 1002, $"GameId: {gameId}", null as GameStateDTO);

            var gs = GamePlayHelpers.LoadAndPrepareGameStateDTO(response.GameState);
            var tradeResponse = GamePlayHelpers.AcceptTradeFromUser(gs, request);

            if (!tradeResponse.Success)
                return tradeResponse;

            var json = JsonSerializer.Serialize(tradeResponse.GameState, JsonOptions.Default);
            var blob = _container.GetBlobClient($"{gs.Id.ToString()}.json");

            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            // synchronous wait on async upload to keep CreateGame signature unchanged
            blob.Upload(ms, overwrite: true);

            _logger.LogInformation("Completed accept trade for player {PlayerId} in game {GameId}.", request.PlayerId, gameId);
            return tradeResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Accept trade for player {playerId} for game {GameId} failed.", request.PlayerId, gameId);
            return new ResponseDTO(false, 9999, $"Action: AcceptTrade; GameId: {gameId}; Exception: {ex.Message}", null as GameStateDTO);
        }
    }

    public async Task<ResponseDTO> RejectAllOffersAsync(Guid gameId, BaseRequest request)
    {
        if (_container == null)
        {
            _logger.LogInformation("Blob container not configured; cannot retrieve game {GameId}.", gameId);
            return new ResponseDTO(false, 1001, $"GameId: {gameId}", null as GameStateDTO);
        }

        try
        {
            var response = await GetGameDTO(gameId.ToString());
            if (!response.Success)
                return new ResponseDTO(false, 1002, $"GameId: {gameId}", null as GameStateDTO);

            var gs = GamePlayHelpers.LoadAndPrepareGameStateDTO(response.GameState);
            var tradeResponse = GamePlayHelpers.RejectAllOffersFromUser(gs, request);

            if (!tradeResponse.Success)
                return tradeResponse;

            var json = JsonSerializer.Serialize(tradeResponse.GameState, JsonOptions.Default);
            var blob = _container.GetBlobClient($"{gs.Id.ToString()}.json");

            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            // synchronous wait on async upload to keep CreateGame signature unchanged
            blob.Upload(ms, overwrite: true);

            _logger.LogInformation("Completed reject all offers for player {PlayerId} in game {GameId}.", request.PlayerId, gameId);
            return tradeResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Reject all offers for player {playerId} for game {GameId} failed.", request.PlayerId, gameId);
            return new ResponseDTO(false, 9999, $"Action: RejectAllOffers; GameId: {gameId}; Exception: {ex.Message}", null as GameStateDTO);
        }
    }

    public async Task<ResponseDTO> PlaceRobberAsync(Guid gameId, PlaceOnTileRequest request)
    {
        if (_container == null)
        {
            _logger.LogInformation("Blob container not configured; cannot retrieve game {GameId}.", gameId);
            return new ResponseDTO(false, 1001, $"GameId: {gameId}", null as GameStateDTO);
        }

        try
        {
            var response = await GetGameDTO(gameId.ToString());
            if (!response.Success)
                return new ResponseDTO(false, 1002, $"GameId: {gameId}", null as GameStateDTO);

            var gs = GamePlayHelpers.LoadAndPrepareGameStateDTO(response.GameState);
            var tradeResponse = GamePlayHelpers.PlaceRobberForUser(gs, request.PlayerId, request.TileId);

            if (!tradeResponse.Success)
                return tradeResponse;

            var json = JsonSerializer.Serialize(tradeResponse.GameState, JsonOptions.Default);
            var blob = _container.GetBlobClient($"{gs.Id.ToString()}.json");

            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            // synchronous wait on async upload to keep CreateGame signature unchanged
            blob.Upload(ms, overwrite: true);

            _logger.LogInformation("Completed place robber for player {PlayerId} to tile {TileId} in game {GameId}.", request.PlayerId, request.TileId, gameId);
            return tradeResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Place robber for player {playerId} to tile {tileId} for game {GameId} failed.", request.PlayerId, request.TileId, gameId);
            return new ResponseDTO(false, 9999, $"Action: PlaceRobber; GameId: {gameId}; TileId: {request.TileId}; Exception: {ex.Message}", null as GameStateDTO);
        }
    }

    public async Task<ResponseDTO> BuyDevCardAsync(Guid gameId, string playerId)
    {
        if (_container == null)
        {
            _logger.LogInformation("Blob container not configured; cannot retrieve game {GameId}.", gameId);
            return new ResponseDTO(false, 1001, $"GameId: {gameId}", null as GameStateDTO);
        }

        try
        {
            var response = await GetGameDTO(gameId.ToString());
            if (!response.Success)
                return new ResponseDTO(false, 1002, $"GameId: {gameId}", null as GameStateDTO);

            var gs = GamePlayHelpers.LoadAndPrepareGameStateDTO(response.GameState);
            var tradeResponse = GamePlayHelpers.BuyDevCardFromUser(gs, playerId);

            if (!tradeResponse.Success)
                return tradeResponse;

            var json = JsonSerializer.Serialize(tradeResponse.GameState, JsonOptions.Default);
            var blob = _container.GetBlobClient($"{gs.Id.ToString()}.json");

            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            // synchronous wait on async upload to keep CreateGame signature unchanged
            blob.Upload(ms, overwrite: true);

            _logger.LogInformation("Completed Buy Development Card for player {PlayerId} in game {GameId}.", playerId, gameId);
            return tradeResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Buy Development Card for player {playerId} for game {GameId} failed.", playerId, gameId);
            return new ResponseDTO(false, 9999, $"Action: BuyDevCard; GameId: {gameId}; PlayerId: {playerId}; Exception: {ex.Message}", null as GameStateDTO);
        }
    }

    public async Task<ResponseDTO> PlayDevCardAsync(Guid gameId, PlayDevCardRequest request)
    {
        if (_container == null)
        {
            _logger.LogInformation("Blob container not configured; cannot retrieve game {GameId}.", gameId);
            return new ResponseDTO(false, 1001, $"GameId: {gameId}", null as GameStateDTO);
        }

        try
        {
            var response = await GetGameDTO(gameId.ToString());
            if (!response.Success)
                return new ResponseDTO(false, 1002, $"GameId: {gameId}", null as GameStateDTO);

            var gs = GamePlayHelpers.LoadAndPrepareGameStateDTO(response.GameState);

            response = null; //  = GamePlayHelpers.BuyDevCard(gs, playerId);
            switch(request.DevCardType)
            {
                case DevelopmentCardType.Monopoly:
                    response = GamePlayHelpers.PlayMonopolyDevCardFromUser(gs, request);
                    break;
                case DevelopmentCardType.YearOfPlenty:
                    response = GamePlayHelpers.PlayYearOfPlentyDevCardFromUser(gs, request);
                    break;
                case DevelopmentCardType.Knight:
                    response = GamePlayHelpers.PlayKnightDevCardFromUser(gs, request);
                    break;
                case DevelopmentCardType.RoadBuilding:
                    response = GamePlayHelpers.PlayRoadBuildingDevCardFromUser(gs, request);
                    break;
                default: 
                    return new ResponseDTO(false, 1036, $"GameId: {gameId}; PlayerId: {request.PlayerId}; DevCardType: {request.DevCardType}", null as GameStateDTO);
            }

            if (response == null)
            {
                return new ResponseDTO(false, 9999, $"Action: PlayDevCard; GameId: {gameId}; PlayerId: {request.PlayerId}; DevCardType: {request.DevCardType}", null as GameStateDTO);
            }

            if (!response.Success)
                return response;

            var json = JsonSerializer.Serialize(response.GameState, JsonOptions.Default);
            var blob = _container.GetBlobClient($"{gs.Id.ToString()}.json");

            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            // synchronous wait on async upload to keep CreateGame signature unchanged
            blob.Upload(ms, overwrite: true);

            _logger.LogInformation("Completed Play Development Card {DevCardType} for player {PlayerId} in game {GameId}.", request.DevCardType, request.PlayerId, gameId);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Play Development Card {DevCardType} for player {playerId} for game {GameId} failed.", request.DevCardType, request.PlayerId, gameId);
            return new ResponseDTO(false, 9999, $"Action: PlayDevCard; GameId: {gameId}; PlayerId: {request.PlayerId}; DevCardType: {request.DevCardType}; Exception: {ex.Message}", null as GameStateDTO);
        }
    }

    public async Task<ResponseDTO> DiscardCardsAsync(Guid gameId, DiscardRequest request)
    {
        if (_container == null)
        {
            _logger.LogInformation("Blob container not configured; cannot retrieve game {GameId}.", gameId);
            return new ResponseDTO(false, 1001, $"GameId: {gameId}", null as GameStateDTO);
        }

        try
        {
            var response = await GetGameDTO(gameId.ToString());
            if (!response.Success)
                return new ResponseDTO(false, 1002, $"GameId: {gameId}", null as GameStateDTO);

            var gs = GamePlayHelpers.LoadAndPrepareGameStateDTO(response.GameState);
            response = GamePlayHelpers.DiscardCardRequestFromUser(gs, request);

            if (response == null)
            {
                return new ResponseDTO(false, 9999, $"Action: DiscardCards; GameId: {gameId}; PlayerId: {request.PlayerId}", null as GameStateDTO);
            }

            if (!response.Success)
                return response;

            var json = JsonSerializer.Serialize(response.GameState, JsonOptions.Default);
            var blob = _container.GetBlobClient($"{gs.Id.ToString()}.json");

            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            // synchronous wait on async upload to keep CreateGame signature unchanged
            blob.Upload(ms, overwrite: true);

            _logger.LogInformation("Completed Discard Cards for player {PlayerId} in game {GameId}.", request.PlayerId, gameId);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Discard Cards for player {playerId} for game {GameId} failed.", request.PlayerId, gameId);
            return new ResponseDTO(false, 9999, $"Action: DiscardCards; GameId: {gameId}; PlayerId: {request.PlayerId}; Exception: {ex.Message}", null as GameStateDTO);
        }
    }

}