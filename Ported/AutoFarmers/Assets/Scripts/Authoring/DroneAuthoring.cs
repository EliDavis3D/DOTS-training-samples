using Unity.Entities;

public class DroneAuthoring : UnityEngine.MonoBehaviour
{
}

class DroneBaker : Baker<DroneAuthoring>
{
    public override void Bake(DroneAuthoring authoring)
    {
        AddComponent(new Drone
        {
        });
    }
}