using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;

public readonly partial struct FarmerRockBreakingAspect : IAspect<FarmerRockBreakingAspect>
{
    public readonly Entity Self;

    readonly DynamicBuffer<Waypoint> pathfindingWaypoints;
    readonly RefRO<Translation> translationRef;
    readonly RefRW<FarmerIntent> intentRef;
    readonly RefRW<PathfindingIntent> pathfindingIntentRef;
    readonly RefRW<FarmerCombat> combatRef;

    public DynamicBuffer<Waypoint> PathfindingWaypoints => pathfindingWaypoints;
    public Translation translation => translationRef.ValueRO;
    public FarmerIntent intent { get => intentRef.ValueRO; set => intentRef.ValueRW = value; }
    public PathfindingIntent pathfindingIntent { get => pathfindingIntentRef.ValueRO; set => pathfindingIntentRef.ValueRW = value; }
    public FarmerCombat combat { get => combatRef.ValueRO; set => combatRef.ValueRW = value; }
}
