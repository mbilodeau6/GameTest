using Xunit;
using GameTest.Models;

namespace GameTest.Tests;

public class GameDieTests
{
    [Fact]
    public void Constructor_Random()
    {
        GameDie die = new GameDie();

        Assert.True(die.Random);

        for(int i = 0; i < 5; i++)
        {
            die.Roll();
            Assert.True(die.Value >= 1 && die.Value <= 6);
        }
    }

    [Fact]
    public void Constructor_NotRandom()
    {
        GameDie die = new GameDie(false);

        Assert.False(die.Random);

        die.Roll();
        Assert.Equal(1, die.Value);
        die.Roll();
        Assert.Equal(2, die.Value);
        die.Roll();
        Assert.Equal(3, die.Value);
        die.Roll();
        die.Roll();
        die.Roll();
        Assert.Equal(6, die.Value);
        die.Roll();
        Assert.Equal(1, die.Value);

    }

    [Fact]
    public void Constructor_Serialization()
    {
        GameDie die = new GameDie(4);
        Assert.True(die.Random);
        Assert.Equal(4, die.Value);
    }
}