using Xunit;
using GameTest.Models;
using System.Collections;

namespace GameTest.Tests;

public class GameDiceTests
{
    [Fact]
    public void Constructor_Random()
    {
        GameDice dice = new GameDice(true);

        Assert.True(dice.Die1.Random);
        Assert.True(dice.Die2.Random);
    }

    [Fact]
    public void Constructor_NotRandom()
    {
        GameDice dice = new GameDice(false);

        Assert.False(dice.Die1.Random);
        Assert.False(dice.Die2.Random);
    }

    [Fact]
    public void Constructor_Serialization()
    {
        GameDice dice = new GameDice(5, 1);

        Assert.True(dice.Die1.Random);
        Assert.Equal(5, dice.Die1.Value);
        Assert.True(dice.Die2.Random);
        Assert.Equal(1, dice.Die2.Value);
    }

    [Fact]
    public void Roll_RandomDice()
    {
        GameDice dice = new GameDice(true);

        for (int i = 0; i < 5; i++)
        {
            dice.Roll();
            int sum = dice.Die1.Value + dice.Die2.Value;
            Assert.True(sum >= 2 && sum <= 12);
        }

    }

    [Fact]
    public void Roll_NonRandomDice()
    {
        GameDice dice = new GameDice(false);

        dice.Roll();
        Assert.Equal(3, dice.Die1.Value + dice.Die2.Value);
        dice.Roll();
        Assert.Equal(5, dice.Die1.Value + dice.Die2.Value);
        dice.Roll();
        Assert.Equal(7, dice.Die1.Value + dice.Die2.Value);
    }
    
    [Fact]
    public void GetCombinedValue_Valid()
    {
        // Arrange
        GameDice dice = new GameDice(false);
        dice.Roll();

        // Act
        // Assert
        Assert.Equal(3, dice.GetCombinedValue());
    }

}