namespace GameTest.DTOs;

public class SelectTargetRequest : BaseRequest
{
    public string TargetPlayerId { get; init; }

    public SelectTargetRequest(string playerId, string targetPlayerId) : base(playerId)
    {
        TargetPlayerId = targetPlayerId;
    }
}
