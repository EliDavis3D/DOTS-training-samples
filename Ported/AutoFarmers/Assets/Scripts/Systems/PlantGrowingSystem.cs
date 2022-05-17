using Unity.Burst;
using Unity.Entities;

[BurstCompile]
public partial class PlantGrowingSystem : SystemBase
{
    public void OnCreate(ref SystemState state) {  }

    public void OnDestroy(ref SystemState state) { }


    [BurstCompile]
    protected override void OnUpdate()
    {
        var growthRate = 1f / SystemAPI.GetSingleton<GameConfig>().PlantIncubationTime;
        var dt = Time.DeltaTime;
        var growth = dt * growthRate;
        Entities
            .WithAll<PlantHealth>()
            .ForEach((PlantGrowingAspect health) =>
            {
                health.Health += growth;
            }).ScheduleParallel();
    }
}
