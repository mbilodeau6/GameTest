using GameTest.Models;

namespace GameTest.Tests;

public static class TestHelpers
{
    // Validates that the input string starts with the expected letter followed by a positive integer.
    // Example: "T1", "E23", "V5"
    public static bool ValidateId(string input, char expectedLetter)
    {
        if (string.IsNullOrEmpty(input) || input[0] != expectedLetter)
            return false;

        // Check if the rest is a number > 0
        if (input.Length < 2)
            return false;

        string numberPart = input.Substring(1);
        if (int.TryParse(numberPart, out int number))
            return number > 0;

        return false;
    }

    public static bool IsGameStateValid(GameState gameState)
    {
        if (gameState == null)
            throw new ArgumentNullException(nameof(gameState));

        if (gameState.Players == null || gameState.Players.Count < 2 || gameState.Players.Count > 8)
            throw new ArgumentException("Game must have between 2 and 8 players.");

        if (gameState.Tiles == null || gameState.Edges == null || gameState.Vertices == null)
            throw new ArgumentException("GameState must have non-null Tiles, Edges, and Vertices collections.");

        if (gameState.Tiles.Any(t => t == null || t.DiceNumber < 2 || t.DiceNumber > 12))
            throw new ArgumentException("All tiles must be non-null and have a valid DiceNumber (2-12).");

        if (gameState.Edges.Any(e => e == null
            || e.Tiles == null
            || e.Tiles.Count == 0
            || (e.Direction == null && e.Tiles.Count < 2)
            || (e.Direction != null && e.Tiles.Count != 1)))
            throw new ArgumentException("All edges must be non-null and have valid Tiles and Direction.");

        if (gameState.Vertices.Any(v => v == null
            || v.Tiles == null
            || v.Tiles.Count == 0
            || (v.Direction == null && v.Tiles.Count < 2)
            || (v.Direction != null && v.Tiles.Count != 1)
            || (v.Building != null && v.Owner == null)
            || (v.Building == null && v.Owner != null)))
            throw new ArgumentException("All vertices must be non-null and have valid Tiles, Direction, Building, and Owner.");

        if (gameState.RobberTile != null && !gameState.Tiles.Contains(gameState.RobberTile))
            throw new ArgumentException("RobberTile must correspond to an existing tile in the game.");

        return true;
    }
}