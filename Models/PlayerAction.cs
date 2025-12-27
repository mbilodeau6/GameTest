namespace GameTest.Models;

public enum PlayerAction
{
    RollDice,
    PlaceSettlement,
    UpgradeSettlement,
    PlaceRoad,
    PlaceRobber,
    SelectTarget,
    DiscardCards,
    PlayMonopoly,
    PlayKnight,
    PlayYearOfPlenty,
    PlayRoadBuilding,
    BuyDevelopmentCard,
    TradeWithBank,
    TradeWithPlayers,
    RespondToTrade,
    AcceptTrade,
    RejectAllOffers,
    EndTurn,
    Undo // TODO: Will the BE always know when Undo is possible (or not)?
}
