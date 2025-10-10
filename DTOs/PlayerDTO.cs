using GameTest.Models;
using System.Text.Json.Serialization;

namespace GameTest.DTOs;

public class PlayerDTO
{
    public string Id { get; }
    public string Name { get; }
    public string Color { get; }

    // JsonConstructor parameters must match the JSON property names (case-insensitive).
    [JsonConstructor]
    public PlayerDTO(string id, string name, string color)
    {
        Id = id ?? string.Empty;
        Name = name ?? string.Empty;
        Color = color ?? string.Empty;
    }

    public PlayerDTO(Player player)
    {
        Id = player.Id;
        Name = player.Name;
        Color = player.Color.ToString();
    }
}