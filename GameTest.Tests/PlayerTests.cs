using Xunit;
using GameTest.Models;
using GameTest.DTOs;

namespace GameTest.Tests;

public class PlayerTests
{
    [Fact]
    public void Constructor_Default_CreatesExpectedPlayer()
    {
        // Act
        var player = new Player();

        // Assert
        Assert.True(TestHelpers.ValidateId(player.Id, 'P'));
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
        Assert.True(TestHelpers.ValidateId(player.Id, 'P'));
        Assert.Equal(expectedName, player.Name);
        Assert.Equal(expectedColor, player.Color);
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
    public void Constructor_FromDTO_ValidData()
    {
        // Arrange
        var orig_player = new Player("Mary", PlayerColor.White);
        var dto = new DTOs.PlayerDTO(orig_player);

        // Act
        var new_player = new Player(dto);

        // Assert
        Assert.Equal(orig_player.Id, new_player.Id);
        Assert.Equal(orig_player.Name, new_player.Name);
        Assert.Equal(orig_player.Color, new_player.Color);
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
        Assert.True(TestHelpers.ValidateId(player.Id, 'P'));
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