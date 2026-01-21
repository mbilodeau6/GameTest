using GameTest.DTOs;
using GameTest.Models;
using Microsoft.ApplicationInsights.Extensibility.Implementation.ApplicationId;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace GameTest.Services;

public class BotAI
{
    // Temporarily storing the full GameState object. Assume that I'll eventually have a more optimized
    // internal representation.
    private GameState State;
    private Player Bot;

    public BotAI(GameState gs, Player bot)
    {
        if (gs == null)
            throw new ArgumentNullException(nameof(gs));

        if (bot == null)
            throw new ArgumentNullException(nameof(bot));

        if (!bot.IsBot)
            throw new InvalidOperationException("Player must be a Bot.");

        State = gs;
        Bot = bot;
    }

    public BotMove GetSetUpMove()
    {
        if (!GamePlayHelpers.IsPlayerSetupPhase(State))
            throw new InvalidOperationException($"GetSetUpMove should only be called if in one of the Place(First/Second)(Settlement/Road) phases. Current phase is {State.Phase.PhaseState.ToString()}");

        if (State.Phase.CurrentPlayer == null || !State.Phase.CurrentPlayer.IsBot)
            throw new InvalidOperationException("Current player must be identified and must be a Bot.");

        var move = new BotMove();

        if (State.Phase.PhaseState == GameStates.PlaceFirstSettlement || State.Phase.PhaseState == GameStates.PlaceSecondSettlement)
        {
            var vertex = new VertexPicker(State).PickVertex();

            if (vertex == null)
                throw new InvalidOperationException("VertexPicker didn't return a vertex.");

            move.VertexMove = new VertexDTO(vertex.Id, BuildingType.Settlement, State.Phase.CurrentPlayer.Id, null);
        }
        else if (State.Phase.PhaseState == GameStates.PlaceFirstRoad || State.Phase.PhaseState == GameStates.PlaceSecondRoad)
        {
            var edge = new VertexPicker(State).PickEdge();

            if (edge == null)
                throw new InvalidOperationException("VertexPicker didn't return an edge.");
                
            move.EdgeMove = new EdgeDTO(edge.Id, State.Phase.CurrentPlayer.Id, null);
        }

        return move;
    }

    private static Dictionary<ResourceType, int> CalculateTradeRates(Player player)
    {
        var tradeRates = new Dictionary<ResourceType, int>();

        foreach (ResourceType rt in Enum.GetValues(typeof(ResourceType)))
        {
            if (rt == ResourceType.Desert)
                continue;

            tradeRates.Add(rt, Bank.GetTradeRate(player, rt));
        }

        return tradeRates;
    }

