using Unity.Entities;
using Unity.Mathematics;

class GameConfigAuthoring : UnityEngine.MonoBehaviour
{
    public UnityEngine.GameObject FarmerPrefab;
    public int InitialFarmers;
    public int2 MapSize;

    public UnityEngine.GameObject GroundTileTilledPrefab;
    public UnityEngine.GameObject GroundTileUntilledPrefab;
    public UnityEngine.GameObject PlantPrefab;
    
    [UnityEngine.Tooltip("How long it takes (in seconds) for the plant to completely finish growing.")]
    public float PlantIncubationTime;

    public UnityEngine.GameObject SiloPrefab;
}

class GameConfigBaker : Baker<GameConfigAuthoring>
{
    public override void Bake(GameConfigAuthoring authoring)
    {
        AddComponent(new GameConfig
        {
            FarmerPrefab = GetEntity(authoring.FarmerPrefab),
            PlantPrefab = GetEntity(authoring.PlantPrefab),
            InitialFarmerCount = authoring.InitialFarmers,
            MapSize = authoring.MapSize,

            GroundTileTilledPrefab = GetEntity(authoring.GroundTileTilledPrefab),
            GroundTileUntilledPrefab = GetEntity(authoring.GroundTileUntilledPrefab),

            PlantIncubationTime = authoring.PlantIncubationTime,

            SiloPrefab= GetEntity(authoring.SiloPrefab),
        });
    }
}