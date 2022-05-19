using System.Resources;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

[BurstCompile]
public partial struct FarmerDroneSpawnerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<FarmMoney>();
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var moneyEntity = SystemAPI.GetSingletonEntity<FarmMoney>();
        var money = SystemAPI.GetSingletonRW<FarmMoney>();
        //money.FarmerMoney += 1;
        //money.DroneMoney += 1;
        var gameConfig = SystemAPI.GetSingleton<GameConfig>();

        int farmersToSpawn = (money.FarmerMoney / gameConfig.CostToSpawnFarmer) - money.SpawnedFarmers;
        int dronesToSpawn = (money.DroneMoney / gameConfig.CostToSpawnDrone) - money.SpawnedDrones;

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        Random randomGenerator = new Random((uint)gameConfig.WorldGenerationSeed);

        for (int i = 0; i < farmersToSpawn; i++)
        {
            var farmer = ecb.Instantiate(gameConfig.FarmerPrefab);
            ecb.AddComponent(farmer, new Translation()
            {
                Value = new float3(randomGenerator.NextInt(0, gameConfig.MapSize.x), 0, randomGenerator.NextInt(0, gameConfig.MapSize.y)),
            });
            ecb.AddBuffer<Waypoint>(farmer);
            money.SpawnedFarmers += 1;
        }

        for (int i = 0; i < dronesToSpawn; i++)
        {
            var drone = ecb.Instantiate(gameConfig.DronePrefab);
            ecb.AddComponent(drone, new Translation()
            {
                Value = new float3(randomGenerator.NextInt(0, gameConfig.MapSize.x), 0, randomGenerator.NextInt(0, gameConfig.MapSize.y)),
            });
            ecb.AddBuffer<Waypoint>(drone);
            money.SpawnedDrones += 1;
        }

        ecb.SetComponent(moneyEntity, money);
        ecb.ShouldPlayback = true;
    }
}
