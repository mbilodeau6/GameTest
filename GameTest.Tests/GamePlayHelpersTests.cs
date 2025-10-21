using Xunit;
using GameTest.Models;
using GameTest.DTOs;
using GameTest.Services;

namespace GameTest.Tests;

public class GamePlayHelpersTests
{
    [Fact]
    public void GetResourcesEarnedOnLastRoll_NoBuildingsOnMatchingTiles()
    {
        // Arrage
        var gameState = BoardCreationHelpers.CreateNewBoard(GameType.Test);
        gameState.SetDiceForTesting(new GameDice(new GameDie(3), new GameDie(5)));

        // Act
        var resources = GamePlayHelpers.GetResourcesEarnedOnLastRoll(gameState);

        // Assert
        Assert.Empty(resources);
    }

    [Fact]
    public void GetResourcesEarnedOnLastRoll_BuildingsOnMatchingTiles()
    {
        // Arrage
        var gameState = BoardCreationHelpers.CreateNewBoard(GameType.Test);

        var targetTile = BoardCreationHelpers.GetRequiredTileAt(gameState.Tiles, 0, 0);
        Assert.NotNull(targetTile);
        Assert.Equal(9, targetTile.DiceNumber);

        var bluePlayer = gameState.Players.First(p => p.Color == PlayerColor.Blue);
        Assert.NotNull(bluePlayer);
        var redPlayer = gameState.Players.First(p => p.Color == PlayerColor.Red);
        Assert.NotNull(redPlayer);

        var blueVertex = gameState.Vertices.First(v => v.Tiles.Contains(targetTile) && v.Owner == bluePlayer);
        Assert.NotNull(blueVertex);
        Assert.Equal(BuildingType.Settlement, blueVertex.Building);
        blueVertex.UpgradeToCity();

        gameState.SetDiceForTesting(new GameDice(new GameDie(4), new GameDie(5)));

        // Act
        var resources = GamePlayHelpers.GetResourcesEarnedOnLastRoll(gameState);

        // Assert
        Assert.NotEmpty(resources);
        Assert.True(resources.ContainsKey(bluePlayer));
        Assert.True(resources.ContainsKey(redPlayer));
        Assert.True(resources[redPlayer].ContainsKey(ResourceType.Grain));
        Assert.Equal(1, resources[redPlayer][ResourceType.Grain]);
        Assert.True(resources[bluePlayer].ContainsKey(ResourceType.Grain));
        Assert.Equal(2, resources[bluePlayer][ResourceType.Grain]);
    }

    [Fact]
    public void GetVictoryPointsForBuild_ForSettlement()
    {
        // Arrange
        // Act
        // Assert
        Assert.Equal(1, GamePlayHelpers.GetVictoryPointsForBuild(BuildingType.Settlement));
    }

    [Fact]
    public void GetVictoryPointsForBuild_ForCity()
    {
        // Arrange
        // Act
        // Assert
        Assert.Equal(2, GamePlayHelpers.GetVictoryPointsForBuild(BuildingType.City));
    }

    [Fact]
    public void AssignResourcesToPlayers()
    {
        // Arrange
        GameState gameState = new GameState(new Guid());

        var player1 = new Player("Fred", PlayerColor.Blue);
        var player2 = new Player("Marge", PlayerColor.Orange);
        gameState.AddPlayer(player1);
        gameState.AddPlayer(player2);

        Dictionary<Player, Dictionary<ResourceType, int>> resources = new Dictionary<Player, Dictionary<ResourceType, int>>();
        resources.Add(player1, new Dictionary<ResourceType, int>());
        resources[player1].Add(ResourceType.Ore, 4);
        resources.Add(player2, new Dictionary<ResourceType, int>());
        resources[player2].Add(ResourceType.Wood, 1);
        resources[player2].Add(ResourceType.Brick, 2);

        // Act
        GamePlayHelpers.AssignResourcesToPlayers(gameState, resources);

        // Assert
        Assert.Equal(4, gameState.Players[0].Resources[ResourceType.Ore]);
        Assert.Equal(0, gameState.Players[0].Resources[ResourceType.Brick]);
        Assert.Equal(0, gameState.Players[0].Resources[ResourceType.Wood]);
        Assert.Equal(4, gameState.Players[0].ResourceCount);
        Assert.Equal(0, gameState.Players[1].Resources[ResourceType.Ore]);
        Assert.Equal(2, gameState.Players[1].Resources[ResourceType.Brick]);
        Assert.Equal(1, gameState.Players[1].Resources[ResourceType.Wood]);
        Assert.Equal(3, gameState.Players[1].ResourceCount);
    }

