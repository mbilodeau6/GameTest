namespace GameTest.DTOs;

public class UndoRequest : BaseRequest
{
    public int EventId { get; init; }

    public UndoRequest(string playerId, int eventId) : base(playerId)
    {
        EventId = eventId;
    }
}
