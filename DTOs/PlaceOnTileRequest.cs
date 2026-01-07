namespace GameTest.DTOs;

public class PlaceOnTileRequest : BaseRequest
{
    public string TileId { get; init; } = string.Empty;
}