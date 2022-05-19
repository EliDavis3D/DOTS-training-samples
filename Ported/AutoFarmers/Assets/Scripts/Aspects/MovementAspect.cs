using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

readonly partial struct MovementAspect : IAspect<MovementAspect>
{
    public readonly Entity Self;

    public readonly TransformAspect Transform;

    private readonly RefRW<Mover> Mover;

    public float3 Position
    {
        get => Transform.Position;
        set => Transform.Position = value;
    }

    public bool HasDestination
    {
        get => Mover.ValueRO.HasDestination;
        set => Mover.ValueRW.HasDestination = value;
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

    public bool AtDesiredLocation
    {
        get {
            return math.distancesq(DesiredWorldLocation, Position) < 0.01f;
        }
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
