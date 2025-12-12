using System.Text.Json.Serialization;
using GameTest.Models;

namespace GameTest.DTOs;

public class TradeRequestDTO
{
    public string PlayerId { get; private set; }
    public Dictionary<ResourceType, int> Offer { get; private set; }
    public Dictionary<ResourceType, int> Request { get; private set; }

    // JsonConstructor lets System.Text.Json bind constructor parameters to JSON properties.
    [JsonConstructor]
    public TradeRequestDTO(string playerId, Dictionary<ResourceType, int> offer, Dictionary<ResourceType, int> request)
    {
        PlayerId = playerId ?? string.Empty;
        Offer = offer ?? new Dictionary<ResourceType, int>();
        Request = request ?? new Dictionary<ResourceType, int>();
    }
    
    public TradeRequestDTO(TradeRequest request)
    {
        PlayerId = request.Player.Id;
        Offer = new Dictionary<ResourceType, int>();
        Request = new Dictionary<ResourceType, int>();

        foreach (var resource in request.Offer)
            Offer[resource.Key] = resource.Value;

        foreach (var resource in request.Request)
            Request[resource.Key] = resource.Value;
    }
}