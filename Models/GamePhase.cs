namespace GameTest.Models;
using System.Text.Json.Serialization;
using GameTest.DTOs;
using GameTest.Services;

public class GamePhase
{
    // TODO: Would like to remove this but currently need to set up player order in SettingUpBoard phase.
    private static readonly Random _random = new();

    public GameStates PhaseState { get; set; } = GameStates.SettingUpBoard;
    public Player? CurrentPlayer { get; set; }
    public Player? EndPlayer { get; set; }
    public GameStates? PreviousState {get; private set; } = null;
    public Tile? OriginalRobberTile { get; private set; } = null;
    public int? RoadsPreRoadBuilding { get; private set; } = null;
    public bool WaitingForRoll { get; private set; } = false;
    private int VictoryPointsToWin { get; init; }
    public bool DevCardPlayedThisRound {get; private set; } = false;
    public List<TradeResponse>? PendingTradeResponses { get; private set; }

    // TODO: Can all callers to this version be changed to use the DTO version?
    public GamePhase(GameStates state, int victoryPointsToWin, Player? current = null, Player? end = null)
    {
        PhaseState = state;
        CurrentPlayer = current ?? null;
        EndPlayer = end ?? null;
        VictoryPointsToWin = victoryPointsToWin;
    }

    public GamePhase(GameStates state, Player? current = null, Player? end = null) : this(state, 10, current, end)
    {
    }

    public GamePhase(GameState gs, GamePhaseDTO dto)
    {
        PhaseState = dto.PhaseState;
        if (dto.CurrentPlayerId != null)
            CurrentPlayer = GamePlayHelpers.GetPlayerFromPlayerId(gs, dto.CurrentPlayerId);

        if (dto.EndPlayerId != null)
            EndPlayer = GamePlayHelpers.GetPlayerFromPlayerId(gs, dto.EndPlayerId);

        PreviousState = dto.PreviousState;

        if (dto.OriginalRobberTileId != null)
            OriginalRobberTile = gs.Tiles.First(t => t.Id == dto.OriginalRobberTileId);

        RoadsPreRoadBuilding = dto.RoadsPreRoadBuilding;
        WaitingForRoll = dto.WaitingForRoll;
        DevCardPlayedThisRound = dto.DevCardPlayedThisRound;
        VictoryPointsToWin = gs.Settings.VictoryPointsToWin;

        if (dto.PendingTradeResponses != null)
        {
            PendingTradeResponses = new List<TradeResponse>();
            foreach (var trDto in dto.PendingTradeResponses)
            {
                var player = GamePlayHelpers.GetPlayerFromPlayerId(gs, trDto.PlayerId);
                var tradeResponse = new TradeResponse(player, trDto.ResponseType, trDto.Offer, trDto.Request);
                PendingTradeResponses.Add(tradeResponse);
            }
        }
    }

    // Copy Constructor
    public GamePhase(GamePhase gamePhase)
    {
        PhaseState = gamePhase.PhaseState;
        CurrentPlayer = gamePhase.CurrentPlayer;
        EndPlayer = gamePhase.EndPlayer;
        PreviousState = gamePhase.PreviousState;
        OriginalRobberTile = gamePhase.OriginalRobberTile;
        RoadsPreRoadBuilding = gamePhase.RoadsPreRoadBuilding;
        WaitingForRoll = gamePhase.WaitingForRoll;
        DevCardPlayedThisRound = gamePhase.DevCardPlayedThisRound;
        VictoryPointsToWin = gamePhase.VictoryPointsToWin;
    }
    
    public void SetStateToReturnTo(GameStates state, Tile originalTile)
    {
        if (PreviousState != null)
            throw new InvalidOperationException("Unexpected Error. Call to SetPreRobberState when it is already set.");
            
        PreviousState = state;
        OriginalRobberTile = originalTile;
    }

    public void ClearRobberState()
    {
        PreviousState = null;
        OriginalRobberTile = null;
    }

    public void ClearRoadBuildingState()
    {
        PreviousState = null;
        RoadsPreRoadBuilding = null;
    }

    public void StoreStateDevCardRoadBuilding(GameStates currentState, int roadCount)
    {
        RoadsPreRoadBuilding = roadCount;
        PreviousState = currentState;
    }

