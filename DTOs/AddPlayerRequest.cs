using GameTest.Models;

namespace GameTest.DTOs;

public class AddPlayerRequest
{
    public string? PlayerName { get; init; }
    public bool IsBot { get; init; } = true;
    public PlayerColor? PreferredColor { get; init; }
}
