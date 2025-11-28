namespace GameTest.DTOs;

public class PlaceOnTileRequest
{
    public string PlayerId { get; init; } = string.Empty;
    public string TileId { get; init; } = string.Empty;
}