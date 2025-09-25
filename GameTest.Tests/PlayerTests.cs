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
    public void Constructor_SetAll_ValidValues()
    {
        // Arrange
        PlayerColor expectedColor = PlayerColor.Green;
        string expectedName = "Robert";

        // Act
        var player = new Player(expectedName, expectedColor);

        // Assert
        Assert.True(player.Id >= 1);
        Assert.Equal(expectedName, player.Name);
        Assert.Equal(expectedColor, player.Color);
    }

    [Fact]
    public void Constructor_NullName()
    {
        // Arrange
        PlayerColor expectedColor = PlayerColor.Green;
        string expectedName = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new Player(expectedName, expectedColor));

        Assert.Equal("Name cannot be empty (Parameter 'name')", exception.Message);
    }

    [Fact]
    public void Constructor_EmptyName()
    {
        // Arrange
        PlayerColor expectedColor = PlayerColor.Green;
        string expectedName = string.Empty;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new Player(expectedName, expectedColor));

        Assert.Equal("Name cannot be empty (Parameter 'name')", exception.Message);
    }

    [Fact]
    public void Setters_CanChangeAllButId()
    {
        // Arrange
        PlayerColor expectedColor = PlayerColor.Blue;
        string expectedName = "Alice";

        // Act
        var player = new Player();
        player.Name = expectedName;
        player.Color = expectedColor;

        // Assert
        Assert.True(player.Id >= 1);
        Assert.Equal(expectedName, player.Name);
        Assert.Equal(expectedColor, player.Color);
    }

[Fact]
    public void ToString_Verify()
    {
        // Arrange
        PlayerColor expectedColor = PlayerColor.White;
        string expectedName = "Mary";

        // Act
        var player = new Player(expectedName, expectedColor);

        // Assert
        Assert.Equal("Mary (" + player.Id + ") - White", player.ToString());
    }

}