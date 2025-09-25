using Xunit;
using GameTest.Models;

namespace GameTest.Tests;

public class GameStateTests
{
    [Fact]
    public void Constructor_Default_EmptyState()
    {
        // Act
        var game = new GameState();

        // Assert
        Assert.Empty(game.Players);
        Assert.Empty(game.Tiles);
    }

    [Fact]
    public void AddPlayer_AddSinglePlayer()
    {
        // Arrange
        string expectedName = "Alice";

        // Act
        var game = new GameState();
        game.AddPlayer(new Player(expectedName, PlayerColor.Blue));

        // Assert
        Assert.NotEmpty(game.Players);
        Assert.Equal(expectedName, game.Players[0].Name);
    }    

    [Fact]
    public void AddTile_AddSingleTile()
    {
        // Arrange
        ResourceType expectedResourceType = ResourceType.Wool;

        // Act
        var game = new GameState();
        game.AddTile(new Tile(expectedResourceType, 5, 0, 0));  

        // Assert
        Assert.NotEmpty(game.Tiles);
        Assert.Equal(expectedResourceType, game.Tiles[0].Resource);
    }    

}