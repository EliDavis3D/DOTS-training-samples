using Unity.Entities;
using Unity.Mathematics;

public struct Targeting : IComponentData
{
    public int2 tileTarget;
    public Entity entityTarget;
}