    // TODO: Add code to factor in probabily of getting each resource when determining which 
    // resources to offer and request (i.e. if you need wool and grain to build a settlement,
    // trade for the resource you are less likely to roll first).
    public (bool CanTrade, TradeRequest? TradeRequest) AnalyzePossibleBankTrades()
    {
        if (State.Phase.CurrentPlayer == null || !State.Phase.CurrentPlayer.IsBot)
            throw new InvalidOperationException("Current player must be identified and must be a Bot.");

        bool couldBuildCity = AIHelpers.GetSettlementToUpgrade(State) != null && State.UnusedCityAvailable(State.Phase.CurrentPlayer);
        bool couldBuildSettlement = AIHelpers.GetVertexReadyForSettlement(State) != null
                && State.UnusedSettlementAvailable(State.Phase.CurrentPlayer);
        bool couldBuildRoad = State.UnusedRoadAvailable(State.Phase.CurrentPlayer);

        ResourceType resourceToTrade = ResourceType.Desert;
        ResourceType resourceToGet = ResourceType.Desert;

        var neededForRoad = AIHelpers.CalculateResourcesNeededForRoad(State.Phase.CurrentPlayer.Resources); 
        int shortForRoad = neededForRoad.Count();
        var neededForSettlement = AIHelpers.CalculateResourcesNeededForSettlement(State.Phase.CurrentPlayer.Resources); 
        int shortForSettlement = neededForSettlement.Count();
        var neededForCity = AIHelpers.CalculateResourcesNeededForCity(State.Phase.CurrentPlayer.Resources);
        int shortForCity = (neededForCity.ContainsKey(ResourceType.Ore) ? neededForCity[ResourceType.Ore] : 0)
            + (neededForCity.ContainsKey(ResourceType.Grain) ? neededForCity[ResourceType.Grain] : 0);

        if (shortForRoad == 0 && shortForSettlement == 0 && shortForCity == 0)
            return (false, null);

        var tradeRates = CalculateTradeRates(State.Phase.CurrentPlayer);

        if (couldBuildCity && shortForCity > 0 && (!couldBuildRoad 
            || State.CountSettlementsForPlayer(State.Phase.CurrentPlayer) >= State.Settings.SettlementsPerPlayer
            || (shortForRoad > shortForCity && !couldBuildSettlement)
            || shortForSettlement > shortForCity))
        {
            if (State.Phase.CurrentPlayer.Resources[ResourceType.Wool] >= tradeRates[ResourceType.Wool])
                resourceToTrade = ResourceType.Wool;
            else if (State.Phase.CurrentPlayer.Resources[ResourceType.Wood] >= tradeRates[ResourceType.Wood])
                resourceToTrade = ResourceType.Wood;
            else if (State.Phase.CurrentPlayer.Resources[ResourceType.Brick] >= tradeRates[ResourceType.Brick])
                resourceToTrade = ResourceType.Brick;
            else if (State.Phase.CurrentPlayer.Resources[ResourceType.Grain] >= (tradeRates[ResourceType.Grain] + 2))
                resourceToTrade = ResourceType.Grain;
            else if (State.Phase.CurrentPlayer.Resources[ResourceType.Ore] >= (tradeRates[ResourceType.Ore] + 3))
                resourceToTrade = ResourceType.Ore;

            resourceToGet = neededForCity.First().Key;
        }
        else if (couldBuildSettlement && shortForSettlement > 0)
        {
            if (State.Phase.CurrentPlayer.Resources[ResourceType.Wool] >= (tradeRates[ResourceType.Wool] + 1))
                resourceToTrade = ResourceType.Wool;
            else if (State.Phase.CurrentPlayer.Resources[ResourceType.Wood] >= (tradeRates[ResourceType.Wood] + 1))
                resourceToTrade = ResourceType.Wood;
            else if (State.Phase.CurrentPlayer.Resources[ResourceType.Brick] >= (tradeRates[ResourceType.Brick] + 1))
                resourceToTrade = ResourceType.Brick;
            else if (State.Phase.CurrentPlayer.Resources[ResourceType.Grain] >= (tradeRates[ResourceType.Grain] + 1))
                resourceToTrade = ResourceType.Grain;
            else if (State.Phase.CurrentPlayer.Resources[ResourceType.Ore] >= tradeRates[ResourceType.Ore])
                resourceToTrade = ResourceType.Ore;

            resourceToGet = neededForSettlement.First().Key;
        }
        else if (couldBuildRoad && shortForRoad > 0)
        {
            if (State.Phase.CurrentPlayer.Resources[ResourceType.Wool] >= tradeRates[ResourceType.Wool])
                resourceToTrade = ResourceType.Wool;
            else if (State.Phase.CurrentPlayer.Resources[ResourceType.Wood] >= (tradeRates[ResourceType.Wood] + 1))
                resourceToTrade = ResourceType.Wood;
            else if (State.Phase.CurrentPlayer.Resources[ResourceType.Brick] >= (tradeRates[ResourceType.Brick] + 1))
                resourceToTrade = ResourceType.Brick;
            else if (State.Phase.CurrentPlayer.Resources[ResourceType.Grain] >= tradeRates[ResourceType.Grain])
                resourceToTrade = ResourceType.Grain;
            else if (State.Phase.CurrentPlayer.Resources[ResourceType.Ore] >= tradeRates[ResourceType.Ore])
                resourceToTrade = ResourceType.Ore;

            resourceToGet = neededForSettlement.First().Key;
        }

        if (resourceToTrade != ResourceType.Desert && resourceToGet != ResourceType.Desert)
        {
            return (true, new TradeRequest(State.Phase.CurrentPlayer,
                    new Dictionary<ResourceType, int>() { { resourceToTrade, tradeRates[resourceToTrade] } },
                    new Dictionary<ResourceType, int>() { { resourceToGet, 1 } }));
        }

        return (false, null);
    }

