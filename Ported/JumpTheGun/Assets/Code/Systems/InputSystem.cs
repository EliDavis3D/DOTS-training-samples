using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityInput = UnityEngine.Input;


[UpdateInGroup(typeof(Unity.Entities.SimulationSystemGroup))]
[UpdateAfter(typeof(SpawnerSystem))]
public class InputSystem : SystemBase
{
    protected override void OnUpdate()
    {
        //ray trace mouse position for the board, and figure out a destination position for the ball
        MinMaxHeight minMaxHeight;

        if (!TryGetSingleton<MinMaxHeight>(out minMaxHeight))
            return;
                        
        float halfHeight = (minMaxHeight.Value.x + minMaxHeight.Value.y) * 0.5f;            
        Ray ray = Camera.main.ScreenPointToRay(UnityInput.mousePosition);
        Vector3 mouseWorldPos = new Vector3(0, halfHeight, 0);
        float t = (halfHeight - ray.origin.y) / ray.direction.y;
        mouseWorldPos.x = ray.origin.x + t * ray.direction.x;
        mouseWorldPos.z = ray.origin.z + t * ray.direction.z;

        //Update the player destination position
        var player = GetSingletonEntity<Player>();
        SetComponent(player, new TargetPosition { Value = mouseWorldPos });
        SetComponent(player, new Translation { Value = new float3(0.0f, halfHeight + 10.0f, 0.0f) });
    }
}
