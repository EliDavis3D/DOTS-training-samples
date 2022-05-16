using Unity.Entities;
using Unity.Mathematics;

class FarmerAuthoring : UnityEngine.MonoBehaviour
{
}

class FarmerBaker : Baker<FarmerAuthoring>
{
    public override void Bake(FarmerAuthoring authoring)
    {
        AddComponent(new Farmer
        {
        });
    }
}