    private BotMove DeterminePreferredMove(bool restrictToHeldResources = true)
    {
        var move = new BotMove();

        if (State.Phase.CurrentPlayer == null)
            throw new InvalidOperationException("CurrentPlayer required.");

        // WIN CONDITION: If bot can win via longest road, prioritize building roads
        if (AIHelpers.ShouldPrioritizeRoadsForWin(State, State.Phase.CurrentPlayer)
            && State.UnusedRoadAvailable(State.Phase.CurrentPlayer)
            && (GamePlayHelpers.HasResourcesToBuildRoad(State.Phase.CurrentPlayer) || !restrictToHeldResources))
        {
            var longestRoadTargets = AIHelpers.GetRankedListOfVertexTargets(State, AIHelpers.GetAllOwnedBuildings(State, State.Phase.CurrentPlayer))
                .OrderByDescending(g => g.OverallScore);

            // Find any vertex that needs roads and build toward it
            var targetWithRoads = longestRoadTargets.FirstOrDefault(v => v.RoadsNeeded > 0 && v.NextEdgeToTarget != null);
            if (targetWithRoads != null)
            {
                move.EdgeMove = new EdgeDTO(targetWithRoads.NextEdgeToTarget!.Id, State.Phase.CurrentPlayer.Id, null);
                return move;
            }
        }

        // First look to see if we can upgrade settlements to a city
        var settlementToUpgrade = AIHelpers.GetSettlementToUpgrade(State);
        if (settlementToUpgrade != null && State.Phase.CurrentPlayer != null
            && State.UnusedCityAvailable(State.Phase.CurrentPlayer)
            && (GamePlayHelpers.HasResourcesToBuildCity(State.Phase.CurrentPlayer) || !restrictToHeldResources))
        {
                move.VertexMove = new VertexDTO(settlementToUpgrade.Id, BuildingType.City, State.Phase.CurrentPlayer.Id, null);
                return move;
        }

        // Determine if there are settlements or roads the bot should work towards
            
        var candidateVertices = AIHelpers.GetRankedListOfVertexTargets(State, AIHelpers.GetAllOwnedBuildings(State, State.Phase.CurrentPlayer)).OrderByDescending(g => g.OverallScore);
        if (candidateVertices.Count() > 0)
        {
            // Next, see if you can build on the most valuable vertex identified
            if (State.UnusedSettlementAvailable(State.Phase.CurrentPlayer)
                && (GamePlayHelpers.HasResourcesToBuildSettlement(State.Phase.CurrentPlayer) || !restrictToHeldResources) 
                && candidateVertices.First().RoadsNeeded == 0)
            {
                move.VertexMove = new VertexDTO(candidateVertices.First().TargetVertex.Id, BuildingType.Settlement, State.Phase.CurrentPlayer.Id, null);
                return move;
            }

            // If there isn't a vertex the Bot can build on (yet), build the next road needed to make that vertex available
            if (State.UnusedRoadAvailable(State.Phase.CurrentPlayer)
                && State.UnusedSettlementAvailable(State.Phase.CurrentPlayer)
                && (GamePlayHelpers.HasResourcesToBuildRoad(State.Phase.CurrentPlayer) || !restrictToHeldResources) 
                && candidateVertices.First().RoadsNeeded > 0)
            {
                var candidateVertex = candidateVertices.FirstOrDefault();
                if (candidateVertex != null && candidateVertex.NextEdgeToTarget != null)
                {
                    move.EdgeMove = new EdgeDTO(candidateVertex.NextEdgeToTarget.Id, State.Phase.CurrentPlayer.Id, null);
                    return move;
                }
            }

            // Notice: If the highest priority vertex is available and requires no roads but the Bot doesn't have the resources to
            // build a settlement, the Bot will NOT build another road. It will keep the resources it could use for a road in case
            // it helps it get a settlement (needed to buy settlement or trade for resources needed).
            // TODO: Need to revisit and set up rules for when the Bot should go ahead and build a road even though it isn't
            // required for the highest value target.
        }

        // Consider buying a dev card if we have resources and it makes sense
        bool spotReadyForSettlement = candidateVertices.Any() && candidateVertices.First().RoadsNeeded == 0;
        if (AIHelpers.ShouldBuyDevelopmentCard(State, State.Phase.CurrentPlayer, spotReadyForSettlement) >= 0.5)
        {
            move.BuyDevelopmentCard = true;
            return move;
        }

        return move;        
    }

