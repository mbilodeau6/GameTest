using Azure.Storage.Blobs.Models;
using GameTest.DTOs;

namespace GameTest.Models;

public class BotMove
{
    public VertexDTO? VertexMove { get; set; }
    public EdgeDTO? EdgeMove { get; set; }
    public DevelopmentCardType? PlayDevelopmentCard { get; set; }
    public ResourceType? MonopolyTarget { get; set; }
    public List<ResourceType>? YearOfPlentyResources { get; set; }
    public bool RollDice { get; set; }
    public bool BuyDevelopmentCard { get; set; }
    
    public bool EndTurn { get; set; }

    public TradeRequestDTO? BankTrade { get; set; }
    public TileDTO? TileMove { get; set; }
    public List<ResourceType>? DiscardResources { get; set; }
    public Player? SelectedPlayer { get; set; }
}

