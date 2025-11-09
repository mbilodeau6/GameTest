using Xunit;
using GameTest.Models;
using GameTest.DTOs;
using GameTest.Services;
using GameTest.Functions;
using Microsoft.VisualStudio.TestPlatform.Common.ExtensionFramework;

namespace GameTest.Tests;

public class TradeRequestDTOTests
{
    [Fact]
    public void Constructor_TradeRequestToDTO()
    {
        // Arrange
        var player = new Player("Alice", PlayerColor.Green);
        var offer = new Dictionary<ResourceType, int>
        {
            { ResourceType.Wood, 2 },
            { ResourceType.Brick, 1 }
        };
        var request = new Dictionary<ResourceType, int>
        {
            { ResourceType.Wool, 3 }
        };
        var tradeRequest = new TradeRequest(player, offer, request);

        // Act
        var dto = new TradeRequestDTO(tradeRequest);

        // Assert
        Assert.Equal(player.Id, dto.PlayerId);
        Assert.Equal(2, dto.Offer.Count);
        Assert.Equal(2, dto.Offer["Wood"]);
        Assert.Equal(1, dto.Offer["Brick"]);
        Assert.Single(dto.Request);
        Assert.Equal(3, dto.Request["Wool"]);
    }
}
