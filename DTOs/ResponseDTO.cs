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

    public ResponseDTO(bool success, int errorCode, string errorParmValues, GameState? gameState) : 
            this(success, errorCode, errorParmValues, gameState != null ? new GameStateDTO(gameState) : null)
    {
    }

    public ResponseDTO(bool success, int errorCode, string errorParmValues, GameStateDTO? gameStateDTO)
    {
        Success = success;
        if (success)
        {
            if (gameStateDTO == null)
                throw new ArgumentNullException("gameState");

            GameState = gameStateDTO;
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
        { 1003, "Game state does not support the requested action."},
        { 1004, "Request to create game must indicate gameType"}, 
        { 1005, "Request to build road must specify player id and edge id'."},
        { 1006, "Player does not have the resources offered."},
        { 1007, "Bank only accepts trades of single resource types."},
        { 1008, "Bank requires more resources for the resource type offered."},
        { 1009, "Cannot trade the resource for itself."},
        { 1010, "Bank doesn't accept trades requesting more than one resource."},
        { 1011, "It is not player's turn."},
        { 1012, "Invalid player id."},
        { 1013, "Invalid edge id."},
        { 1014, "Invalid vertex id."},
        { 1015, "Edge not adjacent to a city/settlement for the identified player."},
        { 1016, "Edge already has a road."},
        { 1017, "Player does not have the required resources."}, 
        { 1018, "Vertex already has a settlement."},
        { 1019, "Vertex already has a city."},
        { 1020, "Vertex does not have a settlement to upgrade."},
        { 1021, "Vertex already occupied by another player."},
        { 1022, "Vertex is too close to another development."}, 
        { 1023, "Vertex not adjacent to one of the player's roads"},
        { 1024, "Request to build a settlement must specify player id and vertex id'."},
        { 1026, "Request for a bank trade must specify player id, request, and offer'."},
        { 1027, "Could not build road (game/player/edge missing or edge occupied)."},
        { 9999, "Unexpected error."},

    };
}