    private Player CreatePlayerWithSufficientResources()
    {
        var player = new Player("Mary", PlayerColor.Red);

        player.Resources[ResourceType.Wood] = 1;
        player.Resources[ResourceType.Brick] = 1;
        player.Resources[ResourceType.Grain] = 2;
        player.Resources[ResourceType.Wool] = 1;
        player.Resources[ResourceType.Ore] = 3;

        return player;
    }

    private Player CreatePlayerWithInsufficientResources()
    {
        var player = new Player("Mary", PlayerColor.Red);

        player.Resources[ResourceType.Wood] = 1;
        player.Resources[ResourceType.Brick] = 0;
        player.Resources[ResourceType.Grain] = 2;
        player.Resources[ResourceType.Wool] = 0;
        player.Resources[ResourceType.Ore] = 2;

        return player;
    }

    [Fact]
    public void WithdrawResourcesToBuildRoad_SufficientResources()
    {
        // Arrange
        var player = CreatePlayerWithSufficientResources();
        var woodCount = player.Resources[ResourceType.Wood];
        var brickCount = player.Resources[ResourceType.Brick];

        // Act
        var result = GamePlayHelpers.WithdrawResourcesToBuildRoad(player);

        // Assert
        Assert.True(result);
        Assert.Equal(woodCount - 1, player.Resources[ResourceType.Wood]);
        Assert.Equal(brickCount - 1, player.Resources[ResourceType.Brick]);
    }

    [Fact]
    public void WithdrawResourcesToBuildRoad_InsufficientResources()
    {
        // Arrange
        var player = CreatePlayerWithInsufficientResources();
        var woodCount = player.Resources[ResourceType.Wood];
        var brickCount = player.Resources[ResourceType.Brick];

        // Act
        var result = GamePlayHelpers.WithdrawResourcesToBuildRoad(player);

        // Assert
        Assert.False(result);
        Assert.Equal(woodCount, player.Resources[ResourceType.Wood]);
        Assert.Equal(brickCount, player.Resources[ResourceType.Brick]);
    }

    [Fact]
    public void WithdrawResourcesToBuildSettlement_SufficientResources()
    {
        // Arrange
        var player = CreatePlayerWithSufficientResources();
        var woodCount = player.Resources[ResourceType.Wood];
        var brickCount = player.Resources[ResourceType.Brick];
        var woolCount = player.Resources[ResourceType.Wool];
        var grainCount = player.Resources[ResourceType.Grain];

        // Act
        var result = GamePlayHelpers.WithdrawResourcesToBuildSettlement(player);

        // Assert
        Assert.True(result);
        Assert.Equal(woodCount - 1, player.Resources[ResourceType.Wood]);
        Assert.Equal(brickCount - 1, player.Resources[ResourceType.Brick]);
        Assert.Equal(woolCount - 1, player.Resources[ResourceType.Wool]);
        Assert.Equal(grainCount - 1, player.Resources[ResourceType.Grain]);
    }

    [Fact]
    public void WithdrawResourcesToBuildSettlement_InufficientResources()
    {
        // Arrange
        var player = CreatePlayerWithInsufficientResources();
        var woodCount = player.Resources[ResourceType.Wood];
        var brickCount = player.Resources[ResourceType.Brick];
        var woolCount = player.Resources[ResourceType.Wool];
        var grainCount = player.Resources[ResourceType.Grain];

        // Act
        var result = GamePlayHelpers.WithdrawResourcesToBuildSettlement(player);

        // Assert
        Assert.False(result);
        Assert.Equal(woodCount, player.Resources[ResourceType.Wood]);
        Assert.Equal(brickCount, player.Resources[ResourceType.Brick]);
        Assert.Equal(woolCount, player.Resources[ResourceType.Wool]);
        Assert.Equal(grainCount, player.Resources[ResourceType.Grain]);
    }