    public BotMove GetBuildMove()
    {
        if (State.Phase.PhaseState != GameStates.BuildOrTrade)
            throw new InvalidOperationException($"GetBuildMove should only be called if phase is BuildOrTrade. Current phase is {State.Phase.PhaseState.ToString()}");

        if (State.Phase.CurrentPlayer == null || !State.Phase.CurrentPlayer.IsBot)
            throw new InvalidOperationException("Current player must be identified and must be a Bot.");

        var move = new BotMove();

        // Check dev cards that should be played before attempting to build
        var cardToPlay = AIHelpers.GetDevCardToPlay(State, State.Phase.CurrentPlayer);
        if (cardToPlay == DevelopmentCardType.RoadBuilding)
        {
            // Road Building: play to get free roads and save resources for settlement
            move.PlayDevelopmentCard = DevelopmentCardType.RoadBuilding;
            return move;
        }
        else if (cardToPlay == DevelopmentCardType.YearOfPlenty)
        {
            // Year of Plenty: get 2 resources to complete a build
            move.PlayDevelopmentCard = DevelopmentCardType.YearOfPlenty;
            move.YearOfPlentyResources = AIHelpers.GetYearOfPlentyResources(State, State.Phase.CurrentPlayer);
            return move;
        }
        else if (cardToPlay == DevelopmentCardType.Monopoly)
        {
            // Monopoly: steal all of one resource type from opponents
            move.PlayDevelopmentCard = DevelopmentCardType.Monopoly;
            move.MonopolyTarget = AIHelpers.GetMonopolyTarget(State, State.Phase.CurrentPlayer);
            return move;
        }
        else if (cardToPlay == DevelopmentCardType.Knight)
        {
            // Knight: move robber (for Largest Army or to move robber off bot's tile)
            move.PlayDevelopmentCard = DevelopmentCardType.Knight;
            move.TileMove = new TileDTO(AIHelpers.PickTargetForRobber(State, State.Phase.CurrentPlayer));
            return move;
        }

        move = DeterminePreferredMove();
        if (move.VertexMove != null || move.EdgeMove != null || move.BuyDevelopmentCard)
            return move;

        var tradeAnalysis = AnalyzePossibleBankTrades();
        if (tradeAnalysis.CanTrade && tradeAnalysis.TradeRequest != null)
        {
            move.BankTrade = new TradeRequestDTO(tradeAnalysis.TradeRequest);
            return move;
        }

        // Try to initiate a player trade if we can't build or bank trade
        var initiatedTrade = GetInitiatedTrade();
        if (initiatedTrade != null)
        {
            move.InitiateTrade = initiatedTrade;
            return move;
        }

        move.EndTurn = true;
        return move;
    }

