using Xunit;
using GameTest.Models;
using GameTest.DTOs;

namespace GameTest.Tests;

public class TradeResponseDTOTests
{
    [Fact]
    public void Constructor_Counter_Valid()
    {
        // Arrange
        var player = Player.CreateTestPlayer("player1", PlayerColor.Red);
        var offer = new Dictionary<ResourceType, int> { { ResourceType.Wood, 1 }, { ResourceType.Wool, 1 } };
        var request = new Dictionary<ResourceType, int> { { ResourceType.Brick, 3 } };
        var response = new TradeResponse(player, TradeResponseType.Counter, offer, request);

        // Act
        var dto = new TradeResponseDTO(response);

        // Assert
        Assert.Equal(player.Id, dto.PlayerId);
        Assert.Equal(TradeResponseType.Counter, dto.ResponseType);
        Assert.NotNull(dto.Offer);
        Assert.Equal(offer.Count, dto.Offer.Count);
        Assert.True(dto.Offer.ContainsKey(ResourceType.Wood) && dto.Offer[ResourceType.Wood] == 1);
        Assert.True(dto.Offer.ContainsKey(ResourceType.Wool) && dto.Offer[ResourceType.Wool] == 1);
        Assert.NotNull(dto.Request);
        Assert.Equal(request.Count, dto.Request.Count);
        Assert.True(dto.Request.ContainsKey(ResourceType.Brick) && dto.Request[ResourceType.Brick] == 3);
    }

    [Fact]
    public void Constructor_Reject_Valid()
    {
        // Arrange
        var player = Player.CreateTestPlayer("player1", PlayerColor.Red);
        var response = new TradeResponse(player, TradeResponseType.Reject, null, null);

        // Act
        var dto = new TradeResponseDTO(response);

        // Assert
        Assert.Equal(player.Id, dto.PlayerId);
        Assert.Equal(TradeResponseType.Reject, dto.ResponseType);
        Assert.Null(dto.Offer);
        Assert.Null(dto.Request);
    }
}
