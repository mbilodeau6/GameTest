namespace GameTest.DTOs;

public class DiscardRequest : BaseRequest
{
    public List<string> SelectedResources { get; init; }

    public DiscardRequest(string playerId, List<string> selectedResources) : base(playerId)
    {
        SelectedResources = selectedResources;
    }
}