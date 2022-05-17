using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Rendering;

readonly partial struct PlantGrowingAspect : IAspect<PlantGrowingAspect>
{
    private readonly RefRW<PlantHealth> PlantHealth;

    private readonly RefRW<Scale> Scale;

    public float Health
    {
        get => PlantHealth.ValueRO.Health;
        set {
            var clamped = math.clamp(value, 0, 1);
            PlantHealth.ValueRW.Health = clamped;
            Scale.ValueRW.Value = clamped;
        }
    }
}
