using GameTest.DTOs;

namespace GameTest.Models;

public class TradeRequest
{
    public Player Player { get; private set; }
    public Dictionary<ResourceType, int> Offer { get; private set; }
    public Dictionary<ResourceType, int> Request { get; private set; }

    public TradeRequest(Player player, Dictionary<ResourceType, int> offer, Dictionary<ResourceType, int> request)
    {
        Player = player ?? throw new ArgumentNullException(nameof(player));
        Offer = offer ?? throw new ArgumentNullException(nameof(offer));
        Request = request ?? throw new ArgumentNullException(nameof(request));
    }

    public TradeRequest(GameState gs, TradeRequestDTO dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        if (gs == null)
            throw new ArgumentNullException(nameof(gs));

        Player = gs.Players.First(p => p.Id == dto.PlayerId);

        Offer = new Dictionary<ResourceType, int>();
        Request = new Dictionary<ResourceType, int>();

        foreach (var resource in dto.Offer)
            Offer[resource.Key] = resource.Value;

        foreach (var resource in dto.Request)
            Request[resource.Key] = resource.Value;
    }
}
