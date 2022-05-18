using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

readonly partial struct MovementAspect : IAspect<MovementAspect>
{
    public readonly Entity Self;

    private readonly TransformAspect Transform;

    private readonly RefRW<Mover> Mover;

    public float3 Position
    {
        get => Transform.Position;
        set => Transform.Position = value;
    }

    public int2 DesiredLocation
    {
        get => Mover.ValueRO.DesiredLocation;
        set => Mover.ValueRW.DesiredLocation = value;
    }

    public float3 DesiredWorldLocation
    {
        get => new float3(Mover.ValueRO.DesiredLocation.x, YOffset, Mover.ValueRO.DesiredLocation.y);
    }


    public float Speed
    {
        get => Mover.ValueRO.Speed;
    }

    public float YOffset
    {
        get => Mover.ValueRO.YOffset;
    }
}
