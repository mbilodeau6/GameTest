namespace GameTest.DTOs;

public class AcceptTradeDTO : BaseRequest
{
    public string AcceptedPlayerId { get; init; }

    public AcceptTradeDTO(string playerId, string acceptedPlayerId) : base(playerId)
    {
        AcceptedPlayerId = acceptedPlayerId;
    }
}