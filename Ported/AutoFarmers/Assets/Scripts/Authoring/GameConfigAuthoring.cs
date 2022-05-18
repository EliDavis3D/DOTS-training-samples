using Unity.Entities;
using Unity.Mathematics;

class GameConfigAuthoring : UnityEngine.MonoBehaviour
{
    public UnityEngine.GameObject FarmerPrefab;
    public int InitialFarmers;

    public UnityEngine.GameObject DronePrefab;

    public int2 MapSize;

    public UnityEngine.GameObject GroundTileNormalPrefab;
    public UnityEngine.GameObject GroundTileTilledPrefab;
    public UnityEngine.GameObject GroundTileUnpassablePrefab;
    
    public UnityEngine.GameObject PlantPrefab;
    
    [UnityEngine.Tooltip("How long it takes (in seconds) for the plant to completely finish growing.")]
    public float PlantIncubationTime;

    public UnityEngine.GameObject SiloPrefab;

    public UnityEngine.GameObject RockPrefab;
    public int InitialRockAttempts;
    public float MinRockSize;
    public float MaxRockSize;
    public int RockHealthPerUnitArea;
    public float MinRockDepth;
    public float MaxRockDepth;
}

class GameConfigBaker : Baker<GameConfigAuthoring>
{
    public override void Bake(GameConfigAuthoring authoring)
    {
        AddComponent(new GameConfig
        {
            FarmerPrefab = GetEntity(authoring.FarmerPrefab),
            InitialFarmerCount = authoring.InitialFarmers,

            DronePrefab= GetEntity(authoring.DronePrefab),

            PlantPrefab = GetEntity(authoring.PlantPrefab),
            MapSize = authoring.MapSize,

            GroundTileNormalPrefab = GetEntity(authoring.GroundTileNormalPrefab),
            GroundTileTilledPrefab = GetEntity(authoring.GroundTileTilledPrefab),
            GroundTileUnpassablePrefab = GetEntity(authoring.GroundTileUnpassablePrefab),

            PlantIncubationTime = authoring.PlantIncubationTime,

            SiloPrefab = GetEntity(authoring.SiloPrefab),

            RockPrefab = GetEntity(authoring.RockPrefab),
            InitialRockAttempts = authoring.InitialRockAttempts,
            MinRockSize = authoring.MinRockSize,
            MaxRockSize = authoring.MaxRockSize,
            RockHealthPerUnitArea = authoring.RockHealthPerUnitArea,
            MinRockDepth = authoring.MinRockDepth,
            MaxRockDepth = authoring.MaxRockDepth,
        });
    }
}