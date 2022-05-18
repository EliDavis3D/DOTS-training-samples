using System.Resources;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

[BurstCompile]
public partial struct MovementSystem : ISystem
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
        var dt = state.Time.DeltaTime;
        foreach (var mover in SystemAPI.Query<MovementAspect>())
        {
            var dir = mover.DesiredWorldLocation - mover.Position;
            mover.Position += math.normalize(dir) * dt * mover.Speed;
        }
    }
}
