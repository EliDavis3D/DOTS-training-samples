using Unity.Entities;
using Unity.Mathematics;

public enum FarmerIntentState
{
    None,       // Not doing anything, shouldn't be in this state much.
    SmashRocks, // Searches for a rock to smash to unblock its ground for potential tilling.
    TillGround, // Tills ground (in unblocked [from a rock or other])
    PlantSeeds, // Plants seeds in tilled ground.
    SellPlants, // Searches for a plant to harvest, if already holding one, sells it.
}

public struct FarmerIntent : IComponentData
{
    public FarmerIntentState value;
    public float elapsed; // Time passed in current intent state.
    public Random random; 
}
