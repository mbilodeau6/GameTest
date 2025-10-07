using System;
using GameTest.Models;

namespace GameTest.Services;

public class GameService
{
    public GameState CreateGame(string gameType)
    {
        // Minimal behavior: create and return a new GameState with a new GUID
        var gs = new GameState(Guid.NewGuid());
        // TODO: initialize tiles/players based on gameType
        
        return gs;
    }
}