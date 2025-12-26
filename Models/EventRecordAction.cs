namespace GameTest.Models;

public enum EventRecordAction
{
    RollDice,
    PlaceSettlement,
    UpgradeSettlement,
    PlaceRoad,
    PlaceRobber,
    StealResource, // Shouldn't ever be sent to UI as possible move (it happens automatically). Needed for EventRecord.
    SelectTarget,
    DiscardCards,
    PlayMonoploy,
    PlayKnight,
    PlayYearOfPlenty,
    PlayRoadBuilding,
    BuyDevelopmentCard,
    TradeWithBank,
    OfferToTrade, 
    AcceptTrade,
    RejectTrade,
    CounterOffer,
    TradeWithPlayer,
    EndTurn, // not used for eventRecord entries but could be
    ReceivedResources
}