    public BotMove GetDevCardRoadMove()
    {
        if (State.Phase.PhaseState != GameStates.FirstDevCardRoad && State.Phase.PhaseState != GameStates.SecondDevCardRoad)
            throw new InvalidOperationException($"GetDevCardRoadMove should only be called if phase is FirstDevCardRoad or SecondDevCardRoad. Current phase is {State.Phase.PhaseState.ToString()}");

        if (State.Phase.CurrentPlayer == null || !State.Phase.CurrentPlayer.IsBot)
            throw new InvalidOperationException("Current player must be identified and must be a Bot.");

        var move = new BotMove();

        // Get ranked vertex targets and pick the edge toward the highest value vertex that still needs roads
        var candidateVertices = AIHelpers.GetRankedListOfVertexTargets(State, AIHelpers.GetAllOwnedBuildings(State, State.Phase.CurrentPlayer))
            .OrderByDescending(g => g.OverallScore)
            .ToList();

        foreach (var candidate in candidateVertices)
        {
            if (candidate.RoadsNeeded > 0 && candidate.NextEdgeToTarget != null)
            {
                move.EdgeMove = new EdgeDTO(candidate.NextEdgeToTarget.Id, State.Phase.CurrentPlayer.Id, null);
                return move;
            }
        }

        // Fallback: if all high-value vertices are reachable, just pick any valid edge
        // This shouldn't normally happen, but provides a safety net
        var validEdges = State.Edges.Where(e => e.Owner == null && e.Vertices.Any(v =>
            v.Owner?.Id == State.Phase.CurrentPlayer.Id ||
            v.Edges.Any(ve => ve.Owner?.Id == State.Phase.CurrentPlayer.Id)));

        var fallbackEdge = validEdges.FirstOrDefault();
        if (fallbackEdge != null)
        {
            move.EdgeMove = new EdgeDTO(fallbackEdge.Id, State.Phase.CurrentPlayer.Id, null);
        }

        return move;
    }

    public BotMove GetPreRollMove()
    {
        if (State.Phase.PhaseState != GameStates.RollOrUseDevCard)
            throw new InvalidOperationException($"GetPreRollMove should only be called if phase is RollOrUseDevCard. Current phase is {State.Phase.PhaseState.ToString()}");

        if (State.Phase.CurrentPlayer == null || !State.Phase.CurrentPlayer.IsBot)
            throw new InvalidOperationException("Current player must be identified and must be a Bot.");

        var move = new BotMove();

        // WIN CONDITION: Play Road Building pre-roll if it would win the game via longest road
        if (State.Phase.CurrentPlayer.DevCardsReadyToPlay.Contains(DevelopmentCardType.RoadBuilding)
            && AIHelpers.CanWinWithRoadBuildingCard(State, State.Phase.CurrentPlayer))
        {
            move.PlayDevelopmentCard = DevelopmentCardType.RoadBuilding;
            return move;
        }

        // Consider playing Knight pre-roll (to move robber before collecting resources)
        var cardToPlay = AIHelpers.GetDevCardToPlay(State, State.Phase.CurrentPlayer);
        if (cardToPlay == DevelopmentCardType.Knight)
        {
            move.PlayDevelopmentCard = DevelopmentCardType.Knight;
            move.TileMove = new TileDTO(AIHelpers.PickTargetForRobber(State, State.Phase.CurrentPlayer));
            return move;
        }

        move.RollDice = true;
        return move;
    }

    public BotMove GetRobberMove()
    {
        if (State.Phase.PhaseState != GameStates.PlaceRobber)
            throw new InvalidOperationException($"GetRobberMove should only be called if phase is PlaceRobber. Current phase is {State.Phase.PhaseState.ToString()}");

        if (State.Phase.CurrentPlayer == null)
            throw new InvalidOperationException("CurrentPlayer required.");

        var move = new BotMove();
        move.TileMove = new TileDTO(AIHelpers.PickTargetForRobber(State, State.Phase.CurrentPlayer));

        return move;
    }

