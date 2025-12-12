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
        player.SetVictoryPoints(6);

        return player;
    }

    [Fact]
    public void Constructor_ResourcesAndDevCardsIncluded_CreatesDTO()
    {
        var player = createTestPlayer();

        var playerDto = new PlayerDTO(player, false);

        Assert.Equal(player.Name, playerDto.Name);
        Assert.Equal(player.Color, playerDto.Color);
        Assert.Equal(player.ResourceCount, playerDto.ResourceCount);
        Assert.Equal(player.Resources[ResourceType.Wool], playerDto.Resources[ResourceType.Wool]);
        Assert.False(player.IsBot);
        Assert.Equal(2, playerDto.DevelopmentCardCount);
        Assert.NotNull(playerDto.DevCardsPlayed);
        Assert.Single(playerDto.DevCardsPlayed);
        Assert.Contains(DevelopmentCardType.Knight, playerDto.DevCardsPlayed);
        Assert.NotNull(playerDto.DevCardsPurchasedThisRound);
        Assert.Single(playerDto.DevCardsPurchasedThisRound);
        Assert.Contains(DevelopmentCardType.VictoryPoint, playerDto.DevCardsPurchasedThisRound);
        Assert.NotNull(playerDto.DevCardsReadyToPlay);
        Assert.Single(playerDto.DevCardsReadyToPlay);
        Assert.Contains(DevelopmentCardType.Monopoly, playerDto.DevCardsReadyToPlay);
        Assert.Equal(6, playerDto.VictoryPoints);
    }

    [Fact]
    public void Constructor_ResourceDevCountsOnly_CreatesDTO()
    {
        var player = createTestPlayer();

        var playerDto = new PlayerDTO(player, true);

        Assert.Equal(player.Name, playerDto.Name);
        Assert.Equal(player.Color, playerDto.Color);
        Assert.Equal(player.ResourceCount, playerDto.ResourceCount);
        Assert.Equal(0, playerDto.Resources[ResourceType.Wool]);
        Assert.Equal(player.DevelopmentCardCount, playerDto.DevelopmentCardCount);
        Assert.Empty(playerDto.DevCardsPurchasedThisRound);
        Assert.Empty(playerDto.DevCardsReadyToPlay);
        Assert.NotNull(playerDto.DevCardsPlayed);
        Assert.NotEmpty(playerDto.DevCardsPlayed);  // Played cards are always visible
        Assert.Contains(DevelopmentCardType.Knight, playerDto.DevCardsPlayed);
    }
}