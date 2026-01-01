using Xunit;
using GameTest.Models;
using GameTest.DTOs;

namespace GameTest.Tests;

public class TradeResponseTests
{
    [Fact]
    public void Constructor_Valid()
    {
        // Arrange
        var player = Player.CreateTestPlayer("player1", PlayerColor.Red);

        // Act
        var response = new TradeResponse(player, TradeResponseType.Reject, null, null);

        // Assert
        Assert.Equal(player.Id, response.Player.Id);
        Assert.Equal(TradeResponseType.Reject, response.ResponseType);
        Assert.Null(response.Offer);
        Assert.Null(response.Request);
    }

    [Fact]
    public void Constructor_EmptyOfferRequest_Valid()
    {
        // Arrange
        var player = Player.CreateTestPlayer("player1", PlayerColor.Red);
        var offer = new Dictionary<ResourceType, int>();
        var request = new Dictionary<ResourceType, int>();

        // Act
        var response = new TradeResponse(player, TradeResponseType.Accept, offer, request);

        // Assert
        Assert.Equal(player.Id, response.Player.Id);
        Assert.Equal(TradeResponseType.Accept, response.ResponseType);
        Assert.NotNull(response.Offer);
        Assert.Empty(response.Offer);
        Assert.NotNull(response.Request);
        Assert.Empty(response.Request);
    }

