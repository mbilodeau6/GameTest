using Xunit;
using GameTest.Models;
using GameTest.Services;

namespace GameTest.Tests;

public class GameServiceTests
{
    [Fact]
    public void CreateGame_ReturnGameState()
    {
        // Act
        var gs = new GameService();
        var game = gs.CreateGame("Default");

        // Assert
        Assert.NotEmpty(game.Vertices);
        Assert.NotEmpty(game.Edges);
    }
}
