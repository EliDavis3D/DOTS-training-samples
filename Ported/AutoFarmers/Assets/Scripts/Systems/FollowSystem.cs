using System.Resources;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

[BurstCompile]
public partial struct FollowSystem : ISystem
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
        var transform = state.GetComponentDataFromEntity<LocalToWorld>();
        foreach (var follower in SystemAPI.Query<FollowAspect>())
        {
            follower.Position = transform.GetRefRO(follower.ThingToFollow).ValueRO.Position + follower.Offset;
        }
    }
}