    [Fact]
    public void Constructor_OfferOnAccept_Invalid()
    {
        // Arrange
        var player = Player.CreateTestPlayer("player1", PlayerColor.Red);
        var offer = new Dictionary<ResourceType, int> { { ResourceType.Wood, 2 } };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new TradeResponse(player, TradeResponseType.Accept, offer, null));
    }

    [Fact]
    public void Constructor_RequestOnAccept_Invalid()
    {
        // Arrange
        var player = Player.CreateTestPlayer("player1", PlayerColor.Red);
        var request = new Dictionary<ResourceType, int> { { ResourceType.Wood, 2 } };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new TradeResponse(player, TradeResponseType.Accept, null, request));
    }

    [Fact]
    public void Constructor_Counter_MissingOffer()
    {
        // Arrange
        var player = Player.CreateTestPlayer("player1", PlayerColor.Red);
        var request = new Dictionary<ResourceType, int> { { ResourceType.Wood, 2 } };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TradeResponse(player, TradeResponseType.Counter, null, request));
    }

    [Fact]
    public void Constructor_Counter_MissingRequest()
    {
        // Arrange
        var player = Player.CreateTestPlayer("player1", PlayerColor.Red);
        var offer = new Dictionary<ResourceType, int> { { ResourceType.Wood, 2 } };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TradeResponse(player, TradeResponseType.Counter, offer, null));
    }

    [Fact]
    public void Constructor_Counter_OfferEmpty()
    {
        // Arrange
        var player = Player.CreateTestPlayer("player1", PlayerColor.Red);
        var offer = new Dictionary<ResourceType, int>();
        var request = new Dictionary<ResourceType, int> { { ResourceType.Wood, 2 } };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TradeResponse(player, TradeResponseType.Counter, offer, request));
    }

    [Fact]
    public void Constructor_Counter_RequestEmpty()
    {
        // Arrange
        var player = Player.CreateTestPlayer("player1", PlayerColor.Red);
        var request = new Dictionary<ResourceType, int>();
        var offer = new Dictionary<ResourceType, int> { { ResourceType.Wood, 2 } };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TradeResponse(player, TradeResponseType.Counter, offer, request));
    }

    [Fact]
    public void Constructor_Counter_RequestEqualsOffer()
    {
        // Arrange
        var player = Player.CreateTestPlayer("player1", PlayerColor.Red);
        var request = new Dictionary<ResourceType, int>() { { ResourceType.Wood, 1 } };
        var offer = new Dictionary<ResourceType, int> { { ResourceType.Wood, 1 } };

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new TradeResponse(player, TradeResponseType.Counter, offer, request));
    }

    [Fact]
    public void Constructor_Counter_Valid()
    {
        // Arrange
        var player = Player.CreateTestPlayer("player1", PlayerColor.Red);
        var request = new Dictionary<ResourceType, int>() { { ResourceType.Wood, 2 } };
        var offer = new Dictionary<ResourceType, int> { { ResourceType.Brick, 1 }, {ResourceType.Wool, 1 } };

        // Act
        var response = new TradeResponse(player, TradeResponseType.Counter, offer, request);

        // Assert
        Assert.Equal(player.Id, response.Player.Id);
        Assert.Equal(TradeResponseType.Counter, response.ResponseType);
        Assert.NotNull(response.Offer);
        Assert.Equal(2, response.Offer.Count);
        Assert.Contains(response.Offer, kvp => kvp.Key == ResourceType.Brick && kvp.Value == 1);
        Assert.Contains(response.Offer, kvp => kvp.Key == ResourceType.Wool && kvp.Value == 1);
        Assert.NotNull(response.Request);
        Assert.Single(response.Request);
        Assert.Contains(response.Request, kvp => kvp.Key == ResourceType.Wood && kvp.Value == 2);
    }


    [Fact]
    public void Constructor_DTO()
    {
        // Arrange
        var gs = new GameState(new Guid());
        var player = Player.CreateTestPlayer("player1", PlayerColor.Red);
        gs.AddPlayer(player);
        var request = new Dictionary<ResourceType, int>() { { ResourceType.Wood, 2 } };
        var offer = new Dictionary<ResourceType, int> { { ResourceType.Brick, 1 }, {ResourceType.Wool, 1 } };
        var response = new TradeResponse(player, TradeResponseType.Counter, offer, request);
        var dto = new TradeResponseDTO(response);

        // Act
        var responseFromDto = new TradeResponse(gs, dto);

        // Assert
        Assert.Equal(player.Id, responseFromDto.Player.Id);
        Assert.Equal(TradeResponseType.Counter, responseFromDto.ResponseType);
        Assert.NotNull(responseFromDto.Offer);
        Assert.Equal(2, responseFromDto.Offer.Count);
        Assert.Contains(responseFromDto.Offer, kvp => kvp.Key == ResourceType.Brick && kvp.Value == 1);
        Assert.Contains(responseFromDto.Offer, kvp => kvp.Key == ResourceType.Wool && kvp.Value == 1);
        Assert.NotNull(responseFromDto.Request);
        Assert.Single(responseFromDto.Request);
        Assert.Contains(responseFromDto.Request, kvp => kvp.Key == ResourceType.Wood && kvp.Value == 2);
    }

    [Fact]
    public void Constructor_Original_Valid()
    {
        // Arrange
        var player = Player.CreateTestPlayer("player1", PlayerColor.Red);
        var offer = new Dictionary<ResourceType, int> { { ResourceType.Wood, 2 } };
        var request = new Dictionary<ResourceType, int> { { ResourceType.Brick, 1 } };

        // Act
        var response = new TradeResponse(player, TradeResponseType.Original, offer, request);

        // Assert
        Assert.Equal(player.Id, response.Player.Id);
        Assert.Equal(TradeResponseType.Original, response.ResponseType);
        Assert.NotNull(response.Offer);
        Assert.Single(response.Offer);
        Assert.Contains(response.Offer, kvp => kvp.Key == ResourceType.Wood && kvp.Value == 2);
        Assert.NotNull(response.Request);
        Assert.Single(response.Request);
        Assert.Contains(response.Request, kvp => kvp.Key == ResourceType.Brick && kvp.Value == 1);
    }

    [Fact]
    public void Constructor_Original_CantGiveSomethingForNothing()
    {
        // Arrange
        var player = Player.CreateTestPlayer("player1", PlayerColor.Red);
        var offer = new Dictionary<ResourceType, int> { { ResourceType.Wood, 2 } };
        var request = new Dictionary<ResourceType, int>();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new TradeResponse(player, TradeResponseType.Original, offer, request));
    }
}
