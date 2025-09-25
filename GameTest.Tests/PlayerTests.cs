using Xunit;
using GameTest.Models;
using System.Drawing;

namespace GameTest.Tests;

public class PlayerTests
{
    [Fact]
    public void Constructor_Default_CreatesExpectedPlayer()
    {
        // Act
        var player = new Player();

        // Assert
        Assert.True(player.Id >= 1);
        Assert.Equal(string.Empty, player.Name);
        Assert.Equal(PlayerColor.Red, player.Color);
    }

    [Fact]
    public void Setters_CanChangeAllButId()
    {
        // Act
        var player = new Player();
        player.Name = "Alice";
        player.Color = PlayerColor.Blue;

        // Assert
        Assert.True(player.Id >= 1);
        Assert.Equal("Alice", player.Name);
        Assert.Equal(PlayerColor.Blue, player.Color);
    }

}