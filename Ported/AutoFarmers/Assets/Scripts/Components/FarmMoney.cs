using Unity.Entities;

public struct FarmMoney : IComponentData
{
    public int FarmerMoney;

    public int DroneMoney;

    public int SpawnedFarmers;

    public int SpawnedDrones;
}
