using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;

public readonly partial struct FarmerTargetingAspect : IAspect<FarmerTargetingAspect>
{
    readonly RefRO<Translation> translationRef;
    readonly RefRW<Targeting> targetingRef;
    readonly RefRW<FarmerIntent> intentRef;
    readonly RefRW<FarmerCombat> combatRef;

    public Translation translation => translationRef.ValueRO;
    public Targeting targeting { get => targetingRef.ValueRO; set => targetingRef.ValueRW = value; }
    public FarmerIntent intent { get => intentRef.ValueRO; set => intentRef.ValueRW = value; }
    public FarmerCombat combat { get => combatRef.ValueRO; set => combatRef.ValueRW = value; }
}
