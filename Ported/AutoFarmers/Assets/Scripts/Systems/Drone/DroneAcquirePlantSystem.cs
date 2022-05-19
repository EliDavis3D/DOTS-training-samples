using System.Resources;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

public partial struct DroneAcquirePlantSystem : ISystem
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
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        foreach (var drone in SystemAPI.Query<DroneGettingPlantAspect>())
        {
            if (drone.AtDesiredLocation && drone.HasPlant)
            {
                ecb.AddComponent(drone.Plant, new Follow
                {
                    EntityToFollow = drone.Self,
                    Offset = new float3(0, 1, 0)
                });
                drone.DesiredLocation = new int2(0, 0);
                ecb.RemoveComponent<DroneAquirePlantIntent>(drone.Self);
                ecb.AddComponent(drone.Self, new DroneDepositPlantIntent
                {
                    Plant = drone.Plant
                });
            }
        }

    }
}
