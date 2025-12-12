namespace GameTest.Models;

public enum TradeResponseType
{
    Original, // Trade requested by current player. Other players respond to this one.
    Accept,
    Reject,
    Counter,

}