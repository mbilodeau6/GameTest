using System;

namespace GameTest.Models;

public enum PlayerColor
{
    Red,
    Blue,
    White,
    Orange,
    Brown,
    Green,
    Yellow,
    Purple
}

public class Player
{
    private static int s_nextId = 0;

    public int Id { get; init; }
    public string Name { get; set; }
    public PlayerColor Color { get; set; }

    // Parameterless ctor for serializers
    public Player()
    {
        Id = Interlocked.Increment(ref s_nextId);

        // TODO: Need to assign a unique name
        Name = string.Empty;

        // TODO: Need to assign a color that is not already taken
        Color = PlayerColor.Red;
    }

    public Player(string name, PlayerColor color)
    {
        Id = Interlocked.Increment(ref s_nextId);

        // TODO: Need to ensure name and color are unique
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        Name = name;
        Color = color;
    }

    public override string ToString() => $"{Name} ({Id}) - {Color}";
}