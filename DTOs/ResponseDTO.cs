using GameTest.Models;
using System.Text;
using System.Text.Json.Serialization;

namespace GameTest.DTOs;

public class ResponseDTO
{
    public bool Success { get; init; }
    public int ErrorCode { get; init; }
    public string ErrorMessage { get; init; }
    public GameStateDTO? GameState { get; init; }

    public ResponseDTO(bool success, int errorCode, string errorParmValues, GameState? gameState)
    {
        Success = success;
        if (success)
        {
            if (gameState == null)
                throw new ArgumentNullException("gameState");

            GameState = new GameStateDTO(gameState);
            ErrorCode = 0;
            ErrorMessage = string.Empty;
        }
        else
        {
            GameState = null;
            ErrorCode = errorCode;
            ErrorMessage = GetErrorMessage(errorCode, errorParmValues);
        }
    }

    private string GetErrorMessage(int errorCode, string errorParmValues)
    {
        StringBuilder sb = new StringBuilder();
        if (!ErrorMessageStrings.ContainsKey(errorCode))
            sb.Append("Unknown error code.");
        else
            sb.Append(ErrorMessageStrings[errorCode]);

        if (!string.IsNullOrWhiteSpace(errorParmValues)) 
        {
            sb.Append(" (");
            sb.Append(errorParmValues);
            sb.Append(")");
        }

        return sb.ToString();
    }
    private Dictionary<int, string> ErrorMessageStrings = new Dictionary<int, string>()
    {
        { 1000, "Invalid game id."},
        { 1001, "Blob container not configured; cannot retrieve game."},
        { 1002, "Unable to retrieve game."},
        { 1003, "Game state does not support the requested action."}

    };
}
