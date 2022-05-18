using Unity.Entities;

public class DroneAuthoring : UnityEngine.MonoBehaviour
{
    public float Speed;
}

class DroneBaker : Baker<DroneAuthoring>
{
    public override void Bake(DroneAuthoring authoring)
    {
        AddComponent(new Drone
        {
        });
        AddComponent(new Mover
        {
            Speed = authoring.Speed
        });
    }
}