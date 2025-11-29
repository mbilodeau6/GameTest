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
        player.AssignDevelopmentCard(DevelopmentCardType.Knight);
        player.MakeNewDevelopmentCardsPlayable();
        player.PlayDevelopmentCard(DevelopmentCardType.Knight);
        player.AssignDevelopmentCard(DevelopmentCardType.VictoryPoint);
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
        Assert.False(player.IsBot);
        Assert.Equal(2, playerDto.DevelopmentCardCount);
        Assert.NotNull(playerDto.DevCardsPlayed);
        Assert.Single(playerDto.DevCardsPlayed);
        Assert.Contains(DevelopmentCardType.Knight.ToString(), playerDto.DevCardsPlayed);
        Assert.NotNull(playerDto.DevCardsPurchasedThisRound);
        Assert.Single(playerDto.DevCardsPurchasedThisRound);
        Assert.Contains(DevelopmentCardType.VictoryPoint.ToString(), playerDto.DevCardsPurchasedThisRound);
        Assert.NotNull(playerDto.DevCardsReadyToPlay);
        Assert.Single(playerDto.DevCardsReadyToPlay);
        Assert.Contains(DevelopmentCardType.Monopoly.ToString(), playerDto.DevCardsReadyToPlay);
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
        Assert.Null(playerDto.DevCardsPurchasedThisRound);
        Assert.Null(playerDto.DevCardsReadyToPlay);
        Assert.Null(playerDto.DevCardsPlayed);
    }
}