using GameTest.DTOs;
using Microsoft.AspNetCore.Mvc.ApiExplorer;

namespace GameTest.Models;

public class TradeResponse
{
    public Player Player { get; private set; }
    public TradeResponseType ResponseType { get; set; }
    public Dictionary<ResourceType, int>? Offer { get; private set; }
    public Dictionary<ResourceType, int>? Request { get; private set; }

    public TradeResponse(Player player, TradeResponseType resposneType, Dictionary<ResourceType, int>? offer, Dictionary<ResourceType, int>? request)
    {
        Player = player ?? throw new ArgumentNullException(nameof(player));
        ResponseType = resposneType;

        if (ResponseType == TradeResponseType.Accept || ResponseType == TradeResponseType.Reject)
        {
            if ((offer != null && offer.Count > 0) || (request != null && request.Count > 0))
                throw new ArgumentException("Offer and request must be null or empty if accepting/rejecting offer.");
        }

        if (resposneType == TradeResponseType.Counter || resposneType == TradeResponseType.Original)
        {
            if (offer == null || offer.Count == 0)
                throw new ArgumentNullException(nameof(offer), "Offer cannot be null or empty for a counter or original trade response.");

            if (request == null || request.Count == 0)
                throw new ArgumentNullException(nameof(request), "Request cannot be null or empty for a counter or original trade response.");

            if (offer.Count == request.Count && !offer.Except(request).Any())
                throw new ArgumentException("Offer and Request cannot be the same for a counter or original trade response.");
        }

        Offer = offer;
        Request = request;
    }

    public TradeResponse(GameState gs, TradeResponseDTO dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        if (gs == null)
            throw new ArgumentNullException(nameof(gs));

        Player = gs.Players.First(p => p.Id == dto.PlayerId);
        ResponseType = dto.ResponseType;

        if ((ResponseType == TradeResponseType.Counter || ResponseType == TradeResponseType.Original) && dto.Offer != null && dto.Request != null)
        {
            Offer = new Dictionary<ResourceType, int>();
            Request = new Dictionary<ResourceType, int>();

            foreach (var resource in dto.Offer)
                Offer[resource.Key] = resource.Value;

            foreach (var resource in dto.Request)
                Request[resource.Key] = resource.Value;
        }
    }
}
