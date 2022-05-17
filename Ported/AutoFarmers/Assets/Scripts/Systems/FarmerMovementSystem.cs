using System.Resources;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

[BurstCompile]
public partial class FarmerMovementSystem : SystemBase
{
    public void OnCreate(ref SystemState state)
    {
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    protected override void OnUpdate()
    {
        var dt = Time.DeltaTime;
        Entities
            .WithAll<Farmer>()
            .ForEach((Entity entity, TransformAspect transform) =>
            {
                var pos = transform.Position;
                pos.y = entity.Index;
                var angle = (0.5f + noise.cnoise(pos / 10f)) * 4.0f * math.PI;
                var dir = float3.zero;
                math.sincos(angle, out dir.x, out dir.z);
                transform.Position += dir * dt * 5.0f;
                transform.Rotation = quaternion.RotateY(angle);
            }).ScheduleParallel();
    }
}
