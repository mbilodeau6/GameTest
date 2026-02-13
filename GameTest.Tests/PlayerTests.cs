using Xunit;
using GameTest.Models;
using GameTest.DTOs;
using System.Formats.Asn1;

namespace GameTest.Tests;

public class PlayerTests
{
    [Fact]
    public void Constructor_Default_CreatesExpectedPlayer()
    {
        // Act
        var player = new Player("P103", "Tim", PlayerColor.Red);

        // Assert
        Assert.Equal("P103", player.Id);
        Assert.Equal("Tim", player.Name);
        Assert.Equal(PlayerColor.Red, player.Color);
        Assert.All(player.Resources.Values, v => Assert.Equal(0, v));
        Assert.Equal(0, player.DevelopmentCardCount);
        Assert.Equal(0, player.ResourceCount);
        Assert.False(player.IsBot);
        Assert.Empty(player.DevCardsPlayed);
        Assert.Empty(player.DevCardsPurchasedThisRound);
        Assert.Empty(player.DevCardsReadyToPlay);
        Assert.Equal(0, player.FullVictoryPoints);
        Assert.Equal(0, player.VisibleVictoryPoints);
        Assert.Equal(0, player.LongestRoadLength);
    }

    [Fact]
    public void Constructor_EmptyName()
    {
        // Arrange
        PlayerColor expectedColor = PlayerColor.Green;
        string expectedName = string.Empty;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new Player("P103", expectedName, expectedColor));
    }

    [Fact]
    public void Constructor_EmptyId()
    {
        // Arrange
        PlayerColor expectedColor = PlayerColor.Green;
        string expectedName = "Tim";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new Player(" ", expectedName, expectedColor));
    }

    [Fact]
    public void Constructor_FromDTO_ValidData()
    {
        // Arrange
        var orig_player = Player.CreateTestPlayer("Mary", PlayerColor.White, true);
        orig_player.AssignDevelopmentCard(DevelopmentCardType.RoadBuilding);
        orig_player.AssignDevelopmentCard(DevelopmentCardType.Knight);
        orig_player.MakeNewDevelopmentCardsPlayable();
        orig_player.AssignDevelopmentCard(DevelopmentCardType.YearOfPlenty);
        orig_player.AssignDevelopmentCard(DevelopmentCardType.Knight);
        orig_player.PlayDevelopmentCard(DevelopmentCardType.Knight);
        orig_player.AssignResources(ResourceType.Brick, 2);
        orig_player.AssignResources(ResourceType.Ore, 1);
        orig_player.SetVictoryPoints(3, 5);
        orig_player.SetLongRoadLength(2);

        var dto = new DTOs.PlayerDTO(orig_player, false);

        // Act
        var new_player = new Player(dto);

        // Assert
        Assert.Equal(orig_player.Id, new_player.Id);
        Assert.Equal(orig_player.Name, new_player.Name);
        Assert.Equal(orig_player.Color, new_player.Color);
        Assert.Equal(3, new_player.DevelopmentCardCount);
        Assert.Single(new_player.DevCardsPlayed);
        Assert.Contains(DevelopmentCardType.Knight, new_player.DevCardsPlayed);
        Assert.Single(new_player.DevCardsReadyToPlay);
        Assert.Contains(DevelopmentCardType.RoadBuilding, new_player.DevCardsReadyToPlay);
        Assert.Equal(2, new_player.DevCardsPurchasedThisRound.Count);
        Assert.Contains(DevelopmentCardType.Knight, new_player.DevCardsPurchasedThisRound);
        Assert.Contains(DevelopmentCardType.YearOfPlenty, new_player.DevCardsPurchasedThisRound);
        Assert.Equal(3, new_player.ResourceCount);
        Assert.Equal(2, new_player.Resources[ResourceType.Brick]);
        Assert.Equal(1, new_player.Resources[ResourceType.Ore]);
        Assert.Equal(0, new_player.Resources[ResourceType.Grain]);
        Assert.True(new_player.IsBot);
        Assert.Equal(5, new_player.FullVictoryPoints);
        Assert.Equal(3, new_player.VisibleVictoryPoints);
        Assert.Equal(orig_player.LongestRoadLength, new_player.LongestRoadLength);
    }

    [Fact]
    public void Setters_CanChangeAllButId()
    {
        // Arrange
        PlayerColor expectedColor = PlayerColor.Blue;
        string expectedName = "Alice";

        // Act
        var player = Player.CreateTestPlayer(expectedName, expectedColor);
        player.SetLongRoadLength(3);

        // Assert
        Assert.True(TestHelpers.ValidateId(player.Id, 'P'));
        Assert.Equal(expectedName, player.Name);
        Assert.Equal(expectedColor, player.Color);
        Assert.Equal(3, player.LongestRoadLength);
    }

    [Fact]
    public void ToString_Verify()
    {
        // Arrange
        PlayerColor expectedColor = PlayerColor.White;
        string expectedName = "Mary";

        // Act
        var player = Player.CreateTestPlayer(expectedName, expectedColor);

        // Assert
        Assert.Equal("Mary (" + player.Id + ") - White", player.ToString());
    }

    [Fact]
    public void AssignResource_Single()
    {
        // Arrange
        var player = Player.CreateTestPlayer("Mary", PlayerColor.Red);

        // Act
        player.AssignResources(ResourceType.Grain, 1);

        // Assert
        Assert.Equal(1, player.Resources[ResourceType.Grain]);
        Assert.Equal(1, player.ResourceCount);
    }

    [Fact]
    public void AssignResource_Multiple()
    {
        // Arrange
        var player = Player.CreateTestPlayer("Mary", PlayerColor.Red);
        player.AssignResources(ResourceType.Brick, 1);
        player.AssignResources(ResourceType.Ore, 1);

        // Act
        player.AssignResources(ResourceType.Ore, 2);

        // Assert
        Assert.Equal(3, player.Resources[ResourceType.Ore]);
        Assert.Equal(4, player.ResourceCount);
    }

    [Fact]
    public void AssignResource_Desert_ThrowsException()
    {
        // Arrange
        var player = Player.CreateTestPlayer("Mary", PlayerColor.Red);

        // Act
        var exception = Assert.Throws<ArgumentException>(() =>
            player.AssignResources(ResourceType.Desert, 1));

        Assert.Equal("Desert is not a resource that can be earned/owned.", exception.Message);
    }

    private Player CreatePlayerWithResources()
    {
        var player = Player.CreateTestPlayer("Mary", PlayerColor.Red);
        player.AssignResources(ResourceType.Brick, 2);
        player.AssignResources(ResourceType.Ore, 3);

        return player;
    }

    [Fact]
    public void RemoveResources_ExactAmount()
    {
        // Arrange
        var player = CreatePlayerWithResources();
        Assert.Equal(2, player.Resources[ResourceType.Brick]);

        // Act
        player.RemoveResources(ResourceType.Brick, 2);

        // Assert
        Assert.Equal(0, player.Resources[ResourceType.Brick]);
    }

    [Fact]
    public void RemoveResources_FewerThanOwn()
    {
        // Arrange
        var player = CreatePlayerWithResources();
        Assert.Equal(3, player.Resources[ResourceType.Ore]);

        // Act
        player.RemoveResources(ResourceType.Ore, 2);

        // Assert
        Assert.Equal(1, player.Resources[ResourceType.Ore]);
    }
    
    [Fact]
    public void RemoveResources_MoreThanOwn()
    {
        // Arrange
        var player = CreatePlayerWithResources();
        Assert.Equal(2, player.Resources[ResourceType.Brick]);

        // Act
        // Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            player.RemoveResources(ResourceType.Brick, 3));

        Assert.Equal("Player doesn't have 3 Brick.", exception.Message);

        // Assert
        Assert.Equal(2, player.Resources[ResourceType.Brick]);
    }

    [Fact]
    public void AssignDevelopmentCard_First()
    {
        // Arrange
        var p1 = Player.CreateTestPlayer("Tim", PlayerColor.Red);
        var dc1 = DevelopmentCardType.Knight;

        // Act
        p1.AssignDevelopmentCard(dc1);

        // Assert
        Assert.Equal(1, p1.DevelopmentCardCount);
        Assert.Single(p1.DevCardsPurchasedThisRound);
        Assert.Contains(dc1, p1.DevCardsPurchasedThisRound);
        Assert.Empty(p1.DevCardsReadyToPlay);
        Assert.Empty(p1.DevCardsPlayed);
    }

    [Fact]
    public void AssignDevelopmentCard_Multiple()
    {
        // Arrange
        var p1 = Player.CreateTestPlayer("Tim", PlayerColor.Red);
        var dc1 = DevelopmentCardType.Knight;
        var dc2 = DevelopmentCardType.Monopoly;

        p1.AssignDevelopmentCard(dc1);
        p1.AssignDevelopmentCard(dc2);
        p1.MakeNewDevelopmentCardsPlayable();

        // Act
        p1.AssignDevelopmentCard(dc1);

        // Assert
        Assert.Single(p1.DevCardsPurchasedThisRound);
        Assert.Contains(dc1, p1.DevCardsPurchasedThisRound);
        Assert.Equal(2, p1.DevCardsReadyToPlay.Count);
        Assert.Contains(dc1, p1.DevCardsReadyToPlay);
        Assert.Contains(dc2, p1.DevCardsReadyToPlay);
        Assert.Empty(p1.DevCardsPlayed);
        Assert.Equal(3, p1.DevelopmentCardCount);
    }

    [Fact]
    public void AssignDevelopmentCard_PlayedNotInCount()
    {
        // Arrange
        var p1 = Player.CreateTestPlayer("Tim", PlayerColor.Red);
        var dc1 = DevelopmentCardType.Knight;
        var dc2 = DevelopmentCardType.Monopoly;

        p1.AssignDevelopmentCard(dc1);
        p1.AssignDevelopmentCard(dc2);
        p1.AssignDevelopmentCard(dc1);
        p1.MakeNewDevelopmentCardsPlayable();
        p1.PlayDevelopmentCard(dc1);
        p1.PlayDevelopmentCard(dc1);

        // Act
        p1.AssignDevelopmentCard(dc2);

        // Assert
        Assert.Single(p1.DevCardsPurchasedThisRound);
        Assert.Contains(dc2, p1.DevCardsPurchasedThisRound);
        Assert.Single(p1.DevCardsReadyToPlay);
        Assert.Contains(dc2, p1.DevCardsReadyToPlay);
        Assert.Equal(2, p1.DevCardsPlayed.Count);
        Assert.Contains(dc1, p1.DevCardsPlayed);
        Assert.Equal(2, p1.DevelopmentCardCount);
    }


    [Fact]
    public void PlayDevelopmentCard_PlayerDoesntHave()
    {
        // Arrange
        var p1 = Player.CreateTestPlayer("Tim", PlayerColor.Red);
        var dc1 = DevelopmentCardType.Knight;
        p1.AssignDevelopmentCard(dc1);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => p1.PlayDevelopmentCard(DevelopmentCardType.Monopoly));
    }

    [Fact]
    public void PlayDevelopmentCard_NotPlayableYet()
    {
        // Arrange
        var p1 = Player.CreateTestPlayer("Tim", PlayerColor.Red);
        var dc1 = DevelopmentCardType.Knight;
        p1.AssignDevelopmentCard(dc1);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => p1.PlayDevelopmentCard(DevelopmentCardType.Knight));
    }

    [Fact]
    public void PlayDevelopmentCard_Knight()
    {
        // Arrange
        var p1 = Player.CreateTestPlayer("Tim", PlayerColor.Red);
        var dc1 = DevelopmentCardType.Knight;
        p1.AssignDevelopmentCard(dc1);
        p1.MakeNewDevelopmentCardsPlayable();
        Assert.Equal(1, p1.DevelopmentCardCount);

        // Act
        p1.PlayDevelopmentCard(DevelopmentCardType.Knight);

        // Assert
        Assert.Empty(p1.DevCardsPurchasedThisRound);
        Assert.Empty(p1.DevCardsReadyToPlay);
        Assert.Single(p1.DevCardsPlayed);
        Assert.Contains(dc1, p1.DevCardsPlayed);
        Assert.Equal(0, p1.DevelopmentCardCount);
    }

    [Fact]
    public void PlayDevelopmentCard_Monopoly()
    {
        // Arrange
        var p1 = Player.CreateTestPlayer("Tim", PlayerColor.Red);
        var dc1 = DevelopmentCardType.Knight;
        p1.AssignDevelopmentCard(dc1);
        var dc2 = DevelopmentCardType.Monopoly;
        p1.AssignDevelopmentCard(dc2);
        p1.MakeNewDevelopmentCardsPlayable();

        // Act
        p1.PlayDevelopmentCard(DevelopmentCardType.Monopoly);

        // Assert
        Assert.Empty(p1.DevCardsPurchasedThisRound);
        Assert.Single(p1.DevCardsReadyToPlay);
        Assert.Contains(dc1, p1.DevCardsReadyToPlay);
        Assert.Empty(p1.DevCardsPlayed);
        Assert.Equal(1, p1.DevelopmentCardCount);
    }

    [Fact]
    public void PlayDevelopmentCard_RoadBuilding()
    {
        // Arrange
        var p1 = Player.CreateTestPlayer("Tim", PlayerColor.Red);
        var dc1 = DevelopmentCardType.RoadBuilding;
        p1.AssignDevelopmentCard(dc1);
        p1.MakeNewDevelopmentCardsPlayable();

        // Act
        p1.PlayDevelopmentCard(DevelopmentCardType.RoadBuilding);

        // Assert
        Assert.Empty(p1.DevCardsPurchasedThisRound);
        Assert.Empty(p1.DevCardsReadyToPlay);
        Assert.Empty(p1.DevCardsPlayed);
        Assert.Equal(0, p1.DevelopmentCardCount);
    }

    [Fact]
    public void PlayDevelopmentCard_YearOfPlenty()
    {
        // Arrange
        var p1 = Player.CreateTestPlayer("Tim", PlayerColor.Red);
        var dc1 = DevelopmentCardType.YearOfPlenty;
        p1.AssignDevelopmentCard(dc1);
        p1.MakeNewDevelopmentCardsPlayable();

        // Act
        p1.PlayDevelopmentCard(DevelopmentCardType.YearOfPlenty);

        // Assert
        Assert.Empty(p1.DevCardsPurchasedThisRound);
        Assert.Empty(p1.DevCardsReadyToPlay);
        Assert.Empty(p1.DevCardsPlayed);
        Assert.Equal(0, p1.DevelopmentCardCount);
    }

    [Fact]
    public void PlayDevelopmentCard_VictoryPoint()
    {
        // Arrange
        var p1 = Player.CreateTestPlayer("Tim", PlayerColor.Red);
        var dc1 = DevelopmentCardType.Knight;
        p1.AssignDevelopmentCard(dc1);
        var dc2 = DevelopmentCardType.VictoryPoint;
        p1.AssignDevelopmentCard(dc2);
        p1.MakeNewDevelopmentCardsPlayable();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => p1.PlayDevelopmentCard(DevelopmentCardType.VictoryPoint));
    }

    [Fact]
    public void SetVictoryPoints()
    {
        var player = Player.CreateTestPlayer("Tim", PlayerColor.Red);
        player.SetVictoryPoints(4, 5);
        Assert.Equal(5, player.FullVictoryPoints);
        Assert.Equal(4, player.VisibleVictoryPoints);
    }
}