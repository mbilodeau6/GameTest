namespace GameTest.Models;

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