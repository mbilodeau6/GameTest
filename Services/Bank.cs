using GameTest.DTOs;
using GameTest.Models;

namespace GameTest.Services;

public class Bank
{
    public Dictionary<ResourceType, int> Resources { get; set; }= new Dictionary<ResourceType, int>()
    {
        { ResourceType.Brick, 19 },
        { ResourceType.Wood, 19 },
        { ResourceType.Wool, 19 },
        { ResourceType.Ore, 19 },
        { ResourceType.Grain, 19 }
    };

    public Bank(Bank bank)
    {
        foreach (var resource in bank.Resources)
            Resources[resource.Key] = resource.Value;
    }

    public Bank(Dictionary<ResourceType, int> resources)
    {
        foreach (var resource in resources)
            Resources[resource.Key] = resource.Value;
    }

    public Bank()
    {
    }

    public int GetResourceCount(ResourceType resource)
    {
        if (!Resources.ContainsKey(resource))
            throw new InvalidOperationException($"Bank does not have resource type: {resource}");

        return Resources[resource];
    }

    public void WithdrawResources(ResourceType resource, int quantity)
    {
        if (!Resources.ContainsKey(resource))
            throw new InvalidOperationException($"Bank does not have resource type: {resource}");

        if (Resources[resource] < quantity)
            throw new InvalidOperationException($"Bank does not have enough of resource type: {resource}");

        Resources[resource] -= quantity;
    }
    public void ReturnResources(ResourceType resource, int quantity)
    {
        if (!Resources.ContainsKey(resource))
            throw new InvalidOperationException($"Bank does not have resource type: {resource}");

        Resources[resource] += quantity;
    }

    public static int GetTradeRate(Player player, ResourceType resource)
    {
        var tradeRate = GameSettings.DefaultBankTradeRate;

        if (player.Ports.Contains(PortType.ThreeToOne))
            tradeRate = 3;
        
        if ((resource == ResourceType.Brick && player.Ports.Contains(PortType.Brick))
                || (resource == ResourceType.Wood && player.Ports.Contains(PortType.Wood))
                || (resource == ResourceType.Wool && player.Ports.Contains(PortType.Wool))
                || (resource == ResourceType.Ore && player.Ports.Contains(PortType.Ore))
                || (resource == ResourceType.Grain && player.Ports.Contains(PortType.Grain)))
            tradeRate = 2;

        return tradeRate;    
    }

    public ResponseDTO TradeWithBank(GameState gs, Player player, Dictionary<ResourceType, int> offer, Dictionary<ResourceType, int> request)
    {
        // Check if player has the offered resources
        foreach (var resource in offer)
            if (!player.Resources.ContainsKey(resource.Key) || player.Resources[resource.Key] < resource.Value)
                return new ResponseDTO(false, 1006, $"PlayerId: {player.Id}; ResourceOffered: {resource.Key}; OfferedQty: {resource.Value}", null as GameStateDTO);

        // Check if the trade is valid according to bank rules
        if (offer.Count != 1 || request.Count != 1)
            return new ResponseDTO(false, 1007, $"PlayerId: {player.Id}; ResourceTypesInOffer: {offer.Count}; ResourceTypesInRequest: {request.Count}", null as GameStateDTO);

        var required = GetTradeRate(player, offer.First().Key);
        if (offer.First().Value != required)
            return new ResponseDTO(false, 1008, $"PlayerId: {player.Id}; ResourceOffered: {offer.First().Key}; Offered: {offer.First().Value}; Required: {required}", null as GameStateDTO);

        if (offer.First().Key == request.First().Key)
            return new ResponseDTO(false, 1009, $"PlayerId: {player.Id}; ResourceOffered: {offer.First().Key}", null as GameStateDTO);

        if (request.First().Value != 1)
            return new ResponseDTO(false, 1009, $"PlayerId: {player.Id}; ResourceRequested: {request.First().Key}; RequestedQty: {request.First().Value}", null as GameStateDTO);

        if (GetResourceCount(request.First().Key) < request.First().Value)
            return new ResponseDTO(false, 1076, $"PlayerId: {player.Id}; ResourceRequested: {request.First().Key}; RequestedQty: {request.First().Value}", null as GameStateDTO);

        // Execute trade
        foreach (var resource in offer)
        {
            player.RemoveResources(resource.Key, resource.Value);
            ReturnResources(resource.Key, resource.Value);
        }

        foreach (var resource in request)
        {
            WithdrawResources(resource.Key, resource.Value);
            player.AssignResources(resource.Key, resource.Value);
        }

        return new ResponseDTO(true, 0, string.Empty, gs, player);
    }

    public Dictionary<ResourceType, int> GetBankResources()
    {
        return new Dictionary<ResourceType, int>(Resources);
    }
}
