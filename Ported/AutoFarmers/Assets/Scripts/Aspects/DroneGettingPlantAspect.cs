using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

readonly partial struct DroneGettingPlantAspect : IAspect<DroneGettingPlantAspect>
{
    public readonly Entity Self;

    private readonly MovementAspect Movement;

    private readonly RefRW<Drone> Drone;

    public bool AtDesiredLocation
    {
        get => Movement.AtDesiredLocation;
    }

    public int2 DesiredLocation
    {
        set => Movement.DesiredLocation = value;
    }

    public Entity Plant
    {
        get => Drone.ValueRO.Plant;
    }
}
