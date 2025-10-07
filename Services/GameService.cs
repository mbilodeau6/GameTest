using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure;
using Azure.Storage.Blobs;
using GameTest.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

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

    public GameState CreateGame(string gameType)
    {
        // Minimal behavior: create and return a new GameState with a new GUID
        var gs = new GameState(Guid.NewGuid());

        // Try to persist a DTO representation to blob storage (best-effort).
        try {
            if (_container != null)
            {
                var dto = new DTOs.GameStateDTO(gs);

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Converters = { new JsonStringEnumConverter() }
                };

                var json = JsonSerializer.Serialize(dto, options);
                var blob = _container.GetBlobClient($"{gs.Id}.json");

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
}