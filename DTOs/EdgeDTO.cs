using GameTest.Models;

namespace GameTest.DTOs;

public class EdgeDTO
{
    public string Id { get; private set; }
    public string PlayerId { get; private set; }

    public EdgeDTO(Edge edge)
    {
        Id = edge.Id;
        PlayerId = edge.Owner.Id;
    }
}