    private GoalWeights ExtractWeightsFromBotMove(BotMove move)
    {
        double settlementWeight = 0.0;
        double cityWeight = 0.0;
        double roadWeight = 0.0;
        double devCardWeight = 0.0;

        if (move.VertexMove != null && move.VertexMove.Building == BuildingType.Settlement)
            settlementWeight = 1.0;

        if (move.VertexMove != null && move.VertexMove.Building == BuildingType.City)
            cityWeight = 1.0;

        if (move.EdgeMove != null)
            roadWeight = 1.0;

        if (move.BuyDevelopmentCard)
            devCardWeight = 1.0;

        return new GoalWeights(settlementWeight, cityWeight, roadWeight, devCardWeight);
    }

    public static List<ResourceType> ResourcePlayerDoesntNeed(List<ResourceType> heldResources, GoalWeights weights)
    {
        var discard = new List<ResourceType>();

        var oreNeeded = 0;
        var grainNeeded = 0;
        var woolNeeded = 0;
        var woodNeeded = 0;
        var brickNeeded = 0;

        if (weights.CityWeight == 1.0)
        {
            oreNeeded = 3;
            grainNeeded = 2;
        }
        else if (weights.SettlementWeight == 1.0)
        {
            woodNeeded = 1;
            brickNeeded = 1;
            grainNeeded = 1;
            woolNeeded = 1;
        }
        else if (weights.RoadWeight == 1.0)
        {
            woodNeeded = 1;
            brickNeeded = 1;
        }
        else if (weights.DevelopmentCardWeight == 1.0)
        {
            oreNeeded = 1;
            grainNeeded = 1;
            woolNeeded = 1;
        }

        var oreCount = heldResources.Count(r => r == ResourceType.Ore);
        var grainCount = heldResources.Count(r => r == ResourceType.Grain);
        var woolCount = heldResources.Count(r => r == ResourceType.Wool);
        var woodCount = heldResources.Count(r => r == ResourceType.Wood);
        var brickCount = heldResources.Count(r => r == ResourceType.Brick);

        if (oreCount > oreNeeded)
        {
            discard.AddRange(heldResources.Where(r => r == ResourceType.Ore).Take(oreCount - oreNeeded));
        }

        if (grainCount > grainNeeded)
        {
            discard.AddRange(heldResources.Where(r => r == ResourceType.Grain).Take(grainCount - grainNeeded));
        }

        if (woolCount > woolNeeded)
        {
            discard.AddRange(heldResources.Where(r => r == ResourceType.Wool).Take(woolCount - woolNeeded));
        }

        if (woodCount > woodNeeded)
        {
            discard.AddRange(heldResources.Where(r => r == ResourceType.Wood).Take(woodCount - woodNeeded));
        }

        if (brickCount > brickNeeded)
        {
            discard.AddRange(heldResources.Where(r => r == ResourceType.Brick).Take(brickCount - brickNeeded));
        }

        return discard;
    }

