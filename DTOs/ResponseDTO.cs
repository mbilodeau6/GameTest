using Azure;
using GameTest.Models;
using GameTest.Services;
using System.Text;
using System.Text.Json.Serialization;

namespace GameTest.DTOs;

public class ResponseDTO
{
    public bool Success { get; init; }
    public int ErrorCode { get; init; }
    public string ErrorMessage { get; init; }
    public GameStateDTO? GameState { get; init; }
    public List<PossiblePlayerAction> PossibleActions { get; init; }

    /// <summary>
    /// ETag for optimistic concurrency control. Used internally for blob storage updates.
    /// </summary>
    [JsonIgnore]
    public ETag? ETag { get; init; }

    public ResponseDTO(bool success, int errorCode, string errorParmValues, GameState? gameState, Player? player = null, ETag? etag = null)
        : this(success, errorCode, errorParmValues,
               success && gameState != null ? new GameStateDTO(gameState) : null,
               success && gameState != null && player != null ? GamePlayHelpers.GetPossiblePlayerActions(gameState, player) : null,
               etag)
    {
    }

    public ResponseDTO(bool success, int errorCode, string errorParmValues, GameStateDTO? gameStateDTO,
        List<PossiblePlayerAction>? possibleActions = null, ETag? etag = null)
    {
        Success = success;
        ETag = etag;
        if (success)
        {
            if (gameStateDTO == null)
                throw new ArgumentNullException(nameof(gameStateDTO));

            GameState = gameStateDTO;
            ErrorCode = 0;
            ErrorMessage = string.Empty;
            PossibleActions = possibleActions ?? new List<PossiblePlayerAction>();
        }
        else
        {
            GameState = null;
            ErrorCode = errorCode;
            ErrorMessage = GetErrorMessage(errorCode, errorParmValues);
            PossibleActions = new List<PossiblePlayerAction>();
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
        { 1028, "Player has used all of their cities."},
        { 1029, "Player has used all of their settlements."},
        { 1030, "Player has used all of their roads."},
        { 1031, "PlaceRobber request must specify player and tile id."},
        { 1032, "Invalid tile id."},
        { 1033, "Robber can not remain in it's current position. It must be moved to a new spot."},
        { 1034, "BuyDevCard request must specify player."},
        { 1035, "PlayDevCard request must specify player and devCardType."},
        { 1036, "PlayDevCard received a request with an unsupported DevCardType."},
        { 1037, "Monopoly requires the selection of one, and only one, resource"},
        { 1038, "Invalid resource type requested."},
        { 1039, "Player does not have the development card required for this action. Or the card is not playable yet."},
        { 1040, "YearOfPlenty requires the selection of exactly two resources."},
        { 1041, "A play Knight request must specify a tile id."},
        { 1042, "During set up, the second road must be built off of the second settlement not off of the first settlement or first road."},
        { 1043, "Can not play two development cards in the same round."},
        { 1044, "Discard request must specify player and selected resources."},
        { 1045, "Player must discard half (and only half) their resources when a 7 is rolled."},
        { 1046, "TradeResponse must specify offered and requested cards if the response type is Counter. All responses must identify the player."},
        { 1047, "AcceptTrade must specify the player accepting the trade and the player id of the trade being accepted."},
        { 1048, "Trade request must specify cards offered and cards requested."}, 
        { 1049, "Invalid trade. The offer and request are identical."},
        { 1050, "Players can not offer a trade response for their own trades request."},
        { 1051, "Invalid response type for trade response."},
        { 1052, "Trade counter offers must specify the resources offered and requested."},
        { 1053, "A player can not accept their own trade."},
        { 1054, "Can not accept a trade response for a player who did not respond."},
        { 1055, "Can not accept a rejected trade response."},
        { 1056, "Attempt to accept offer from player that doesn't exist in game."},
        { 1057, "Concurrency conflict: game state was modified by another request. Please retry."},
        { 9999, "Unexpected error."},
    };
}
