using Unity.Entities;
using Unity.Mathematics;

class GameConfigAuthoring : UnityEngine.MonoBehaviour
{
    public UnityEngine.GameObject FarmerPrefab;
    public UnityEngine.GameObject PlantPrefab;
    public int InitialFarmers;
    public int2 MapSize;
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
            MapSize = authoring.MapSize
        });
    }
}