using GameTest.DTOs;
using GameTest.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace GameTest.Services;

public class BotAI
{
    // Temporarily storing the full GameState object. Assume that I'll eventually have a more optimized
    // internal representation.
    private GameState State;

    public BotAI(GameState gs)
    {
        if (gs == null)
            throw new ArgumentNullException("gs");

        if (gs.Phase.CurrentPlayer == null || !gs.Phase.CurrentPlayer.IsBot)
            throw new InvalidOperationException("Current player must be identified and must be a Bot.");

        State = gs;
    }

    public BotMove GetSetUpMove()
    {
        if (!GamePlayHelpers.IsPlayerSetupPhase(State))
            throw new InvalidOperationException($"GetSetUp should only be called if in one of the phases. Current phase is {State.Phase.PhaseState.ToString()}");

        if (State.Phase.CurrentPlayer == null || !State.Phase.CurrentPlayer.IsBot)
            throw new InvalidOperationException("Current player must be identified and must be a Bot.");

        var move = new BotMove();

        if (State.Phase.PhaseState == GameStates.PlaceFirstSettlement || State.Phase.PhaseState == GameStates.PlaceSecondSettlement)
            move.VertexMove = new VertexDTO(new VertexPicker(State).PickVertex().Id, BuildingType.Settlement.ToString(), State.Phase.CurrentPlayer.Id, null);
        else if (State.Phase.PhaseState == GameStates.PlaceFirstRoad || State.Phase.PhaseState == GameStates.PlaceSecondRoad)
            move.EdgeMove = new EdgeDTO(new VertexPicker(State).PickEdge().Id, State.Phase.CurrentPlayer.Id, null);

        return move;
    }

    // TODO: Will need to update when Ports are supported and may want to factor in probabily of
    // getting each resource when determining which resources to offer and request.
    public (bool CanTrade, TradeRequest? TradeRequest) AnalyzePossibleBankTrades()
    {
        if (State.Phase.CurrentPlayer == null || !State.Phase.CurrentPlayer.IsBot)
            throw new InvalidOperationException("Current player must be identified and must be a Bot.");

        bool couldBuildCity = AIHelpers.GetSettlementToUpgrade(State) != null && GamePlayHelpers.UnusedCityAvailable(State, State.Phase.CurrentPlayer);
        bool couldBuildSettlement = AIHelpers.GetVertexReadyForSettlement(State) != null
                && GamePlayHelpers.UnusedSettlementAvailable(State, State.Phase.CurrentPlayer);
        bool couldBuildRoad = GamePlayHelpers.UnusedRoadAvailable(State, State.Phase.CurrentPlayer);

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

        if (couldBuildCity && shortForCity > 0 && (!couldBuildRoad 
            || GamePlayHelpers.CountSettlementsForPlayer(State, State.Phase.CurrentPlayer) >= State.Settings.SettlementsPerPlayer
            || (shortForRoad > shortForCity && !couldBuildSettlement)
            || shortForSettlement > shortForCity))
        {
            if (State.Phase.CurrentPlayer.Resources[ResourceType.Wool] >= 4)
                resourceToTrade = ResourceType.Wool;
            else if (State.Phase.CurrentPlayer.Resources[ResourceType.Wood] >= 4)
                resourceToTrade = ResourceType.Wood;
            else if (State.Phase.CurrentPlayer.Resources[ResourceType.Brick] >= 4)
                resourceToTrade = ResourceType.Brick;
            else if (State.Phase.CurrentPlayer.Resources[ResourceType.Grain] >= 6)
                resourceToTrade = ResourceType.Grain;
            else if (State.Phase.CurrentPlayer.Resources[ResourceType.Ore] >= 7)
                resourceToTrade = ResourceType.Ore;

            resourceToGet = neededForCity.First().Key;
        }
        else if (couldBuildSettlement && shortForSettlement > 0)
        {
            if (State.Phase.CurrentPlayer.Resources[ResourceType.Wool] >= 5)
                resourceToTrade = ResourceType.Wool;
            else if (State.Phase.CurrentPlayer.Resources[ResourceType.Wood] >= 5)
                resourceToTrade = ResourceType.Wood;
            else if (State.Phase.CurrentPlayer.Resources[ResourceType.Brick] >= 5)
                resourceToTrade = ResourceType.Brick;
            else if (State.Phase.CurrentPlayer.Resources[ResourceType.Grain] >= 5)
                resourceToTrade = ResourceType.Grain;
            else if (State.Phase.CurrentPlayer.Resources[ResourceType.Ore] >= 4)
                resourceToTrade = ResourceType.Ore;

            resourceToGet = neededForSettlement.First().Key;
        }
        else if (couldBuildRoad && shortForRoad > 0)
        {
            if (State.Phase.CurrentPlayer.Resources[ResourceType.Wool] >= 4)
                resourceToTrade = ResourceType.Wool;
            else if (State.Phase.CurrentPlayer.Resources[ResourceType.Wood] >= 5)
                resourceToTrade = ResourceType.Wood;
            else if (State.Phase.CurrentPlayer.Resources[ResourceType.Brick] >= 5)
                resourceToTrade = ResourceType.Brick;
            else if (State.Phase.CurrentPlayer.Resources[ResourceType.Grain] >= 4)
                resourceToTrade = ResourceType.Grain;
            else if (State.Phase.CurrentPlayer.Resources[ResourceType.Ore] >= 4)
                resourceToTrade = ResourceType.Ore;

            resourceToGet = neededForSettlement.First().Key;
        }

        if (resourceToTrade != ResourceType.Desert && resourceToGet != ResourceType.Desert)
        {
            return (true, new TradeRequest(State.Phase.CurrentPlayer,
                    new Dictionary<ResourceType, int>() { { resourceToTrade, 4 } },
                    new Dictionary<ResourceType, int>() { { resourceToGet, 1 } }));
        }

        return (false, null);
    }

