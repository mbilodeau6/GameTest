namespace GameTest.Models;
using System.Text.Json.Serialization;
using GameTest.Services;

public class GameDie
{
    public int Value { get; private set; } = 0;
    public bool Random { get; }

    public GameDie(bool random = true)
    {
        Random = random;
    }

    // JsonConstructor lets System.Text.Json bind constructor parameters to JSON properties.
    // Only used for serialization/deserialized for displaying last roll
    [JsonConstructor]
    public GameDie(int value, bool random = true)
    {
        Random = random;
        Value = value;
    }

    public void Roll()
    {
        if (Random)
        {
            Value = SharedHelpers.NextRandom(1, 7);
        }
        else
        {
            Value = (Value % 6) +1;
        }
    }
}