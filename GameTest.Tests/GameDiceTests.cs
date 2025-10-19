using Xunit;
using GameTest.Models;

namespace GameTest.Tests;

public class GameDiceTests
{
    [Fact]
    public void Constructor_Random()
    {
        GameDice dice = new GameDice();

        Assert.True(dice.Die1.Random);
        Assert.True(dice.Die2.Random);

        for(int i = 0; i < 5; i++)
        {
            dice.Roll();
            int sum = dice.Die1.Value + dice.Die2.Value;
            Assert.True(sum >= 2 && sum <= 12);
        }
    }

    [Fact]
    public void Constructor_NotRandom()
    {
        GameDice dice = new GameDice(false);

        Assert.False(dice.Die1.Random);
        Assert.False(dice.Die2.Random);

        dice.Roll();
        Assert.Equal(3, dice.Die1.Value + dice.Die2.Value);
        dice.Roll();
        Assert.Equal(4, dice.Die1.Value + dice.Die2.Value);
        dice.Roll();
        Assert.Equal(9, dice.Die1.Value + dice.Die2.Value);
    }
}