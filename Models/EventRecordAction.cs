namespace GameTest.Models;

public enum EventRecordAction
{
    RollDice,
    PlaceSettlement,
    UpgradeSettlement,
    PlaceRoad,
    PlaceRobber,
    StealResource,
    SelectRobberTarget,
    SevenDiscard,
    PlayMonoploy,
    PlayKnight,
    PlayYearOfPlenty,
    PlayRoadBuilding,
    BuyDevelopmentCard,
    TradeWithBank,
    TradeWithPlayer, 
    TradeOfferAccepted // Used if multiple players accepted trade
}