    public void SetWaitingForRoll()
    {
        WaitingForRoll = true;
    }

    public void SetDevCardPlayedThisRound()
    {
        DevCardPlayedThisRound = true;
    }

    public void ClearDevCardPlayState()
    {
        DevCardPlayedThisRound = false;
    }

    public void ClearWaitingForRoll()
    {
        WaitingForRoll = false;
    }

    // TODO: Should be private but have public for testing
    public Player GetNextPlayer(Player currentPlayer, List<Player> players)
    {
        int currentPlayerIndex = players.FindIndex(p => p.Id == currentPlayer.Id);
        int nextPlayerIndex = (currentPlayerIndex + 1) % players.Count;

        return players[nextPlayerIndex];
    }

    // TODO: Should be private but have public for testing
    public Player GetPreviousPlayer(Player currentPlayer, List<Player> players)
    {
        int currentPlayerIndex = players.FindIndex(p => p.Id == currentPlayer.Id);
        int previousPlayerIndex = (currentPlayerIndex + players.Count - 1) % players.Count;

        return players[previousPlayerIndex];
    }

    // TODO: Should be private but have public for testing
    public bool PlayerHasWon(Player player)
    {
        return player.VictoryPoints >= VictoryPointsToWin;
    }

    // TODO: Should be private but have public for testing
    // TODO: See note below... If SettingUpBoard is moved out of the game loop, 
    // we could pass List<Player> in the constructor and not have to pass it in
    // with each call to GetNextPhase.
    public GamePhase GetNextPhase(List<Player> players, int playerSettlementCount, int playerRoadCount, int diceValue, Tile robberTile)
    {
        var nextPhase = new GamePhase(this);

        // TODO: Consider moving SettingUpBoard logic outside of GameLoop as 
        // it doesn't have CurrentPlayer. Could set up player order outside
        // of GamePhase.
        if (PhaseState == GameStates.SettingUpBoard)
        {
            nextPhase.PhaseState = GameStates.PlaceFirstSettlement;
            nextPhase.CurrentPlayer = players[_random.Next(players.Count)];
            nextPhase.EndPlayer = GetPreviousPlayer(nextPhase.CurrentPlayer, players);
        }
        else
        {
            if (CurrentPlayer == null)
                throw new InvalidOperationException("CurrentPlayer expected to be set to a valid value.");

            // Also need to check previous player in case they won with their last turn.
            if (PlayerHasWon(CurrentPlayer) || PlayerHasWon(GetPreviousPlayer(CurrentPlayer, players)))
            {
                nextPhase.PhaseState = GameStates.GameOver;
            }
            else if (PhaseState == GameStates.PlaceFirstSettlement)
            {
                if (playerSettlementCount > 0)
                    nextPhase.PhaseState = GameStates.PlaceFirstRoad;
            }
            else if (PhaseState == GameStates.PlaceFirstRoad)
            {
                if (playerRoadCount > 0)
                {
                    if (CurrentPlayer == EndPlayer)
                    {
                        nextPhase.PhaseState = GameStates.PlaceSecondSettlement;
                        nextPhase.EndPlayer = GetNextPlayer(CurrentPlayer, players);
                    }
                    else
                    {
                        nextPhase.PhaseState = GameStates.PlaceFirstSettlement;
                        nextPhase.CurrentPlayer = GetNextPlayer(CurrentPlayer, players);
                    }
                }
            }
            else if (PhaseState == GameStates.PlaceSecondSettlement)
            {
                if (playerSettlementCount > 1)
                    nextPhase.PhaseState = GameStates.PlaceSecondRoad;
            }
            else if (PhaseState == GameStates.PlaceSecondRoad)
            {
                if (playerRoadCount > 1)
                {
                    if (CurrentPlayer == EndPlayer)
                    {
                        nextPhase.PhaseState = GameStates.RollOrUseDevCard;
                        nextPhase.SetWaitingForRoll();
                        nextPhase.EndPlayer = GetPreviousPlayer(CurrentPlayer, players);
                    }
                    else
                    {
                        nextPhase.PhaseState = GameStates.PlaceSecondSettlement;
                        nextPhase.CurrentPlayer = GetPreviousPlayer(CurrentPlayer, players);
                    }
                }
            }
            else if (PhaseState == GameStates.RollOrUseDevCard && !WaitingForRoll)
            {
                if (diceValue == 7)
                {
                    nextPhase.SetStateToReturnTo(GameStates.BuildOrTrade, robberTile);
                    nextPhase.EndPlayer = CurrentPlayer;

                    if (CurrentPlayer.Resources.Values.Sum() > 7)
                        nextPhase.PhaseState = GameStates.DiscardCards;
                    else 
                    {
                        nextPhase.CurrentPlayer = GetNextPlayer(CurrentPlayer, players);
                        while (nextPhase.CurrentPlayer.Id != nextPhase.EndPlayer.Id && nextPhase.CurrentPlayer.Resources.Values.Sum() <= 7)
                            nextPhase.CurrentPlayer = GetNextPlayer(nextPhase.CurrentPlayer, players);

                        if (nextPhase.CurrentPlayer.Id == nextPhase.EndPlayer.Id)
                        {
                            nextPhase.PhaseState = GameStates.PlaceRobber;
                            nextPhase.EndPlayer = GetPreviousPlayer(nextPhase.CurrentPlayer, players);
                        }
                        else
                            nextPhase.PhaseState = GameStates.DiscardCards;
                    }
                }
                else
                    nextPhase.PhaseState = GameStates.BuildOrTrade;
            }
            else if (PhaseState == GameStates.DiscardCards)
            {
                nextPhase.CurrentPlayer = GetNextPlayer(CurrentPlayer, players);
                while (nextPhase.CurrentPlayer.Id != EndPlayer.Id && nextPhase.CurrentPlayer.Resources.Values.Sum() <= 7)
                    nextPhase.CurrentPlayer = GetNextPlayer(nextPhase.CurrentPlayer, players);

                if (nextPhase.CurrentPlayer.Id == EndPlayer.Id)
                    nextPhase.PhaseState = GameStates.PlaceRobber;
                else
                    nextPhase.PhaseState = GameStates.DiscardCards;
            }
            else if (PhaseState == GameStates.PlaceRobber 
                && PreviousState != null 
                && OriginalRobberTile != null && robberTile.Id != OriginalRobberTile.Id)
            {
                nextPhase.PhaseState = (GameStates)PreviousState;
                nextPhase.ClearRobberState();
            }
            else if (PhaseState == GameStates.FirstDevCardRoad
                && playerRoadCount > RoadsPreRoadBuilding)
                nextPhase.PhaseState = GameStates.SecondDevCardRoad;
            else if (PhaseState == GameStates.BuildOrTrade && PendingTradeResponses != null && PendingTradeResponses.Count > 0)
            {
                nextPhase.PhaseState = GameStates.RespondToTrade;
            }
            else if (PhaseState == GameStates.RespondToTrade && (PendingTradeResponses == null || PendingTradeResponses.Count == 0))
            {
                nextPhase.PhaseState = GameStates.BuildOrTrade;
            }
            else if (PhaseState == GameStates.SecondDevCardRoad
                && playerRoadCount > RoadsPreRoadBuilding + 1
                && PreviousState != null)
            {
                nextPhase.PhaseState = (GameStates)PreviousState;
                nextPhase.ClearRoadBuildingState();
            }
        }

        return nextPhase;
    }

    public void AddPendingTradeResponse(TradeResponse tradeResponse)
    {
        if (tradeResponse == null)
            throw new ArgumentNullException(nameof(tradeResponse));

        if (PendingTradeResponses == null)
            PendingTradeResponses = new List<TradeResponse>();

        if (tradeResponse.ResponseType != TradeResponseType.Original && !PendingTradeResponses.Any(tr => tr.ResponseType == TradeResponseType.Original))
            throw new InvalidOperationException("Cannot add a trade response before an original trade request is added.");

        if (tradeResponse.ResponseType == TradeResponseType.Original && PendingTradeResponses.Any(tr => tr.ResponseType == TradeResponseType.Original))
            throw new InvalidOperationException("An original trade request has already been added.");

        if (PendingTradeResponses.Any(tr => tr.Player.Id == tradeResponse.Player.Id))
            PendingTradeResponses.RemoveAll(tr => tr.Player.Id == tradeResponse.Player.Id);

        PendingTradeResponses.Add(tradeResponse);
    }

    public void ClearPendingTradeResponses()
    {
        PendingTradeResponses = null;
    }
}