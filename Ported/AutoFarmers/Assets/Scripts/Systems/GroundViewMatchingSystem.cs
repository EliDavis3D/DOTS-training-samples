using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;

[BurstCompile]
public partial struct GroundVisualsSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
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

        Entity untilledPrefab = config.GroundTileUntilledPrefab;
        int untilledMaterialId = state.EntityManager.GetComponentData<MaterialMeshInfo>(untilledPrefab).Material;

        Entity tilledPrefab = config.GroundTileTilledPrefab;
        int tilledMaterialId = state.EntityManager.GetComponentData<MaterialMeshInfo>(tilledPrefab).Material;

        BufferFromEntity<GroundTile> groundData = state.GetBufferFromEntity<GroundTile>(true);
        Entity groundEntity = SystemAPI.GetSingletonEntity<Ground>();

        if( groundData.TryGetBuffer(groundEntity, out DynamicBuffer<GroundTile> bufferData) )
        {
            foreach (var instance in SystemAPI.Query<GroundTileAspect>())
            {
                GroundTileState tileState = bufferData[instance.tileView.Index].tileState;
                bool isTilled = GroundUtilities.StateIsTilled(tileState);

                if (isTilled != instance.tileView.Tilled)
                {
                    MaterialMeshInfo meshInfo = instance.meshInfo;
                    if ( isTilled )
                    {
                        meshInfo.Material = tilledMaterialId;
                    }
                    else
                    {
                        meshInfo.Material = untilledMaterialId;
                    }
                    instance.meshInfo = meshInfo;

                    GroundTileView tileView = instance.tileView;
                    tileView.Tilled = isTilled;
                    instance.tileView = tileView;
                }
            }
        }
    }
}

// on the ground entity singleton - maybe don't put in here?
//readonly RefRO<GroundTileState> tileStateBufferRef;
