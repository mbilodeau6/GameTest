using GameTest.Models;

namespace GameTest.DTOs;

public class VertexDTO
{
    public string Id { get; init; }
    public string Building { get; init; }
    public string PlayerId { get; init; }

    public VertexDTO(Vertex vertex)
    {
        Id = vertex.Id;
        Building = vertex.Building.ToString();
        PlayerId = vertex.Owner.Id;
    }
}