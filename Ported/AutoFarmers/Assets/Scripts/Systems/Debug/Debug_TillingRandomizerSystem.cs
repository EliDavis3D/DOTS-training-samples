using System;
using Unity.Burst;
using Unity.Entities;

[BurstCompile]
public partial struct Debug_TillingRandomizerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.Enabled = false; // NOTE_nic: Not intended for delivery - debug test only

        state.RequireForUpdate<Ground>();
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        var config = SystemAPI.GetSingleton<GameConfig>();

        BufferFromEntity<GroundTile> groundData = state.GetBufferFromEntity<GroundTile>(false);
        Entity groundEntity = SystemAPI.GetSingletonEntity<Ground>();

        if (groundData.TryGetBuffer(groundEntity, out DynamicBuffer<GroundTile> bufferData))
        {
            Random randomGenerator = new Random((int)(state.Time.ElapsedTime * 100));
            int val = randomGenerator.Next(20);
            if (val == 0)
            {
                int index = randomGenerator.Next(config.MapSize.x * config.MapSize.y);
                bufferData[index] = new GroundTile
                {
                    tileState = (GroundTileState)randomGenerator.Next(4)
                };
            }
        }
    }
}
