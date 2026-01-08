using Xunit;
using GameTest.Models;
using GameTest.DTOs;

namespace GameTest.Tests;

public class ResponseDTOTests
{
    [Fact]
    public void Constructor_UnknownErrorCode()
    {
        var gs = new GameState(new Guid(), "UT");
        var response = new ResponseDTO(false, 1, "A: 1", gs);

        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Equal("Unknown error code. (A: 1)", response.ErrorMessage);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void Constructor_NoParams()
    {
        var response = new ResponseDTO(false, 1, null!, null as GameState);

        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Equal("Unknown error code.", response.ErrorMessage);
        Assert.Null(response.GameState);
    }

    [Fact]
    public void Constructor_Success()
    {
        var gs = new GameState(new Guid(), "UT");
        var response = new ResponseDTO(true, 1, "A: 1", gs);

        Assert.NotNull(response);
        Assert.True(response.Success);
        Assert.Equal(0, response.ErrorCode);
        Assert.Empty(response.ErrorMessage);
        Assert.NotNull(response.GameState);
        Assert.Equal(response.GameState.Id, gs.Id.ToString());
    }

    [Fact]
    public void Constructor_FullError()
    {
        var gs = new GameState(new Guid(), "UT");
        var response = new ResponseDTO(false, 1000, "GameId: 80ce2f25-c7ab-43d7-ace3-31735ca2a811", gs);

        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Equal("Invalid game id. (GameId: 80ce2f25-c7ab-43d7-ace3-31735ca2a811)", response.ErrorMessage);
        Assert.Equal(1000, response.ErrorCode);
        Assert.Null(response.GameState);
    }
}
