namespace GameTest.Models;

public class GoalWeights
{
    public double SettlementWeight { get; set; }
    public double CityWeight { get; set; }
    public double RoadWeight { get; set; }
    public double DevelopmentCardWeight { get; set; }

    public GoalWeights(double settlementWeight, double cityWeight, double roadWeight, double developmentCardWeight)
    {
        SettlementWeight = settlementWeight;
        CityWeight = cityWeight;
        RoadWeight = roadWeight;
        DevelopmentCardWeight = developmentCardWeight;
    }

    public void NormalizeWeights()
    {
        double totalWeight = SettlementWeight + CityWeight + RoadWeight + DevelopmentCardWeight;
        if (totalWeight > 0)
        {
            SettlementWeight /= totalWeight;
            CityWeight /= totalWeight;
            RoadWeight /= totalWeight;
            DevelopmentCardWeight /= totalWeight;
        }
    }
}
