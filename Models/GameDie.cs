namespace GameTest.Models;
using System.Text.Json.Serialization;


public class GameDie
{
    public int Value { get; private set; } = 0;
    public bool Random { get; }
    private int RollCount = 0;
    private List<int> nonRandomRolls = new List<int> { 2, 1, 3, 6, 4, 4, 1, 2, 5, 3 };

    private static readonly Random _random = new();

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
            Value = _random.Next(1, 7);
        }
        else
        {
            Value = nonRandomRolls[RollCount % nonRandomRolls.Count];
        }
        
        RollCount++;
    }
}