using GameTest.Models;
using System.Text.Json.Serialization;

namespace GameTest.DTOs;

public class PortDTO
{
    public string Id { get; private set; }
    public PortType Type { get; private set; }
    public List<string> Vertices { get; init; } = new List<string>();

    public PortDTO(Port port)
    {
        Id = port.Id;
        Type = port.Type;

        foreach (var vertex in port.Vertices)
            Vertices.Add(vertex.Id);
    }

    // JsonConstructor parameters must match the JSON property names (case-insensitive).
    [JsonConstructor]
    public PortDTO(string id, PortType type, List<string> vertices)
    {
        Id = id ?? string.Empty;
        Type = type;
        foreach (var vertexId in vertices)
            Vertices.Add(vertexId);
    }
}
