using Unity.Entities;
using Unity.Mathematics;

class FarmerAuthoring : UnityEngine.MonoBehaviour
{
}

class FarmerBaker : Baker<FarmerAuthoring>
{
    public override void Bake(FarmerAuthoring authoring)
    {
        AddComponent(new Farmer { });
        AddComponent(new FarmerIntent { value = FarmerIntentState.None, random = new Random((uint)UnityEngine.Random.Range(0, uint.MaxValue)) });
    }
}