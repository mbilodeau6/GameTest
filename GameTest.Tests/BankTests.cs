using Xunit;
using GameTest.Models;
using GameTest.DTOs;
using GameTest.Services;
using GameTest.Functions;
using Microsoft.VisualStudio.TestPlatform.Common.ExtensionFramework;

namespace GameTest.Tests;

public class BankTests
{
    [Fact]
    public void TradeWithBank_EmptyOffer_TradeRejected()
    {
        // Arrange
        var player = new Player("Alice", PlayerColor.Green);

        var bank = new Bank();
        var offer = new Dictionary<ResourceType, int>();
        var request = new Dictionary<ResourceType, int>
        {
            { ResourceType.Ore, 1 },
        };

        // Act
        var result = bank.TradeWithBank(new GameState(new Guid()), player, offer, request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(1007, result.ErrorCode);
        Assert.Equal(0, player.Resources.GetValueOrDefault(ResourceType.Ore, 0));
    }

    [Fact]
    public void TradeWithBank_EmptyRequest_TradeRejected()
    {
        // Arrange
        var player = new Player("Alice", PlayerColor.Green);
        player.AssignResources(ResourceType.Ore, 4);

        var bank = new Bank();
        var offer = new Dictionary<ResourceType, int>()
        {
            { ResourceType.Ore, 4 },
        };
        var request = new Dictionary<ResourceType, int>();

        // Act
        var result = bank.TradeWithBank(new GameState(new Guid()), player, offer, request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(1007, result.ErrorCode);
        Assert.Equal(4, player.Resources.GetValueOrDefault(ResourceType.Ore, -1));
    }

    [Fact]
    public void TradeWithBank_OfferZero_TradeRejected()
    {
        // Arrange
        var player = new Player("Alice", PlayerColor.Green);
        player.AssignResources(ResourceType.Ore, 4);

        var bank = new Bank();
        var offer = new Dictionary<ResourceType, int>()
        {
            { ResourceType.Ore, 0 },
        };
        var request = new Dictionary<ResourceType, int>();

        // Act
        var result = bank.TradeWithBank(new GameState(new Guid()), player, offer, request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(1007, result.ErrorCode);
        Assert.Equal(4, player.Resources.GetValueOrDefault(ResourceType.Ore, -1));
    }

    [Fact]
    public void TradeWithBank_RequestZero_TradeRejected()
    {
        var player = new Player("Alice", PlayerColor.Green);
        player.AssignResources(ResourceType.Ore, 4);

        var bank = new Bank();
        var offer = new Dictionary<ResourceType, int>();
        var request = new Dictionary<ResourceType, int>()
        {
            { ResourceType.Ore, 0 },
        };

        // Act
        var result = bank.TradeWithBank(new GameState(new Guid()), player, offer, request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(1007, result.ErrorCode);
        Assert.Equal(4, player.Resources.GetValueOrDefault(ResourceType.Ore, -1));
    }

    [Fact]
    public void TradeWithBank_PlayerDoesNotHaveResources_TradeRejected()
    {
        var player = new Player("Alice", PlayerColor.Green);
        player.AssignResources(ResourceType.Ore, 3);

        var bank = new Bank();
        var offer = new Dictionary<ResourceType, int>()
        {
            { ResourceType.Ore, 4 },
        };
        var request = new Dictionary<ResourceType, int>();

        // Act
        var result = bank.TradeWithBank(new GameState(new Guid()), player, offer, request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(1006, result.ErrorCode);
        Assert.Equal(3, player.Resources.GetValueOrDefault(ResourceType.Ore, -1));
    }

    [Fact]
    public void TradeWithBank_PlayerOfferingTooFew_TradeRejected()
    {
        var player = new Player("Alice", PlayerColor.Green);
        player.AssignResources(ResourceType.Ore, 4);

        var bank = new Bank();
        var offer = new Dictionary<ResourceType, int>()
        {
            { ResourceType.Ore, 3 },
        };
        var request = new Dictionary<ResourceType, int>()
        {
            { ResourceType.Wood, 1 },
        };


        // Act
        var result = bank.TradeWithBank(new GameState(new Guid()), player, offer, request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(1008, result.ErrorCode);
        Assert.Equal(4, player.Resources.GetValueOrDefault(ResourceType.Ore, -1));
    }

    [Fact]
    public void TradeWithBank_PlayerOfferingTooMany_TradeRejected()
    {
        var player = new Player("Alice", PlayerColor.Green);
        player.AssignResources(ResourceType.Ore, 5);

        var bank = new Bank();
        var offer = new Dictionary<ResourceType, int>()
        {
            { ResourceType.Ore, 5 },
        };
        var request = new Dictionary<ResourceType, int>()
        {
            { ResourceType.Wood, 1 },
        };

        // Act
        var result = bank.TradeWithBank(new GameState(new Guid()), player, offer, request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(1008, result.ErrorCode);
        Assert.Equal(5, player.Resources.GetValueOrDefault(ResourceType.Ore, -1));
    }

    [Fact]
    public void TradeWithBank_PlayerRequestingMultiple_TradeRejected()
    {
        var player = new Player("Alice", PlayerColor.Green);
        player.AssignResources(ResourceType.Ore, 4);

        var bank = new Bank();
        var offer = new Dictionary<ResourceType, int>()
        {
            { ResourceType.Ore, 4 },
        };
        var request = new Dictionary<ResourceType, int>()
        {
            { ResourceType.Brick, 1 },
            { ResourceType.Wood, 1 },
        };

        // Act
        var result = bank.TradeWithBank(new GameState(new Guid()), player, offer, request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(1007, result.ErrorCode);
        Assert.Equal(4, player.Resources.GetValueOrDefault(ResourceType.Ore, -1));
    }

    [Fact]
    public void TradeWithBank_PlayerOfferingMultiple_TradeRejected()
    {
        var player = new Player("Alice", PlayerColor.Green);
        player.AssignResources(ResourceType.Ore, 3);
        player.AssignResources(ResourceType.Wood, 3);

        var bank = new Bank();
        var offer = new Dictionary<ResourceType, int>()
        {
            { ResourceType.Ore, 2 },
            { ResourceType.Wood, 2 },
        };
        var request = new Dictionary<ResourceType, int>()
        {
            { ResourceType.Brick, 1 },
        };


        // Act
        var result = bank.TradeWithBank(new GameState(new Guid()), player, offer, request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(1007, result.ErrorCode);
        Assert.Equal(3, player.Resources.GetValueOrDefault(ResourceType.Ore, -1));
        Assert.Equal(3, player.Resources.GetValueOrDefault(ResourceType.Wood, -1));
    }

    [Fact]
    public void TradeWithBank_PlayerRequestingTwoResources_TradeRejected()
    {
        var player = new Player("Alice", PlayerColor.Green);
        player.AssignResources(ResourceType.Ore, 4);

        var bank = new Bank();
        var offer = new Dictionary<ResourceType, int>()
        {
            { ResourceType.Ore, 4 },
        };
        var request = new Dictionary<ResourceType, int>()
        {
            { ResourceType.Wood, 1 },
            { ResourceType.Brick, 1 },
        };


        // Act
        var result = bank.TradeWithBank(new GameState(new Guid()), player, offer, request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(1007, result.ErrorCode);
        Assert.Equal(4, player.Resources.GetValueOrDefault(ResourceType.Ore, -1));
    }

    [Fact]
    public void TradeWithBank_PlayerTradingForSame_TradeRejected()
    {
        var player = new Player("Alice", PlayerColor.Green);
        player.AssignResources(ResourceType.Ore, 4);

        var bank = new Bank();
        var offer = new Dictionary<ResourceType, int>()
        {
            { ResourceType.Ore, 4 },
        };
        var request = new Dictionary<ResourceType, int>()
        {
            { ResourceType.Ore, 1 },
        };

        // Act
        var result = bank.TradeWithBank(new GameState(new Guid()), player, offer, request);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(1009, result.ErrorCode);
        Assert.Equal(4, player.Resources.GetValueOrDefault(ResourceType.Ore, -1));
    }

    [Fact]
    public void TradeWithBank_PlayerHasOfferedResources_TradeSuccessful()
    {
        // Arrange
        var player = new Player("Alice", PlayerColor.Green);
        player.AssignResources(ResourceType.Wood, 4);
        player.AssignResources(ResourceType.Grain, 2);
        player.AssignResources(ResourceType.Ore, 2);

        var bank = new Bank();

        var offer = new Dictionary<ResourceType, int>
        {
            { ResourceType.Wood, 4 },
        };

        var request = new Dictionary<ResourceType, int>
        {
            { ResourceType.Ore, 1 },
        };

        // Act
        var result = bank.TradeWithBank(new GameState(new Guid()), player, offer, request);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.GameState);
        Assert.Equal(0, player.Resources.GetValueOrDefault(ResourceType.Wood, 0));
        Assert.Equal(2, player.Resources.GetValueOrDefault(ResourceType.Grain, -1));
        Assert.Equal(3, player.Resources.GetValueOrDefault(ResourceType.Ore, -1));
    }

    [Fact]
    public void GetTradeRate_OrePort()
    {
        var player = new Player("Tim", PlayerColor.Red, false);
        player.AddPort(PortType.Ore);
        var bank = new Bank();

        Assert.Equal(GameSettings.DefaultBankTradeRate, bank.GetTradeRate(player, ResourceType.Brick));
        Assert.Equal(GameSettings.DefaultBankTradeRate, bank.GetTradeRate(player, ResourceType.Wood));
        Assert.Equal(GameSettings.DefaultBankTradeRate, bank.GetTradeRate(player, ResourceType.Wool));
        Assert.Equal(GameSettings.DefaultBankTradeRate, bank.GetTradeRate(player, ResourceType.Grain));
        Assert.Equal(2, bank.GetTradeRate(player, ResourceType.Ore));
    }

    [Fact]
    public void GetTradeRate_ThreeToOneWorksForAll()
    {
        var player = new Player("Tim", PlayerColor.Red, false);
        player.AddPort(PortType.ThreeToOne);
        var bank = new Bank();

        Assert.Equal(3, bank.GetTradeRate(player, ResourceType.Brick));
        Assert.Equal(3, bank.GetTradeRate(player, ResourceType.Wood));
        Assert.Equal(3, bank.GetTradeRate(player, ResourceType.Wool));
        Assert.Equal(3, bank.GetTradeRate(player, ResourceType.Grain));
        Assert.Equal(3, bank.GetTradeRate(player, ResourceType.Ore));
    }

    [Fact]
    public void GetTradeRate_ResourcePortTrumpsThreeToOne()
    {
         var player = new Player("Tim", PlayerColor.Red, false);
        player.AddPort(PortType.ThreeToOne);
        player.AddPort(PortType.Brick);
        player.AddPort(PortType.Wool);
        var bank = new Bank();

        Assert.Equal(2, bank.GetTradeRate(player, ResourceType.Brick));
        Assert.Equal(3, bank.GetTradeRate(player, ResourceType.Wood));
        Assert.Equal(2, bank.GetTradeRate(player, ResourceType.Wool));
        Assert.Equal(3, bank.GetTradeRate(player, ResourceType.Grain));
        Assert.Equal(3, bank.GetTradeRate(player, ResourceType.Ore));
    }
}