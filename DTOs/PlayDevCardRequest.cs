using GameTest.Models;

namespace GameTest.DTOs;

public class PlayDevCardRequest : BaseRequest
{
    public DevelopmentCardType DevCardType { get; init; }
    public List<ResourceType> SelectedResources { get; init; }
    public string? TileId { get; init; }

    public PlayDevCardRequest(string playerId, DevelopmentCardType devCardType, List<ResourceType>? selectedResources, string? tileId) : base(playerId)
    {
        DevCardType = devCardType;

        if (selectedResources == null)
            SelectedResources = new List<ResourceType>();
        else
            SelectedResources = selectedResources;

        if (tileId != null)
            TileId = tileId;
    }
}