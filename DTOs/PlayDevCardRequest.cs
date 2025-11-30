namespace GameTest.DTOs;

public class PlayDevCardRequest : BaseRequest
{
    public string DevCardType { get; init; }
    public List<string> SelectedResources { get; init; }
    public string? TileId { get; init; }

    public PlayDevCardRequest(string playerId, string devCardType, List<string>? selectedResources, string? tileId) : base(playerId)
    {
        DevCardType = devCardType;

        if (selectedResources == null)
            SelectedResources = new List<string>();
        else
            SelectedResources = selectedResources;

        if (tileId != null)
            TileId = tileId;
    }
}