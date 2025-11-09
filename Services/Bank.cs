using GameTest.DTOs;
using GameTest.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace GameTest.Services;

public class Bank
{
    public bool TradeWithBank(Player player, Dictionary<ResourceType, int> offer, Dictionary<ResourceType, int> request)
    {
        // Check if player has the offered resources
        foreach (var resource in offer)
            if (!player.Resources.ContainsKey(resource.Key) || player.Resources[resource.Key] < resource.Value)
                return false; // Player does not have enough resources to offer

        // Check if the trade is valid according to bank rules
        if (offer.Count != 1 || request.Count != 1)
            return false; // Bank only accepts trades with one type of resource offered

        // TODO: Implement different trade ratios based on ports or game settings
        if (offer.First().Value != 4)
            return false; // Bank requires at least 4 of the offered resource

        if (offer.First().Key == request.First().Key)
            return false; // Cannot trade the same resource type

        if (request.First().Value != 1)
            return false; // Bank only gives 1 of the requested resource

        // TODO: Check if bank has the requested resources

        // Execute trade
        foreach (var resource in offer)
            player.RemoveResources(resource.Key, resource.Value);

        foreach (var resource in request)
            player.AssignResources(resource.Key, resource.Value);

        return true;
    }
}
