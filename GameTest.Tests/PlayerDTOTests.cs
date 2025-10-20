using Xunit;
using GameTest.Models;
using GameTest.DTOs;

namespace GameTest.Tests;

public class PlayerDTOTests
{
    private Player createTestPlayer()
    {
        var player = new Player("Henry", PlayerColor.White);
        player.AssignDevelopmentCard(DevelopmentCardType.Monopoly);
        player.AssignResources(ResourceType.Wool, 2);

        return player;
    }

    [Fact]
    public void Constructor_ResourcesAndDevCardsIncluded_CreatesDTO()
    {
        var player = createTestPlayer();

        var playerDto = new PlayerDTO(player, false);

        Assert.Equal(player.Name, playerDto.Name);
        Assert.Equal(player.Color.ToString(), playerDto.Color);
        Assert.Equal(player.ResourceCount, playerDto.ResourceCount);
        Assert.Equal(player.Resources[ResourceType.Wool], playerDto.Resources[ResourceType.Wool]);
        Assert.Equal(player.DevelopmentCardCount, playerDto.DevelopmentCardCount);
        Assert.Equal(player.DevelopmentCards[DevelopmentCardType.Monopoly], playerDto.DevelopmentCards[DevelopmentCardType.Monopoly]);
        Assert.False(player.IsBot);
    }

    [Fact]
    public void Constructor_ResourceDevCountsOnly_CreatesDTO()
    {
        var player = createTestPlayer();

        var playerDto = new PlayerDTO(player, true);

        Assert.Equal(player.Name, playerDto.Name);
        Assert.Equal(player.Color.ToString(), playerDto.Color);
        Assert.Equal(player.ResourceCount, playerDto.ResourceCount);
        Assert.Equal(0, playerDto.Resources[ResourceType.Wool]);
        Assert.Equal(player.DevelopmentCardCount, playerDto.DevelopmentCardCount);
        Assert.Equal(0, playerDto.DevelopmentCards[DevelopmentCardType.Monopoly]);
    }
}