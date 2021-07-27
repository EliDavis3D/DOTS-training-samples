using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using Random = UnityEngine.Random;
namespace DOTSRATS
{
    public class RatSpawnerSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            int numRats = GetEntityQuery(ComponentType.ReadOnly<Rat>()).CalculateEntityCount();
            Entities
                .WithAll<InPlay, RatSpawner>()
                .ForEach((Entity entity, in RatSpawner ratSpawner, in Translation translation, in DirectionData direction) =>
                {
                    if (numRats < ratSpawner.maxRats)
                    {
                        if (Random.Range(0f, 1f) < ratSpawner.spawnRate)
                        {
                            var instance = ecb.Instantiate(ratSpawner.ratPrefab);
                            ecb.SetComponent(instance, translation);
                            ecb.AddComponent(instance, new Velocity{Direction = direction.Value, Speed = 1f});
                        }
                    }
                }).Run();

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}