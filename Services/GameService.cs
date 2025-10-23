using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Nodes;
using Azure;
using Azure.Storage.Blobs;
using GameTest.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using GameTest.DTOs;

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

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Converters = { new JsonStringEnumConverter() }
                };

                var json = JsonSerializer.Serialize(dto, options);
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

    private async Task<DTOs.GameStateDTO?> GetGameDTO(string id)
    {
        if (_container == null)
        {
            _logger.LogInformation("Blob container not configured; cannot retrieve game {GameId}.", id);
            return null;
        }

        try
        {
            var blob = _container.GetBlobClient($"{id}.json");
            var exists = await blob.ExistsAsync();
            if (!exists.Value)
            {
                _logger.LogInformation("Game blob not found for {GameId}.", id);
                return null;
            }

            var download = await blob.DownloadContentAsync();
            string json = download.Value.Content.ToString();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters = { new JsonStringEnumConverter() }
            };

            var dto = JsonSerializer.Deserialize<DTOs.GameStateDTO>(json, options);
            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve GameState for GUID {GameId}.", id);
            return null;
        }
    }

    public async Task<DTOs.GameStateDTO?> GetGameAsync(Guid id)
    {
        GameStateDTO? fullDTO = await GetGameDTO(id.ToString());

        return fullDTO;
    }

    public async Task<string?> BuildRoadAsync(Guid gameId, string edgeId, string playerId)
    {
        if (_container == null)
        {
            _logger.LogInformation("Blob container not configured; cannot retrieve game {GameId}.", gameId);
            return null;
        }

        try
        {
            var dto = await GetGameDTO(gameId.ToString());
            if (dto == null)
                return $"Unable to retrieve game {gameId}";

            var gs = new GameState(dto);
            var player = gs.Players.FirstOrDefault(p => p.Id == playerId);
            if (player == null)
                return $"Player {playerId} not found in game {gameId}";

            var edge = gs.Edges.FirstOrDefault(e => e.Id == edgeId);
            if (edge == null)
                return $"Edge {edgeId} not found in game {gameId}";

            if (edge.Owner != null)
                return $"Edge {edgeId} in game {gameId} already has a road.";

            edge.BuildRoad(player);

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() }
            };

            var updatedDto = new DTOs.GameStateDTO(gs);

            var json = JsonSerializer.Serialize(updatedDto, options);
            var blob = _container.GetBlobClient($"{gs.Id.ToString()}.json");

            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            // synchronous wait on async upload to keep CreateGame signature unchanged
            blob.Upload(ms, overwrite: true);

            _logger.LogInformation("Built road on edge {EdgeId} for player {PlayerId} in game {GameId}.", edgeId, playerId, gameId);
            return $"Built road on edge {edgeId} for player {playerId} in game {gameId}.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build road on edge {EdgeId} for game {GameId}.", edgeId, gameId);
            return null;
        }
    }

    public async Task<string?> BuildSettlementAsync(Guid gameId, string vertexId, string playerId)
    {
        if (_container == null)
        {
            _logger.LogInformation("Blob container not configured; cannot retrieve game {GameId}.", gameId);
            return null;
        }

        try
        {
            var dto = await GetGameDTO(gameId.ToString());
            if (dto == null)
                return $"Unable to retrieve game {gameId}";

            var gs = new GameState(dto);
            var player = gs.Players.FirstOrDefault(p => p.Id == playerId);
            if (player == null)
                return $"Player {playerId} not found in game {gameId}";

            var vertex = gs.Vertices.FirstOrDefault(v => v.Id == vertexId);
            if (vertex == null)
                return $"Vertex {vertexId} not found in game {gameId}";

            if (vertex.Building == BuildingType.Settlement)
                return $"Vertex {vertexId} in game {gameId} already has a settlement.";

            if (vertex.Building == BuildingType.City)
                return $"Vertex {vertexId} in game {gameId} already has a city.";

            vertex.BuildSettlement(player);

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() }
            };

            var updatedDto = new DTOs.GameStateDTO(gs);

            var json = JsonSerializer.Serialize(updatedDto, options);
            var blob = _container.GetBlobClient($"{gs.Id.ToString()}.json");

            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            // synchronous wait on async upload to keep CreateGame signature unchanged
            blob.Upload(ms, overwrite: true);

            _logger.LogInformation("Built settlement on vertex {VertexId} for player {PlayerId} in game {GameId}.", vertexId, playerId, gameId);
            return $"Built settlement on vertex {vertexId} for player {playerId} in game {gameId}.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build settlement on vertex {VertexId} for game {GameId}.", vertexId, gameId);
            return null;
        }
    }

    public async Task<string?> BuildCityAsync(Guid gameId, string vertexId, string playerId)
    {
        if (_container == null)
        {
            _logger.LogInformation("Blob container not configured; cannot retrieve game {GameId}.", gameId);
            return null;
        }

        try
        {
            var dto = await GetGameDTO(gameId.ToString());
            if (dto == null)
                return $"Unable to retrieve game {gameId}";

            var gs = new GameState(dto);
            var player = gs.Players.FirstOrDefault(p => p.Id == playerId);
            if (player == null)
                return $"Player {playerId} not found in game {gameId}";

            var vertex = gs.Vertices.FirstOrDefault(v => v.Id == vertexId);
            if (vertex == null)
                return $"Vertex {vertexId} not found in game {gameId}";

            if (vertex.Building == null || vertex.Owner == null)
                return $"Vertex {vertexId} in game {gameId} does not have a settlement to upgrade.";

            if (vertex.Building == BuildingType.City)
                return $"Vertex {vertexId} in game {gameId} already has a city.";

            if (vertex.Owner.Id != playerId)
                return $"Vertex {vertexId} in game {gameId} is owned by another player.";

            vertex.UpgradeToCity();

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() }
            };

            var updatedDto = new DTOs.GameStateDTO(gs);

            var json = JsonSerializer.Serialize(updatedDto, options);
            var blob = _container.GetBlobClient($"{gs.Id.ToString()}.json");

            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            // synchronous wait on async upload to keep CreateGame signature unchanged
            blob.Upload(ms, overwrite: true);

            _logger.LogInformation("Built city on vertex {VertexId} for player {PlayerId} in game {GameId}.", vertexId, playerId, gameId);
            return $"Built city on vertex {vertexId} for player {playerId} in game {gameId}.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to build city on vertex {VertexId} for game {GameId}.", vertexId, gameId);
            return null;
        }
    }

    public async Task<GameDice?> RollDiceAsync(Guid gameId)
    {
        if (_container == null)
        {
            _logger.LogInformation("Blob container not configured; cannot retrieve game {GameId}.", gameId);
            return null;
        }

        try
        {
            var dto = await GetGameDTO(gameId.ToString());
            if (dto == null)
            {
                _logger.LogError("Unable to retrieve game {GameId}.", gameId);
                return null;
            }

            var gs = new GameState(dto);
            gs.Dice.Roll();

            GamePlayHelpers.AssignResourcesBasedOnLastDiceRoll(gs);

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() }
            };

            var updatedDto = new DTOs.GameStateDTO(gs);

            var json = JsonSerializer.Serialize(updatedDto, options);
            var blob = _container.GetBlobClient($"{gs.Id.ToString()}.json");

            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            // synchronous wait on async upload to keep CreateGame signature unchanged
            blob.Upload(ms, overwrite: true);

            _logger.LogInformation("Rolled: {die1}, {die2}.", gs.Dice.Die1.Value, gs.Dice.Die2.Value);

            return gs.Dice;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to rolle dice for game {GameId}.", gameId);
            return null;
        }
    }

    public async Task<bool> EndTurnAsync(Guid gameId)
    {
        if (_container == null)
        {
            _logger.LogInformation("Blob container not configured; cannot retrieve game {GameId}.", gameId);
            return false;
        }

        try
        {
            var dto = await GetGameDTO(gameId.ToString());
            if (dto == null)
            {
                _logger.LogError("Unable to retrieve game {GameId}.", gameId);
                return false;
            }

            var gs = new GameState(dto);

            // TODO: Need to get player from authorization. Using CurrentPlayer for now.
            if (gs.Phase.CurrentPlayer == null || gs.Phase.PhaseState != GameStates.BuildOrTrade)
            {
                _logger.LogError("Game isn't in a state where EndTurn is valid.");
                return false;
            }
            var player = gs.Phase.CurrentPlayer;
            GamePlayHelpers.EndTurn(player, gs);

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() }
            };

            var updatedDto = new DTOs.GameStateDTO(gs);

            var json = JsonSerializer.Serialize(updatedDto, options);
            var blob = _container.GetBlobClient($"{gs.Id.ToString()}.json");

            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            // synchronous wait on async upload to keep CreateGame signature unchanged
            blob.Upload(ms, overwrite: true);

            _logger.LogInformation("Ended turn for Player {playerId}.", player);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to end turn in {GameId}.", gameId);
            return false;
        }
    }

        public async Task<bool> StartGameAsync(Guid gameId)
    {
        if (_container == null)
        {
            _logger.LogInformation("Blob container not configured; cannot retrieve game {GameId}.", gameId);
            return false;
        }

        try
        {
            var dto = await GetGameDTO(gameId.ToString());
            if (dto == null)
            {
                _logger.LogError("Unable to retrieve game {GameId}.", gameId);
                return false;
            }

            var gs = new GameState(dto);

            if (gs.Phase.PhaseState != GameStates.SettingUpBoard)
            {
                _logger.LogError("Game isn't in a state where StartGame is valid.");
                return false;
            }

            // TODO: In the future, will need every human player to hit start before a game starts.
            // Current version only needs one start call and it starts the game for everyone.
            GamePlayHelpers.StartGame(gs);

            GamePlayHelpers.AssignResourcesBasedOnLastDiceRoll(gs);

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() }
            };

            var updatedDto = new DTOs.GameStateDTO(gs);

            var json = JsonSerializer.Serialize(updatedDto, options);
            var blob = _container.GetBlobClient($"{gs.Id.ToString()}.json");

            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            // synchronous wait on async upload to keep CreateGame signature unchanged
            blob.Upload(ms, overwrite: true);

            _logger.LogInformation("Started game.");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start game {GameId}.", gameId);
            return false;
        }
    }

}