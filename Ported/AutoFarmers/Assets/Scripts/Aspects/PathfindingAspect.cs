
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

readonly partial struct PathfindingAspect : IAspect<PathfindingAspect>
{
    private readonly RefRW<GridMover> Mover;

    public int2 CurrentCoordinates
    {
        get => Mover.ValueRO.CurrentCoordiantes;
        set => Mover.ValueRW.CurrentCoordiantes = value;
    }
}