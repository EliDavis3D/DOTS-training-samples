using System.Resources;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

[BurstCompile]
public partial struct GameInitSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var config = SystemAPI.GetSingleton<GameConfig>();

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        var allocator = state.WorldUnmanaged.UpdateAllocator.ToAllocator;

        GroundUtilities.GenerateGroundAndRocks(ecb, config.MapSize);

        // Initial Farmer
        var farmers = CollectionHelper.CreateNativeArray<Entity>(config.InitialFarmerCount, allocator);
        ecb.Instantiate(config.FarmerPrefab, farmers);

        // This system should only run once at startup. So it disables itself after one update.
        // @TODO: Nic - should we also flag some component as "game ready" so systems relying on game setup know not to run?
        state.Enabled = false;
    }
}
