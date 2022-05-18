
using Unity.Entities;
using Unity.Mathematics;

struct GridMover : IComponentData
{
    public int2 CurrentCoordiantes;
    public int2 DestinationCoordinates;
}
