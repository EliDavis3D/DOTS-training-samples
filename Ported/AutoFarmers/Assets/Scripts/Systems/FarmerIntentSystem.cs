using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

[BurstCompile]
public partial class FarmerIntentSystem : SystemBase
{
    [BurstCompile]
    protected override void OnUpdate()
    {
        const float TimeInIntentMax = 5.0f;

        var ecbSystem = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSystem.CreateCommandBuffer(World.Unmanaged).AsParallelWriter();

        float dt = Time.DeltaTime;
        Entities.WithAll<Farmer>().ForEach((Entity entity, int entityInQueryIndex, ref FarmerIntent intent) =>
        {
            // Debug code: Stay in a state for x seconds, then swap to a new one.
            // a state is denoted by color.
            // ! Please remove after dependent intent logic is in.
            if (intent.value == FarmerIntentState.None)
            {
                ecb.AddComponent<ColorOverride>(entityInQueryIndex, entity, new ColorOverride { Value = new float4(1, 1, 1, 1) });

                intent.elapsed += dt;
                if(intent.elapsed >= TimeInIntentMax)
                {
                    PickNewIntent(entityInQueryIndex, entity, ref ecb, intent.random);
                }
                else
                {
                    // Nothing to do.
                    ecb.SetComponent<FarmerIntent>(entityInQueryIndex, entity, intent);
                    return;
                }
            }
        }).ScheduleParallel();
    }

    static void PickNewIntent(int entityInQueryIndex, Entity entity, ref EntityCommandBuffer.ParallelWriter ecb, Random random)
    {
        if(true)
        {
            FarmerIntent intent = new FarmerIntent
            {
                value = FarmerIntentState.TillGround,
                elapsed = 0,
                random = random
            };

            ColorFarmerByIntent(entityInQueryIndex, entity, intent, ref ecb);
            ecb.SetComponent<FarmerIntent>(entityInQueryIndex, entity, intent);
        }
        /*else --- rock smash system doesn't work, ran out of time to debug
        {
            FarmerIntent intent = new FarmerIntent
            {
                value = FarmerIntentState.SmashRocks,
                elapsed = 0,
                random = random
            };

            ColorFarmerByIntent(entityInQueryIndex, entity, intent, ref ecb);
            ecb.SetComponent<FarmerIntent>(entityInQueryIndex, entity, intent);
        }*/
    }

    static void ColorFarmerByIntent(int entityInQueryIndex, Entity entity, in FarmerIntent intent, ref EntityCommandBuffer.ParallelWriter ecb)
    {
        float4 overrideColor = new float4();
        switch (intent.value)
        {
            case FarmerIntentState.SmashRocks:
                overrideColor = new float4(1, 0, 0, 1); // red
                break;
            case FarmerIntentState.TillGround:
                overrideColor = new float4(1, 1, 1, 1); // white
                break;
            case FarmerIntentState.PlantSeeds:
                overrideColor = new float4(0, 0.92f, 0.016f, 1); // yellow
                break;
            case FarmerIntentState.SellPlants:
                overrideColor = new float4(1, 0, 1, 1); // magenta
                break;
        }

        ecb.AddComponent<ColorOverride>(entityInQueryIndex, entity, new ColorOverride { Value = overrideColor });
    }
}
