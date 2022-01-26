using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public partial class MovementSystem : SystemBase
{
    private EntityCommandBufferSystem sys;

    protected override void OnCreate()
    {
        sys = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
        RequireSingletonForUpdate<Spawner>();
    }

    protected override void OnUpdate()
    {
        var tAdd = UnityEngine.Time.deltaTime;
        var tNow = UnityEngine.Time.timeSinceLevelLoad;
        var spawner = GetSingleton<Spawner>();

        var randomSeed = (uint) math.max(1,
            System.DateTime.Now.Millisecond +
            System.DateTime.Now.Second +
            System.DateTime.Now.Minute +
            System.DateTime.Now.Day +
            System.DateTime.Now.Month +
            System.DateTime.Now.Year);

        var random = new Random(randomSeed);


        // TODO: Can we parallelize movement by scheduling the translation updates
        //       and distance checks separately?  Should let us ues ScheduleParallel()
        //       and WithNativeDisableParallelForRestriction(), yeah?

        //bits
        Entities
            .WithAll<BeeBitsTag>()
            .ForEach((Entity e, ref Translation translation) =>
            {
                // calculate bits falling movement - straight down for now
                if (translation.Value.y > 0)
                {
                    translation.Value = translation.Value * -9.8f * tAdd;
                }
                else
                {
                    //destroy this entity and create/init a blood splat
                    /*
                    ecb.DestroyEntity(e);
                    var instance = ecb.Instantiate(spawner.BloodPrefab);

                    var trans = new Translation
                    {
                        Value = translation.Value
                    };

                    ecb.SetComponent(instance, translation);
                    */

                }
            }).Schedule();
        //sys.AddJobHandleForProducer(Dependency);

        //blood
        Entities
            .WithAll<BloodTag>()
            .ForEach((Entity e, ref Translation translation, ref PP_Movement ppMovement, in Velocity velocity) =>
            {
                // calculate blood scale and later maskOut

                // do orientation later

            }).Schedule();

        //bees
        Entities
            .WithAll<BeeTag>()
            .ForEach((ref Translation translation, ref PP_Movement ppMovement, in BeeTag beeTag, in Velocity velocity) =>
            {
                // do bee movement
                translation.Value = ppMovement.Progress(tAdd);

            }).Schedule();

        var ecb = sys.CreateCommandBuffer();
        //food, dependant on Bees.
        Entities
            .WithAll<Food>()
            .ForEach((Entity e, ref Translation translation, ref PP_Movement ppMovement, in Velocity velocity) =>
            {
                translation.Value = ppMovement.Progress(tAdd);

                if (math.abs(translation.Value.x) >= spawner.ArenaExtents.x)
                {
                    Debug.Log("SpawnBees");
                    if (translation.Value.y <= 0.5f)
                    {
                        //destroy this entity and create/init a blood splat
                        ecb.DestroyEntity(e);


                        for (var i = 0; i < 3; i++)
                        {
                            var minBeeBounds = SpawnerSystem.GetBeeMinBounds(spawner);
                            var maxBeeBounds = SpawnerSystem.GetBeeMaxBounds(spawner, minBeeBounds);

                            var beeRandomY = SpawnerSystem.GetRandomBeeY(ref random, minBeeBounds, maxBeeBounds);
                            var beeRandomZ = SpawnerSystem.GetRandomBeeZ(ref random, minBeeBounds, maxBeeBounds);

                            if (translation.Value.x > 0)
                            {
                                // Yellow Bees
                                var beeRandomX = SpawnerSystem.GetRandomYellowBeeX(ref random, minBeeBounds, maxBeeBounds);

                                SpawnerSystem.BufferEntityInstantiation(spawner.YellowBeePrefab,
                                    new float3(beeRandomX, beeRandomY, beeRandomZ),
                                    ref ecb);
                            }
                            else
                            {
                                // Blue Bees
                                var beeRandomX = SpawnerSystem.GetRandomBlueBeeX(ref random, minBeeBounds, maxBeeBounds);

                                SpawnerSystem.BufferEntityInstantiation(spawner.BlueBeePrefab,
                                    new float3(beeRandomX, beeRandomY, beeRandomZ),
                                    ref ecb);
                            }
                        }
                    }
                }
            }).Schedule();
        sys.AddJobHandleForProducer(Dependency);
    }
}
