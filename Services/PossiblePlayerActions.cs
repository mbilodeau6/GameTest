using GameTest.DTOs;
using GameTest.Models;

namespace GameTest.Services;

public static class PossiblePlayerActions
{
    private static bool StateRequiresCurrentPlayer(GameStates phase)
    {
        return phase != GameStates.SettingUpBoard &&
                phase != GameStates.RespondToTrade &&
                phase != GameStates.GameOver;
    }

    private static bool PlayerIsCurrentPlayer(GameState gs, Player player)
    {
        return gs.Phase.CurrentPlayer != null && gs.Phase.CurrentPlayer.Id == player.Id;
    }

    private static bool PlayerWasLastPlayerAndUndoPossible(GameState gs, Player player)
    {
        return gs.UndoState.Count > 0 && 
        gs.UndoState.Last().Phase.CurrentPlayerId != null &&
        gs.UndoState.Last().Phase.CurrentPlayerId == player.Id;
    }

    public static List<PossiblePlayerAction> GetPossiblePlayerActions(GameState gs, Player player)
    {
        var actions = new List<PossiblePlayerAction>();

        // If it's not the player's turn or CurrentPlayer is null, return empty list
        if ((StateRequiresCurrentPlayer(gs.Phase.PhaseState) && !PlayerIsCurrentPlayer(gs, player) && !PlayerWasLastPlayerAndUndoPossible(gs, player)) ||
                player.IsBot)
            return actions;
        var state = gs.Phase.PhaseState;

        // Handle each game state
        if (PlayerIsCurrentPlayer(gs, player))
        {
            switch (state)
            {
                case GameStates.PlaceFirstSettlement:
                case GameStates.PlaceSecondSettlement:
                    actions.Add(GetPlaceSettlementAction(gs, player, state));
                    break;

                case GameStates.PlaceFirstRoad:
                case GameStates.PlaceSecondRoad:
                case GameStates.FirstDevCardRoad:
                case GameStates.SecondDevCardRoad:
                    actions.Add(GetPlaceRoadAction(gs, player, state));
                    break;

                case GameStates.RollOrUseDevCard:
                    actions.Add(new PossiblePlayerAction { Action = PlayerAction.RollDice });
                    AddPlayableDevCardActions(gs, player, actions);
                    break;

                case GameStates.PlaceRobber:
                    var robberTileIds = gs.Tiles.Where(t => t.Id != gs.RobberTile.Id).Select(t => t.Id).ToList();
                    actions.Add(new PossiblePlayerAction { Action = PlayerAction.PlaceRobber, TileIds = robberTileIds });
                    break;

                case GameStates.SelectTarget:
                    if (gs.Phase.TargetPlayers == null)
                        throw new InvalidOperationException("Unexpected Error: TargetPlayers can not be null if GameState is SelectTarget");
                    actions.Add(new PossiblePlayerAction { Action = PlayerAction.SelectTarget, PlayerIds = gs.Phase.TargetPlayers.Select(p => p.Id).ToList()});
                    break;

                case GameStates.DiscardCards:
                    actions.Add(new PossiblePlayerAction { Action = PlayerAction.DiscardCards });
                    break;

                case GameStates.RespondToTrade:
                    // If this player is the one who opened the trade
                    // TODO: Not sure if I need to check that the player is the one who made the Original trade.
                    //       The current player should be the one who made the original.
                    if (gs.Phase.PendingTradeResponses != null &&
                        gs.Phase.PendingTradeResponses.Any(r => r.ResponseType == TradeResponseType.Original && r.Player.Id == player.Id))
                    {
                        var respondingPlayerIds = gs.Phase.PendingTradeResponses
                            .Where(r => r.ResponseType != TradeResponseType.Original && r.ResponseType != TradeResponseType.Reject)
                            .Select(r => r.Player.Id)
                            .ToList();

                        if (respondingPlayerIds.Count > 0)
                            actions.Add(new PossiblePlayerAction { Action = PlayerAction.AcceptTrade, PlayerIds = respondingPlayerIds });

                        actions.Add(new PossiblePlayerAction { Action = PlayerAction.RejectAllOffers });
                    }
                    else
                    {
                        // This player is responding to someone else's trade
                        actions.Add(new PossiblePlayerAction { Action = PlayerAction.RespondToTrade });
                    }
                    break;

                case GameStates.BuildOrTrade:
                    // Building actions
                    if (gs.UnusedSettlementAvailable(player) && GamePlayHelpers.HasResourcesToBuildSettlement(player))
                    {
                        var validVertices = GetValidSettlementVertices(gs, player);
                        if (validVertices.Count > 0)
                            actions.Add(new PossiblePlayerAction { Action = PlayerAction.PlaceSettlement, VertexIds = validVertices });
                    }

                    if (gs.UnusedCityAvailable(player) && GamePlayHelpers.HasResourcesToBuildCity(player))
                    {
                        var validCityVertices = gs.Vertices
                            .Where(v => v.Owner != null && v.Owner.Id == player.Id && v.Building == BuildingType.Settlement)
                            .Select(v => v.Id)
                            .ToList();
                        if (validCityVertices.Count > 0)
                            actions.Add(new PossiblePlayerAction { Action = PlayerAction.UpgradeSettlement, VertexIds = validCityVertices });
                    }

                    if (gs.UnusedRoadAvailable(player) && GamePlayHelpers.HasResourcesToBuildRoad(player))
                    {
                        var validEdges = GetValidRoadEdges(gs, player);
                        if (validEdges.Count > 0)
                            actions.Add(new PossiblePlayerAction { Action = PlayerAction.PlaceRoad, EdgeIds = validEdges });
                    }

                    // Development card actions
                    if (gs.DevelopmentCards.Count > 0 && GamePlayHelpers.HasResourcesToBuyDevCard(player))
                        actions.Add(new PossiblePlayerAction { Action = PlayerAction.BuyDevelopmentCard });

                    AddPlayableDevCardActions(gs, player, actions);

                    // Player trade
                    if (player.ResourceCount > 0)
                        actions.Add(new PossiblePlayerAction { Action = PlayerAction.TradeWithPlayers });

                    // Bank trade
                    bool haveBankRate = false;
                    foreach(var resourceToTrade in player.Resources)
                        if (Bank.GetTradeRate(player, resourceToTrade.Key) <= resourceToTrade.Value)
                        {
                            haveBankRate = true;
                            break;
                        }

                    if (haveBankRate)
                        actions.Add(new PossiblePlayerAction { Action = PlayerAction.TradeWithBank });

                    // Can always end turn in BuildOrTrade
                    actions.Add(new PossiblePlayerAction { Action = PlayerAction.EndTurn });
                    break;

                case GameStates.SettingUpBoard:
                case GameStates.GameOver:
                    // No actions available
                    break;
            }
        }

        if (!PlayerIsCurrentPlayer(gs, player))
        {
            switch (state)
            {
                case GameStates.RespondToTrade:
                    // This player is responding to someone else's trade
                    actions.Add(new PossiblePlayerAction { Action = PlayerAction.RespondToTrade });
                    break;
            }
        }


        if (gs.Phase.CurrentPlayer != null && gs.EventRecord.Count > 0)
        {
            var eventRecordId = gs.EventRecord.Last().Id;          
            if (UndoHelpers.ValidateUndoRequest(gs, new UndoRequest(player.Id, gs.EventRecord.Last().Id)).UndoPossible)
                actions.Add(new PossiblePlayerAction { Action = PlayerAction.Undo, EventId = eventRecordId });
        }

        return actions;
    }

