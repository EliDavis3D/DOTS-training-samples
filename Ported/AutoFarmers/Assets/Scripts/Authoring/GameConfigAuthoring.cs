using Unity.Entities;
using Unity.Mathematics;

class GameConfigAuthoring : UnityEngine.MonoBehaviour
{
    public UnityEngine.GameObject FarmerPrefab;
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
            InitialFarmerCount = authoring.InitialFarmers,
            MapSize = authoring.MapSize
        });
    }
}