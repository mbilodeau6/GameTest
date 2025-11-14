using System.Linq.Expressions;
using GameTest.DTOs;
using GameTest.Models;
using Microsoft.Identity.Client.Extensibility;

namespace GameTest.Services;

public static class AIHelpers
{
    public static double GetProbabilityForDiceRoll(int diceRoll)
    {
        switch (diceRoll)
        {
            case 2:
            case 12:
                return 1.0 / 36.0;
            case 3:
            case 11:
                return 2.0 / 36.0;
            case 4:
            case 10:
                return 3.0 / 36.0;
            case 5:
            case 9:
                return 4.0 / 36.0;
            case 6:
            case 8:
               return 5.0 / 36.0;
            case 7:
                return 6.0 / 36.0;
            default:
                throw new ArgumentOutOfRangeException("diceRoll", "Dice roll must be between 2 and 12.");
        }
    }
}
