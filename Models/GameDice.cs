namespace GameTest.Models;
using System.Text.Json.Serialization;

public class GameDice
{
    public GameDie Die1 { get; }
    public GameDie Die2 { get; }

    public GameDice(bool random)
    {
        Die1 = new GameDie(random);
        Die2 = new GameDie(random);

        if (!random) 
            Die2.Roll();
    }

    // Only used for serialization/deserialized for displaying last roll
    public GameDice(int die1Value, int die2Value, bool random = true)
    {
        Die1 = new GameDie(die1Value, random);
        Die2 = new GameDie(die2Value, random);
    }

    // JsonConstructor lets System.Text.Json bind constructor parameters to JSON properties.
    [JsonConstructor]
    public GameDice(GameDie? die1 = null, GameDie? die2 = null)
    {
        Die1 = die1 ?? new GameDie();
        Die2 = die2 ?? new GameDie(); 
    }

    public void Roll()
    {
        Die1.Roll();
        Die2.Roll();
    }
}