using Unity.Entities;
using Unity.Mathematics;

class GameConfigAuthoring : UnityEngine.MonoBehaviour
{
    public UnityEngine.GameObject FarmerPrefab;
    public int InitialFarmers;
    public int2 MapSize;
    public UnityEngine.GameObject PlantPrefab;
    
    [UnityEngine.Tooltip("How long it takes (in seconds) for the plant to completely finish growing.")]
    public float PlantIncubationTime;
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
            PlantIncubationTime = authoring.PlantIncubationTime,
        });
    }
}