namespace GameTest.DTOs;

public class BaseRequest
{
    public string PlayerId { get; init; } = string.Empty;

    public BaseRequest(string playerId)
    {
        PlayerId = playerId;
    }
}