    public List<ResourceType> DetermineCardsToDiscard()
    {
        if (State.Phase.CurrentPlayer == null)
            throw new InvalidOperationException("CurrentPlayer required.");

        var discard = new List<ResourceType>();
        var resourcesHeld = AIHelpers.ConvertResourceDictToList(State.Phase.CurrentPlayer.Resources);
        int discardCount = (resourcesHeld.Count() / 2);

        // Determine what the bot would like to do
        var wishWeights = ExtractWeightsFromBotMove(DeterminePreferredMove(false));
        var weightsWithActualCards = ExtractWeightsFromBotMove(DeterminePreferredMove(true));

        // Determine which cards aren't needed to do that and discard those first
        discard.AddRange(ResourcePlayerDoesntNeed(resourcesHeld, weightsWithActualCards));

        // If it doesn't have to discard all the cards it doesn't need, keep the most useful cards
        if (discard.Count > discardCount)
            return discard.Take(discardCount).ToList();

        // If the bot still has to discard after that, discard the least useful cards
        var remainingResources = AIHelpers.MultiSetSubtraction(resourcesHeld, discard);
        // TODO: Refine to consider wishWeights, likelihood of rolling resources, ports, etc.
        var orderToDiscardCards = new List<ResourceType>() { ResourceType.Wool, ResourceType.Wood, ResourceType.Brick, ResourceType.Grain, ResourceType.Ore };

        while (discard.Count < discardCount)
        {
            if (orderToDiscardCards.Count == 0)
                throw new InvalidOperationException("Unexpected Error. Program must have a bug as it can't find enough cards to discard.");

            if (remainingResources.Contains(orderToDiscardCards[0]))
                discard.Add(orderToDiscardCards[0]);
            else
                orderToDiscardCards.RemoveAt(0);
        }
        
        return discard;
    }

    public BotMove GetDiscardMove()
    {
        if (State.Phase.PhaseState != GameStates.DiscardCards)
            throw new InvalidOperationException($"Unexpected Error. GetDiscardMove should only be called if phase is DiscardCards. Current phase is {State.Phase.PhaseState.ToString()}");

        var move = new BotMove();
        move.DiscardResources = DetermineCardsToDiscard();

        return move;
    }

    public BotMove SelectTargetMove()
    {
        if (State.Phase.PhaseState != GameStates.SelectTarget)
            throw new InvalidOperationException($"Unexpected Error. SelectPlayerMove should only be called if phase is SelectTarget. Current phase is {State.Phase.PhaseState.ToString()}");

        var maxVictoryPoints = 0;
        foreach (var player in State.Phase.TargetPlayers)
            if (player.VisibleVictoryPoints > maxVictoryPoints)
                maxVictoryPoints = player.VisibleVictoryPoints;

        var playersWithMax = State.Phase.TargetPlayers.Where(p => p.VisibleVictoryPoints == maxVictoryPoints);
        var selectedPlayer = playersWithMax.First();

        if (playersWithMax.Count() > 1)
            selectedPlayer = State.Phase.TargetPlayers[SharedHelpers.NextRandom(State.Phase.TargetPlayers.Count)];

        var move = new BotMove();
        move.SelectedPlayer = selectedPlayer;

        return move;
    }

    /// <summary>
    /// Determines the bot's response to a trade offer.
    /// </summary>
    /// <param name="offer">Resources being offered to the bot</param>
    /// <param name="request">Resources being requested from the bot</param>
    /// <returns>A TradeResponse indicating Accept or Reject</returns>
    public TradeResponse GetTradeResponse(Dictionary<ResourceType, int> offer, Dictionary<ResourceType, int> request)
    {
        bool shouldAccept = AIHelpers.ShouldAcceptTrade(State, Bot, offer, request);

        if (shouldAccept)
            return new TradeResponse(Bot, TradeResponseType.Accept, null, null);
        else
            return new TradeResponse(Bot, TradeResponseType.Reject, null, null);
    }

    /// <summary>
    /// Determines if the bot should initiate a trade and what to offer/request.
    /// Returns null if the bot shouldn't try to trade.
    /// </summary>
    /// <returns>A TradeRequest with the offer/request, or null if shouldn't trade</returns>
    public TradeRequest? GetInitiatedTrade()
    {
        // Check if bot has exceeded max trade attempts (2 per round)
        if (Bot.TradeAttemptsThisRound >= 2)
            return null;

        // Use AIHelpers to determine if we should trade and what to offer
        var tradeOffer = AIHelpers.GetTradeOffer(State, Bot);
        if (tradeOffer == null)
            return null;

        // Check if this exact trade was already attempted this round
        if (Bot.HasAttemptedTrade(tradeOffer.Offer, tradeOffer.Request))
            return null;

        return tradeOffer;
    }
}
