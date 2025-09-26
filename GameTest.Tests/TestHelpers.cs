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
}