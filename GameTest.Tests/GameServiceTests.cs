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
        // TODO: Need to do a real test once CreateGame does something useful
        Assert.NotNull(game.Id);
    }
}
