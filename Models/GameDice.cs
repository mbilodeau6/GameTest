namespace GameTest.Models;

public class GameDice
{
    public GameDie Die1 { get; }
    public GameDie Die2 { get; }

    public GameDice(bool random = true)
    {
        Die1 = new GameDie(random);
        Die2 = new GameDie(random);

        Die2.Roll();
    }

    public void Roll()
    {
        Die1.Roll();
        Die2.Roll();
    }
}