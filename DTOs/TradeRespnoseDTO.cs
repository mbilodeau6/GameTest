using System.Text.Json.Serialization;
using GameTest.Models;

namespace GameTest.DTOs;

public class TradeResponseDTO
{
    public string PlayerId { get; private set; }
    public TradeResponseType ResponseType { get; set; }
    public Dictionary<ResourceType, int>? Offer { get; private set; }
    public Dictionary<ResourceType, int>? Request { get; private set; }

    // JsonConstructor lets System.Text.Json bind constructor parameters to JSON properties.
    [JsonConstructor]
    public TradeResponseDTO(string playerId, TradeResponseType responseType, Dictionary<ResourceType, int> offer, Dictionary<ResourceType, int> request)
    {
        PlayerId = playerId;
        Offer = offer;
        Request = request;
        ResponseType = responseType;
    }
    
    public TradeResponseDTO(TradeResponse response)
    {
        PlayerId = response.Player.Id;
        ResponseType = response.ResponseType;

        if (ResponseType == TradeResponseType.Counter || ResponseType == TradeResponseType.Original)
        {
            if (response.Offer == null || response.Offer.Count == 0 || response.Request == null || response.Request.Count == 0)
                throw new InvalidOperationException("Offer and Request cannot be null or empty for a counter trade response.");
            else 
            {
                Offer = new Dictionary<ResourceType, int>();
                Request = new Dictionary<ResourceType, int>();

                foreach (var resource in response.Offer)
                    Offer[resource.Key] = resource.Value;

                foreach (var resource in response.Request)
                    Request[resource.Key] = resource.Value;
            }
        }
    }
}