using Xunit;
using GameTest.Models;
using GameTest.DTOs;

namespace GameTest.Tests;

public class PlayerTests
{
    [Fact]
    public void Constructor_Default_CreatesExpectedPlayer()
    {
        // Act
        var player = new Player();

        // Assert
        Assert.True(TestHelpers.ValidateId(player.Id, 'P'));
        Assert.Equal(string.Empty, player.Name);
        Assert.Equal(PlayerColor.Red, player.Color);
        Assert.All(player.Resources.Values, v => Assert.Equal(0, v));
        Assert.All(player.DevelopmentCards.Values, v => Assert.Equal(0, v));
        Assert.Equal(0, player.DevelopmentCardCount);
        Assert.Equal(0, player.ResourceCount);
        Assert.False(player.IsBot);
    }

    [Fact]
    public void Constructor_SetAll_ValidValues()
    {
        // Arrange
        PlayerColor expectedColor = PlayerColor.Green;
        string expectedName = "Robert";

        // Act
        var player = new Player(expectedName, expectedColor, true);

        // Assert
        Assert.True(TestHelpers.ValidateId(player.Id, 'P'));
        Assert.Equal(expectedName, player.Name);
        Assert.Equal(expectedColor, player.Color);
        Assert.True(player.IsBot);
    }

    [Fact]
    public void Constructor_EmptyName()
    {
        // Arrange
        PlayerColor expectedColor = PlayerColor.Green;
        string expectedName = string.Empty;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new Player(expectedName, expectedColor));

        Assert.Equal("Name cannot be empty (Parameter 'name')", exception.Message);
    }

    [Fact]
    public void Constructor_FromDTO_ValidData()
    {
        // Arrange
        var orig_player = new Player("Mary", PlayerColor.White, true);
        orig_player.AssignDevelopmentCard(DevelopmentCardType.RoadBuilding);
        orig_player.AssignDevelopmentCard(DevelopmentCardType.Knight);
        orig_player.AssignResource(ResourceType.Brick, 2);
        orig_player.AssignResource(ResourceType.Ore, 1);

        var dto = new DTOs.PlayerDTO(orig_player, false);

        // Act
        var new_player = new Player(dto);

        // Assert
        Assert.Equal(orig_player.Id, new_player.Id);
        Assert.Equal(orig_player.Name, new_player.Name);
        Assert.Equal(orig_player.Color, new_player.Color);
        Assert.Equal(2, new_player.DevelopmentCardCount);
        Assert.Equal(1, new_player.DevelopmentCards[DevelopmentCardType.RoadBuilding]);
        Assert.Equal(1, new_player.DevelopmentCards[DevelopmentCardType.Knight]);
        Assert.Equal(0, new_player.DevelopmentCards[DevelopmentCardType.VictoryPoint]);
        Assert.Equal(3, new_player.ResourceCount);
        Assert.Equal(2, new_player.Resources[ResourceType.Brick]);
        Assert.Equal(1, new_player.Resources[ResourceType.Ore]);
        Assert.Equal(0, new_player.Resources[ResourceType.Grain]);
        Assert.True(new_player.IsBot);
    }

    [Fact]
    public void Setters_CanChangeAllButId()
    {
        // Arrange
        PlayerColor expectedColor = PlayerColor.Blue;
        string expectedName = "Alice";

        // Act
        var player = new Player();
        player.Name = expectedName;
        player.Color = expectedColor;

        // Assert
        Assert.True(TestHelpers.ValidateId(player.Id, 'P'));
        Assert.Equal(expectedName, player.Name);
        Assert.Equal(expectedColor, player.Color);
    }

    [Fact]
    public void ToString_Verify()
    {
        // Arrange
        PlayerColor expectedColor = PlayerColor.White;
        string expectedName = "Mary";

        // Act
        var player = new Player(expectedName, expectedColor);

        // Assert
        Assert.Equal("Mary (" + player.Id + ") - White", player.ToString());
    }

    [Fact]
    public void AssignResource_Single()
    {
        // Arrange
        var player = new Player("Mary", PlayerColor.Red);

        // Act
        player.AssignResource(ResourceType.Grain, 1);

        // Assert
        Assert.Equal(1, player.Resources[ResourceType.Grain]);
        Assert.Equal(1, player.ResourceCount);
    }

    [Fact]
    public void AssignResource_Multiple()
    {
        // Arrange
        var player = new Player("Mary", PlayerColor.Red);
        player.AssignResource(ResourceType.Brick, 1);
        player.AssignResource(ResourceType.Ore, 1);

        // Act
        player.AssignResource(ResourceType.Ore, 2);

        // Assert
        Assert.Equal(3, player.Resources[ResourceType.Ore]);
        Assert.Equal(4, player.ResourceCount);
    }

    [Fact]
    public void AssignResource_Desert_ThrowsException()
    {
        // Arrange
        var player = new Player("Mary", PlayerColor.Red);

        // Act
        var exception = Assert.Throws<ArgumentException>(() =>
            player.AssignResource(ResourceType.Desert, 1));

        Assert.Equal("Desert is not a resource that can be earned/owned.", exception.Message);
    }

    [Fact]
    public void AssignDevelopmentCard_FirstCard()
    {
        // Arrange
        var player = new Player("Mary", PlayerColor.Red);

        // Act
        player.AssignDevelopmentCard(DevelopmentCardType.Knight);

        // Assert
        Assert.Equal(1, player.DevelopmentCards[DevelopmentCardType.Knight]);
        Assert.Equal(1, player.DevelopmentCardCount);
    }
    
    [Fact]
    public void AssignDevelopmentCard_AdditionalCard()
    {
        // Arrange
        var player = new Player("Mary", PlayerColor.Red);
        player.AssignDevelopmentCard(DevelopmentCardType.Knight);
        player.AssignDevelopmentCard(DevelopmentCardType.Knight);
        player.AssignDevelopmentCard(DevelopmentCardType.VictoryPoint);

        // Act
        player.AssignDevelopmentCard(DevelopmentCardType.Knight);

        // Assert
        Assert.Equal(3, player.DevelopmentCards[DevelopmentCardType.Knight]);
        Assert.Equal(4, player.DevelopmentCardCount);
    }
}