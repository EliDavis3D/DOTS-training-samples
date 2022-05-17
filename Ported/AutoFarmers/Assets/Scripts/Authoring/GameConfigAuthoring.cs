using Unity.Entities;
using Unity.Mathematics;

class GameConfigAuthoring : UnityEngine.MonoBehaviour
{
    public UnityEngine.GameObject FarmerPrefab;
    public int InitialFarmers;
    public int2 MapSize;

    public UnityEngine.GameObject GroundTileTilledPrefab;
    public UnityEngine.GameObject GroundTileUntilledPrefab;
}

class GameConfigBaker : Baker<GameConfigAuthoring>
{
    public override void Bake(GameConfigAuthoring authoring)
    {
        AddComponent(new GameConfig
        {
            FarmerPrefab = GetEntity(authoring.FarmerPrefab),
            InitialFarmerCount = authoring.InitialFarmers,
            MapSize = authoring.MapSize,

            GroundTileTilledPrefab = GetEntity(authoring.GroundTileTilledPrefab),
            GroundTileUntilledPrefab = GetEntity(authoring.GroundTileUntilledPrefab),
        });
    }
}