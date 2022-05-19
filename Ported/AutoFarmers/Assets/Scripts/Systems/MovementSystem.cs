using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

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
            if (mover.HasDestination)
            {
                var dir = mover.DesiredWorldLocation - mover.Position;
                if (!mover.AtDesiredLocation)
                {
                    mover.Position += math.normalize(dir) * dt * mover.Speed;
                }
            }
        }
    }
}
