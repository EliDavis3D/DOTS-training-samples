using System.Resources;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

namespace Assets.Scripts.Systems.Drone
{
    public partial struct FindPlantSystem : ISystem
    {
        EntityQuery grownPlantsQuery;
        
        ComponentDataFromEntity<LocalToWorld> localToWorldFromEntity;

        ComponentDataFromEntity<Plant> plantFromEntity;

        public void OnCreate(ref SystemState state)
        {
            grownPlantsQuery = state.World.EntityManager.CreateEntityQuery(typeof(PlantGrown));
            localToWorldFromEntity = state.GetComponentDataFromEntity<LocalToWorld>(true);
            plantFromEntity = state.GetComponentDataFromEntity<Plant>();
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var chunks = grownPlantsQuery.CreateArchetypeChunkArray(Allocator.Temp);
            if(chunks.Length == 0)
            {
                return;
            }

            localToWorldFromEntity.Update(ref state);
            plantFromEntity.Update(ref state);
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
            foreach (var drone in SystemAPI.Query<DroneFindPlantAspect>().WithAll<DroneFindPlantIntent>())
            {
                var dronePos = localToWorldFromEntity.GetRefRO(drone.Self).ValueRO.Position;
                var closestPlant = Entity.Null;
                var closestPlantDistance = float.MaxValue;
                var closestPlantPos = new float3(0, 0, 0);
                //var chunkIndex = -1;
                //var entryInChunkIndex = -1;
                for (int i = 0; i < chunks.Length; i++)
                {
                    var chunk = chunks[i];
                    var plants = chunk.GetNativeArray(state.GetEntityTypeHandle());
                    for (int j = 0; j < chunk.Count; j++)
                    {
                        var plant = plants[j];
                        var plantPos = localToWorldFromEntity.GetRefRO(plant).ValueRO.Position;
                        var claimed = plantFromEntity.GetRefRO(plant).ValueRO.ClaimedBy;
                        if (claimed != Entity.Null)
                        {
                            continue;
                        }
                        var dist = math.distancesq(plantPos, dronePos);
                        if (dist < closestPlantDistance)
                        {
                            closestPlantDistance = dist;
                            closestPlant = plant;
                            closestPlantPos = plantPos;
                            //chunkIndex = i;
                            //entryInChunkIndex = j;
                        }
                    }
                    plants.Dispose();
                }

                if (closestPlant != Entity.Null)
                {
                    ecb.SetComponent<Plant>(closestPlant, new Plant { ClaimedBy = drone.Self });
                    ecb.AddComponent(drone.Self, new DroneAquirePlantIntent
                    {
                        Plant=closestPlant,
                    });
                    drone.DesiredLocation = new int2(
                        (int)math.round(closestPlantPos.x),
                        (int)math.round(closestPlantPos.z)
                    );
                    ecb.RemoveComponent<DroneFindPlantIntent>(drone.Self);
                    //var plants = chunks[i].GetNativeArray(state.GetEntityTypeHandle());
                }
            }

            chunks.Dispose();
        }
    }
}
