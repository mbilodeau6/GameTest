namespace GameTest.Models;
using System.Text.Json.Serialization;

public class GameDice
{
    public GameDie Die1 { get; }
    public GameDie Die2 { get; }
    public bool WaitingForRoll { get; private set; } = false;

    public GameDice(bool random)
    {
        Die1 = new GameDie(random);
        Die2 = new GameDie(random);

        if (!random) 
            Die2.Roll();
    }

    // Only used for serialization/deserialized for displaying last roll
    public GameDice(int die1Value, int die2Value, bool random = true, bool waitingForRoll = false)
    {
        Die1 = new GameDie(die1Value, random);
        Die2 = new GameDie(die2Value, random);
        WaitingForRoll = waitingForRoll;
    }

    // JsonConstructor lets System.Text.Json bind constructor parameters to JSON properties.
    [JsonConstructor]
    public GameDice(GameDie? die1 = null, GameDie? die2 = null, bool waitingForRoll = false)
    {
        Die1 = die1 ?? new GameDie();
        Die2 = die2 ?? new GameDie();
        WaitingForRoll = waitingForRoll;
    }

    public void Roll()
    {
        Die1.Roll();
        Die2.Roll();
        WaitingForRoll = false;
    }

    public int GetCombinedValue()
    {
        return Die1.Value + Die2.Value;
    }

    public void SetWaiting()
    {
        WaitingForRoll = true;
    }
}