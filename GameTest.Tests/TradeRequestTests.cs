using Xunit;
using GameTest.Models;
using GameTest.DTOs;

namespace GameTest.Tests;

public class TradeRequestTests
{
    [Fact]
    public void Constructor_MissingPlayer()
    {
        // Arrange
        var offer = new Dictionary<ResourceType, int>
        {
            { ResourceType.Brick, 2 }
        };
        var request = new Dictionary<ResourceType, int>
        {
            { ResourceType.Wood, 1 }
        };

        // Act
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new TradeRequest(null!, offer, request));

        // Assert
        Assert.Equal("Value cannot be null. (Parameter 'player')", exception.Message);
    }

    [Fact]
    public void Constructor_MissingOffer()
    {
        // Arrange
        var player = new Player("Alice", PlayerColor.Green);
        var request = new Dictionary<ResourceType, int>
        {
            { ResourceType.Wood, 1 }
        };

        // Act
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new TradeRequest(player, null!, request));

        // Assert
        Assert.Equal("Value cannot be null. (Parameter 'offer')", exception.Message);
    }

    [Fact]
    public void Constructor_MissingRequest()
    {
        // Arrange
        var player = new Player("Alice", PlayerColor.Green);
        var offer = new Dictionary<ResourceType, int>
        {
            { ResourceType.Wood, 1 }
        };

        // Act
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new TradeRequest(player, offer, null!));

        // Assert
        Assert.Equal("Value cannot be null. (Parameter 'request')", exception.Message);
    }

    [Fact]
    public void Constructor_Valid()
    {
        // Arrange
        var player = new Player("Alice", PlayerColor.Green);
        var offer = new Dictionary<ResourceType, int>
        {
            { ResourceType.Wood, 2 }
        };
        var request = new Dictionary<ResourceType, int>
        {
            { ResourceType.Ore, 1 }
        };

        // Act
        var tradeRequest = new TradeRequest(player, offer, request);

        // Assert
        Assert.Equal(player.Id, tradeRequest.Player.Id);
        Assert.NotNull(tradeRequest.Offer);
        Assert.Single(tradeRequest.Offer);
        Assert.Equal(2, tradeRequest.Offer[ResourceType.Wood]);
        Assert.NotNull(tradeRequest.Request);
        Assert.Single(tradeRequest.Request);
        Assert.Equal(1, tradeRequest.Request[ResourceType.Ore]);
    }

    [Fact]
    public void Constructor_FromDTO_NullDTO()
    {
        // Arrange
        var gs = new GameState(new Guid());

        // Act
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new TradeRequest(gs, null!));

        // Assert
        Assert.Equal("Value cannot be null. (Parameter 'dto')", exception.Message);
    }

    [Fact]
    public void Constructor_FromDTO_NullGameState()
    {
        // Arrange
        var dto = new TradeRequestDTO("player1", new Dictionary<ResourceType, int>
        {
            { ResourceType.Wood, 2 }
        }, new Dictionary<ResourceType, int>
        {
            { ResourceType.Brick, 1 }
        });

        // Act
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new TradeRequest(null!, dto));

        // Assert
        Assert.Equal("Value cannot be null. (Parameter 'gs')", exception.Message);
    }


    [Fact]
    public void Constructor_FromDTO_PlayerNotFound()
    {
        // Arrange
        var gs = new GameState(new Guid());
        var dto = new TradeRequestDTO("player1", new Dictionary<ResourceType, int>
        {
            { ResourceType.Wood, 2 }
        }, new Dictionary<ResourceType, int>
        {
            { ResourceType.Brick, 1 }
        });

        // Act
        var exception = Assert.Throws<InvalidOperationException>(() =>
            new TradeRequest(gs, dto));

        // Assert
        Assert.Equal("Sequence contains no matching element", exception.Message);
    }

    [Fact]
    public void Constructor_FromDTO_Successful()
    {
        // Arrange
        var gs = new GameState(new Guid());
        gs.Players.Add(new Player("Bob", PlayerColor.Red));
        var dto = new TradeRequestDTO(gs.Players[0].Id, new Dictionary<ResourceType, int>
        {
            { ResourceType.Wood, 2 }
        }, new Dictionary<ResourceType, int>
        {
            { ResourceType.Brick, 1 }
        });

        // Act
        var request = new TradeRequest(gs, dto);

        // Assert
        Assert.Equal(gs.Players[0].Id, request.Player.Id);
        Assert.NotNull(request.Offer);
        Assert.Single(request.Offer);
        Assert.True(request.Offer.ContainsKey(ResourceType.Wood));
        Assert.Equal(2, request.Offer[ResourceType.Wood]);
        Assert.NotNull(request.Request);
        Assert.Single(request.Request);
        Assert.True(request.Request.ContainsKey(ResourceType.Brick));
        Assert.Equal(1, request.Request[ResourceType.Brick]);
    }
}
