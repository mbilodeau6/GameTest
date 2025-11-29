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
    BuyDevelopmentCard,
    TradeWithBank,
    TradeWithPlayer, 
    TradeOfferAccepted // Used if multiple players accepted trade
}
