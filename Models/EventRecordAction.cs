namespace GameTest.Models;

public enum EventRecordAction
{
    RollDice,
    PlaceSettlement,
    UpgradeSettlement,
    PlaceRoad,
    PlaceRobber,
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
