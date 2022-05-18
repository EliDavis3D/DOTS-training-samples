using System.Resources;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

public partial struct DroneDepositingPlantSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        var plantsDepositted = 0;
        foreach (var drone in SystemAPI.Query<DroneDepositingPlantAspect>())
        {
            if (drone.AtDesiredLocation)
            {
                ecb.DestroyEntity(drone.Plant);
                drone.DesiredLocation = new int2(0, -1);
                ecb.RemoveComponent(drone.Self, typeof(DroneDepositPlantIntent));
                plantsDepositted++;
            }
        }

        if(plantsDepositted > 0)
        {
            var moneyEntity = SystemAPI.GetSingletonEntity<FarmMoney>();
            var money = SystemAPI.GetSingletonRW<FarmMoney>();

            money.FarmerMoney += 100 * plantsDepositted;
            money.DroneMoney += 300 * plantsDepositted;
            ecb.SetComponent(moneyEntity, money);
        }
        
    }
}
