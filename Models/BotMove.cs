using Azure.Storage.Blobs.Models;
using GameTest.DTOs;

namespace GameTest.Models;

public class BotMove
{
    public VertexDTO? VertexMove { get; set; }
    public EdgeDTO? EdgeMove { get; set; }
    public DevelopmentCardType? PlayDevelopmentCard { get; set; }
    public bool RollDice { get; set; }
    public bool BuyDevelopmentCard { get; set; }
    
    public bool EndTurn { get; set; }

    // TODO: Placeholder. Assume Trade will need info on players, resources offering, resources requesting
    public bool Trade { get; set; }
}

