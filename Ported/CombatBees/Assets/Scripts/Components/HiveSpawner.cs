using Unity.Entities;

public struct HiveSpawner : IComponentData
{
    public int BeesAmount;
    public Entity ResourcePrefab;
    public int ResourceAmount;
}
