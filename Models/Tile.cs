namespace GameTest.Models;

public class Tile
{
    public ResourceType Resource { get; private set; }
    public int DiceNumber { get; private set; }
    public bool HasRobber { get; private set; }
    public int X { get; private set; }
    public int Y { get; private set; }

    public Tile(ResourceType resource, int diceNumber, int x, int y)
    {
        if (diceNumber < 2 || diceNumber > 12)
            throw new ArgumentException("Dice number must be between 2 and 12", nameof(diceNumber));
        
        Resource = resource;
        DiceNumber = resource == ResourceType.Desert ? 7 : diceNumber;
        HasRobber = resource == ResourceType.Desert;
        X = x;
        Y = y;
    }

    public void MoveRobberTo()
    {
        HasRobber = true;
    }

    public void RemoveRobber()
    {
        HasRobber = false;
    }

    public override string ToString()
    {
        return $"{Resource} ({X},{Y})({DiceNumber}){(HasRobber ? " [Robber]" : "")}";
    }
}