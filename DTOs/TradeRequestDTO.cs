using System.Text.Json.Serialization;
using GameTest.Models;

namespace GameTest.DTOs;

public class TradeRequestDTO
{
    public string PlayerId { get; private set; }
    public Dictionary<string, int> Offer { get; private set; }
    public Dictionary<string, int> Request { get; private set; }

    // JsonConstructor lets System.Text.Json bind constructor parameters to JSON properties.
    [JsonConstructor]
    public TradeRequestDTO(string plyaerId, Dictionary<string, int> offer, Dictionary<string, int> request)
    {
        PlayerId = plyaerId ?? string.Empty;
        Offer = offer ?? new Dictionary<string, int>();
        Request = request ?? new Dictionary<string, int>();
    }
    
    public TradeRequestDTO(TradeRequest request)
    {
        PlayerId = request.Player.Id;
        Offer = new Dictionary<string, int>();
        Request = new Dictionary<string, int>();

        foreach (var resource in request.Offer)
            Offer[resource.Key.ToString()] = resource.Value;

        foreach (var resource in request.Request)
            Request[resource.Key.ToString()] = resource.Value;
    }
}