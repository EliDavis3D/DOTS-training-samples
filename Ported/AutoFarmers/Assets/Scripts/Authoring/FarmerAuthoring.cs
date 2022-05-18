using Unity.Entities;
using Unity.Mathematics;

class FarmerAuthoring : UnityEngine.MonoBehaviour
{
    public float Speed;
}

class FarmerBaker : Baker<FarmerAuthoring>
{
    public override void Bake(FarmerAuthoring authoring)
    {
        AddComponent(new Farmer
        {
        });
        AddComponent(new Mover
        {
            Speed = authoring.Speed
        });
    }
}