    private static PossiblePlayerAction GetPlaceSettlementAction(GameState gs, Player player, GameStates state)
    {
        var validVertices = GamePlayHelpers.IsPlayerSetupPhase(gs)
            ? gs.Vertices.Where(v => v.Building == null).Select(v => v.Id).ToList()
            : GetValidSettlementVertices(gs, player);

        return new PossiblePlayerAction { Action = PlayerAction.PlaceSettlement, VertexIds = validVertices };
    }

    private static List<string> GetValidSettlementVertices(GameState gs, Player player)
    {
        return gs.Vertices
            .Where(v => v.Building == null && GamePlayHelpers.IsVertexAdjacentToPlayerRoad(gs, v, player))
            .Select(v => v.Id)
            .ToList();
    }

    private static PossiblePlayerAction GetPlaceRoadAction(GameState gs, Player player, GameStates state)
    {
        var validEdges = new List<string>();

        if (state == GameStates.PlaceSecondRoad)
        {
            // Must place adjacent to second settlement
            var targetVertex = GamePlayHelpers.FindSettlementWithNoRoads(gs, player);
            validEdges = targetVertex.Edges
                .Where(e => e.Owner == null)
                .Select(e => e.Id)
                .ToList();
        }
        else
        {
            validEdges = GetValidRoadEdges(gs, player);
        }

        return new PossiblePlayerAction { Action = PlayerAction.PlaceRoad, EdgeIds = validEdges };
    }

    private static List<string> GetValidRoadEdges(GameState gs, Player player)
    {
        return gs.Edges
            .Where(e => e.Owner == null && GamePlayHelpers.IsEdgeAdjacentToPlayerBuild(gs, e, player))
            .Select(e => e.Id)
            .ToList();
    }

    private static void AddPlayableDevCardActions(GameState gs, Player player, List<PossiblePlayerAction> actions)
    {
        if (gs.Phase.DevCardPlayedThisRound)
            return;

        if (player.DevCardsReadyToPlay.Contains(DevelopmentCardType.Knight))
        {
            var tileIds = gs.Tiles.Where(t => t.Id != gs.RobberTile.Id).Select(t => t.Id).ToList();
            actions.Add(new PossiblePlayerAction { Action = PlayerAction.PlayKnight, TileIds = tileIds });
        }

        if (player.DevCardsReadyToPlay.Contains(DevelopmentCardType.Monopoly))
            actions.Add(new PossiblePlayerAction { Action = PlayerAction.PlayMonopoly });

        if (player.DevCardsReadyToPlay.Contains(DevelopmentCardType.YearOfPlenty))
            actions.Add(new PossiblePlayerAction { Action = PlayerAction.PlayYearOfPlenty });

        if (player.DevCardsReadyToPlay.Contains(DevelopmentCardType.RoadBuilding))
            actions.Add(new PossiblePlayerAction { Action = PlayerAction.PlayRoadBuilding });
    }
}