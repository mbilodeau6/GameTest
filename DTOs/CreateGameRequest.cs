namespace GameTest.DTOs;

public class CreateGameRequest : BaseRequest
{
    public string GameType { get; init; } = string.Empty;
}