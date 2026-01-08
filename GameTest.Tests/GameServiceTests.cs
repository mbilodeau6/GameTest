using Xunit;
using GameTest.Models;
using GameTest.Services;

namespace GameTest.Tests;

public class GameServiceTests
{
    [Fact]
    public void CreateGame_RequiresValidToken()
    {
        // Act
        var gs = new GameService();
        Assert.Throws<UnauthorizedAccessException>(() => gs.CreateGame("Default", "InvalidToken"));
    }
}