    public BotMove GetBuildMove()
    {
        if (State.Phase.PhaseState != GameStates.BuildOrTrade)
            throw new InvalidOperationException($"GetBuildMove should only be called if phase is BuildOrTrade. Current phase is {State.Phase.PhaseState.ToString()}");

        if (State.Phase.CurrentPlayer == null || !State.Phase.CurrentPlayer.IsBot)
            throw new InvalidOperationException("Current player must be identified and must be a Bot.");

        var move = new BotMove();

        // First look to see if we can upgrade settlements to a city
        var settlementToUpgrade = AIHelpers.GetSettlementToUpgrade(State);
        if (settlementToUpgrade != null 
            && GamePlayHelpers.UnusedCityAvailable(State, State.Phase.CurrentPlayer) 
            && GamePlayHelpers.HasResourcesToBuildCity(State.Phase.CurrentPlayer))
        {
                move.VertexMove = new VertexDTO(settlementToUpgrade.Id, BuildingType.City.ToString(), State.Phase.CurrentPlayer.Id, null);
                return move;
        }

        // Determine if there are settlements or roads the bot should work towards
        var candidateVertices = AIHelpers.GetRankedListOfVertexTargets(State, AIHelpers.GetAllOwnedBuildings(State, State.Phase.CurrentPlayer)).OrderByDescending(g => g.OverallScore);
        if (candidateVertices.Count() > 0)
        {
            // Next, see if you can build on the most valuable vertex identified
            if (GamePlayHelpers.UnusedSettlementAvailable(State, State.Phase.CurrentPlayer)
                && GamePlayHelpers.HasResourcesToBuildSettlement(State.Phase.CurrentPlayer) 
                && candidateVertices.First().RoadsNeeded == 0)
            {
                move.VertexMove = new VertexDTO(candidateVertices.First().TargetVertex.Id, BuildingType.Settlement.ToString(), State.Phase.CurrentPlayer.Id, null);
                return move;
            }

            // If there isn't a vertex the Bot can build on (yet), build the next road needed to make that vertex available
            if (GamePlayHelpers.UnusedRoadAvailable(State, State.Phase.CurrentPlayer)
                && GamePlayHelpers.UnusedSettlementAvailable(State, State.Phase.CurrentPlayer)
                && GamePlayHelpers.HasResourcesToBuildRoad(State.Phase.CurrentPlayer) 
                && candidateVertices.First().RoadsNeeded > 0)
            {
                move.EdgeMove = new EdgeDTO(candidateVertices.First().NextEdgeToTarget.Id, State.Phase.CurrentPlayer.Id, null);
                return move;
            }

            // Notice: If the highest priority vertex is available and requires no roads but the Bot doesn't have the resources to 
            // build a settlement, the Bot will NOT build another road. It will keep the resources it could use for a road in case
            // it helps it get a settlement (needed to buy settlement or trade for resources needed).
            // TODO: Need to revisit and set up rules for when the Bot should go ahead and build a road even though it isn't
            // required for the highest value target.
        }

        var tradeAnalysis = AnalyzePossibleBankTrades();
        if (tradeAnalysis.CanTrade && tradeAnalysis.TradeRequest != null)
        {
            move.BankTrade = new TradeRequestDTO(tradeAnalysis.TradeRequest);
            return move;
        }

        move.EndTurn = true;
        return move;
    }
    
    public BotMove GetPreRollMove()
    {
        if (State.Phase.PhaseState != GameStates.RollOrUseDevCard)
            throw new InvalidOperationException($"GetPreRollMove should only be called if phase is RollOrUseDevCard. Current phase is {State.Phase.PhaseState.ToString()}");

        var move = new BotMove();
        move.RollDice = true;

        return move;
    }
}
