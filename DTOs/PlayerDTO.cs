using GameTest.Models;

namespace GameTest.DTOs;

public class PlayerDTO
{
    public string Id { get; }
    public string Name { get; }
    public string Color { get; }
    public PlayerDTO(Player player)
    {
        Id = player.Id;
        Name = player.Name;
        Color = player.Color.ToString();
    }
}