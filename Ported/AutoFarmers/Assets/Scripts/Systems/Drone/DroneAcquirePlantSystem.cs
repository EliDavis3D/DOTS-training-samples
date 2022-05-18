using System.Resources;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

public partial struct DroneAcquirePlantSystem : ISystem
{
    ComponentDataFromEntity<LocalToWorld> localToWorldFromEntity;

    ComponentDataFromEntity<Plant> plantFromEntity;

    EntityQuery grownPlantsQuery;

    public void OnCreate(ref SystemState state)
    {
        localToWorldFromEntity = state.GetComponentDataFromEntity<LocalToWorld>(true);
        plantFromEntity = state.GetComponentDataFromEntity<Plant>();
        grownPlantsQuery = state.World.EntityManager.CreateEntityQuery(typeof(PlantGrown));
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        localToWorldFromEntity.Update(ref state);
        plantFromEntity.Update(ref state);

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        var chunks = grownPlantsQuery.CreateArchetypeChunkArray(Allocator.Temp);

        foreach (var drone in SystemAPI.Query<DroneGettingPlantAspect>())
        {
            if (!drone.HasPlant)
            {
                var dronePos = localToWorldFromEntity.GetRefRO(drone.Self).ValueRO.Position;
                var closestPlant = Entity.Null;
                var closestPlantDistance = float.MaxValue;
                var closestPlantPos = new float3(0,0,0);
                var chunkIndex = -1;
                var entryInChunkIndex = -1;
                for (int i = 0; i < chunks.Length; i++)
                {
                    var chunk = chunks[i];
                    var plants = chunk.GetNativeArray(state.GetEntityTypeHandle());
                    for (int j = 0; j < chunk.Count; j++)
                    {
                        var plant = plants[j];
                        var plantPos = localToWorldFromEntity.GetRefRO(plant).ValueRO.Position;
                        var claimed = plantFromEntity.GetRefRO(plant).ValueRO.ClaimedBy;
                        if(claimed != Entity.Null)
                        {
                            continue;
                        }
                        var dist = math.distancesq(plantPos, dronePos);
                        if (dist < closestPlantDistance)
                        {
                            closestPlantDistance = dist;
                            closestPlant = plant;
                            closestPlantPos = plantPos;
                            chunkIndex = i;
                            entryInChunkIndex = j;
                        }
                    }
                    plants.Dispose();
                }

                if (closestPlant != Entity.Null)
                {
                    ecb.SetComponent<Plant>(closestPlant, new Plant { ClaimedBy = drone.Self });
                    drone.Plant = closestPlant;
                    drone.DesiredLocation = new int2((int)math.round(closestPlantPos.x), (int)math.round(closestPlantPos.z));

                    //var plants = chunks[i].GetNativeArray(state.GetEntityTypeHandle());

                }
            }

            if (drone.AtDesiredLocation && drone.HasPlant)
            {
                ecb.AddComponent(drone.Plant, new Follow
                {
                    EntityToFollow = drone.Self,
                    Offset = new float3(0, 1, 0)
                });
                drone.DesiredLocation = new int2(0, 0);
                ecb.RemoveComponent(drone.Self, typeof(DroneAquirePlantIntent));
                ecb.AddComponent(drone.Self, new DroneDepositPlantIntent
                {
                    Plant = drone.Plant
                });
            }
        }

        chunks.Dispose();
    }
}