    [Fact]
    public void WithdrawResourcesToBuildCity_SufficientResources()
    {
        // Arrange
        var player = CreatePlayerWithSufficientResources();
        var oreCount = player.Resources[ResourceType.Ore];
        var grainCount = player.Resources[ResourceType.Grain];

        // Act
        var result = GamePlayHelpers.WithdrawResourcesToBuildCity(player);

        // Assert
        Assert.True(result);
        Assert.Equal(oreCount - 3, player.Resources[ResourceType.Ore]);
        Assert.Equal(grainCount - 2, player.Resources[ResourceType.Grain]);
    }

    [Fact]
    public void WithdrawResourcesToBuildCity_InsufficientResources()
    {
        // Arrange
        var player = CreatePlayerWithInsufficientResources();
        var oreCount = player.Resources[ResourceType.Ore];
        var grainCount = player.Resources[ResourceType.Grain];

        // Act
        var result = GamePlayHelpers.WithdrawResourcesToBuildCity(player);

        // Assert
        Assert.False(result);
        Assert.Equal(oreCount, player.Resources[ResourceType.Ore]);
        Assert.Equal(grainCount, player.Resources[ResourceType.Grain]);
    }

    [Fact]
    public void WithdrawResourcesToBuyDevCard_SufficientResources()
    {
        // Arrange
        var player = CreatePlayerWithSufficientResources();
        var oreCount = player.Resources[ResourceType.Ore];
        var woolCount = player.Resources[ResourceType.Wool];
        var grainCount = player.Resources[ResourceType.Grain];

        // Act
        var result = GamePlayHelpers.WithdrawResourcesToBuyDevCard(player);

        // Assert
        Assert.True(result);
        Assert.Equal(oreCount - 1, player.Resources[ResourceType.Ore]);
        Assert.Equal(woolCount - 1, player.Resources[ResourceType.Wool]);
        Assert.Equal(grainCount - 1, player.Resources[ResourceType.Grain]);
    }

    [Fact]
    public void WithdrawResourcesToBuyDevCard_InsufficientResources()
    {
        // Arrange
        var player = CreatePlayerWithInsufficientResources();
        var oreCount = player.Resources[ResourceType.Ore];
        var woolCount = player.Resources[ResourceType.Wool];
        var grainCount = player.Resources[ResourceType.Grain];

        // Act
        var result = GamePlayHelpers.WithdrawResourcesToBuyDevCard(player);

        // Assert
        Assert.False(result);
        Assert.Equal(oreCount, player.Resources[ResourceType.Ore]);
        Assert.Equal(woolCount, player.Resources[ResourceType.Wool]);
        Assert.Equal(grainCount, player.Resources[ResourceType.Grain]);
    }

    [Fact]
    public void GetNextPhase_PlaySetup_MoveToNextPlayer()
    {

    }

    [Fact]
    public void GetNextPhase_PlaySetup_MoveToSetupSettlementAsc()
    {

    }

    [Fact]
    public void GetNextPhase_SetupSettlementAsc_MoveToSetupRoadAsc()
    {

    }

    [Fact]
    public void GetNextPhase_SetupRoadAsc_MoveToNextPlayer()
    {

    }

    [Fact]
    public void GetNextPhase_SetupRoadAsc_MoveToSetupSettlementDesc()
    {

    }

    [Fact]
    public void GetNextPhase_SetupSettlement_MoveToSetupRoadDesc()
    {

    }

    [Fact]
    public void GetNextPhase_SetupRoadDesc_MoveToNextPlayer()
    {

    }

    [Fact]
    public void GetNextPhase_SetupRoadDesc_MoveToPreRoll()
    {

    }

    [Fact]
    public void GetNextPhase_PreRoll_MoveToPostRoll()
    {

    }

    [Fact]
    public void GetNextPhase_PostRoll_MoveToNextPlayer()
    {

    }

    [Fact]
    public void GetNextPhase_PostRoll_MoveToGameOver()
    {

    }


}
