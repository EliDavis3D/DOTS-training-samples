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

        int farmersToSpawn = (money.FarmerMoney / 100) - money.SpawnedFarmers;
        int dronesToSpawn = (money.DroneMoney / 100) - money.SpawnedDrones;

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        for (int i = 0; i < farmersToSpawn; i++)
        {
            var farmer = ecb.Instantiate(gameConfig.FarmerPrefab);
            ecb.AddComponent(farmer, new Translation()
            {
                Value = new float3(money.FarmerMoney / 10, 0, 0),
            });
            ecb.AddBuffer<Waypoint>(farmer);
            money.SpawnedFarmers += 1;
        }

        for (int i = 0; i < dronesToSpawn; i++)
        {
            var drone = ecb.Instantiate(gameConfig.DronePrefab);
            ecb.AddComponent(drone, new Translation()
            {
                Value = new float3(money.SpawnedDrones, 0, 0),
            });

            ecb.AddComponent(drone, new DroneAquirePlantIntent
            {
                Plant = plant,
            });

            ecb.AddComponent(drone, new Mover()
            {
                HasDestination = true,
                DesiredLocation = new int2(money.SpawnedDrones, -10),
                YOffset = 2,
                Speed = 2,
            });
            money.SpawnedDrones += 1;
        }

        ecb.SetComponent(moneyEntity, money);
        ecb.ShouldPlayback = true;
    }
}
