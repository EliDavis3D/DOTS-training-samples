
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

readonly partial struct PathfindingAspect : IAspect<PathfindingAspect>
{
    public readonly Entity Self;
    
    public readonly RefRO<GridMover> Mover;
    public readonly DynamicBuffer<Waypoint> Waypoints;

    public int2 CurrentCoordinates
    {
        get => Mover.ValueRO.CurrentCoordiantes;
    }

    public void ClearWaypoints()
    {
        Waypoints.Clear();
    }

    public void AddWaypoint(Waypoint newWaypoint)
    {
        Waypoints.Add(newWaypoint);
        
    }
}