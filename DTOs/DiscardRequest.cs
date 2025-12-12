using GameTest.Models;

namespace GameTest.DTOs;

public class DiscardRequest : BaseRequest
{
    public List<ResourceType> SelectedResources { get; init; }

    public DiscardRequest(string playerId, List<ResourceType> selectedResources) : base(playerId)
    {
        SelectedResources = new();
        foreach(var resource in selectedResources)
            SelectedResources.Add(resource);
    }
}