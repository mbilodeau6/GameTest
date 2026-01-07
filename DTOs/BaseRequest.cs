namespace GameTest.DTOs;

public class BaseRequest
{
    public string? PlayerId { get; init; } = null;

    public string? PlayerToken { get; init; } = null;

    public BaseRequest(string? playerId = null, string? playerToken = null)
    {
        PlayerId = playerId;
        PlayerToken = playerToken; 
    }
}