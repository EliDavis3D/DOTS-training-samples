using Unity.Entities;
using Unity.Mathematics;
struct GameConfig : IComponentData
{
    public Entity FarmerPrefab;

    public int InitialFarmerCount;
 
    public int2 MapSize;

    public Entity GroundTileUntilledPrefab;
    public Entity GroundTileTilledPrefab;
}