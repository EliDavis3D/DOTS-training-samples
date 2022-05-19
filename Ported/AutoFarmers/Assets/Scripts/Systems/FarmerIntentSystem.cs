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
            if (intent.value != FarmerIntentState.None)
            {
                intent.elapsed += dt;
                if(intent.elapsed >= TimeInIntentMax)
                {
                    // Time to pick a new intent.
                    intent.elapsed = 0;
                }
                else
                {
                    // Nothing to do.
                    ecb.SetComponent<FarmerIntent>(entityInQueryIndex, entity, intent);
                    return;
                }
            }

            if(intent.value == FarmerIntentState.SmashRocks) { return; } // Smash rocks is done now - remove this when all states are ready

            intent.value = (FarmerIntentState)intent.random.NextInt(1, 5);
            ecb.SetComponent<FarmerIntent>(entityInQueryIndex, entity, intent);

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
        }).ScheduleParallel();
    }
}
