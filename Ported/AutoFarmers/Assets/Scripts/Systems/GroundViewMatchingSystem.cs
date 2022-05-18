using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;

[BurstCompile]
public partial struct GroundViewMatchingSystem : ISystem
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

        Entity normalPrefab = config.GroundTileNormalPrefab;
        int normalMaterialId = state.EntityManager.GetComponentData<MaterialMeshInfo>(normalPrefab).Material;

        Entity tilledPrefab = config.GroundTileTilledPrefab;
        int tilledMaterialId = state.EntityManager.GetComponentData<MaterialMeshInfo>(tilledPrefab).Material;

        Entity unpassablePrefab = config.GroundTileUnpassablePrefab;
        int unpassableMaterialId = state.EntityManager.GetComponentData<MaterialMeshInfo>(unpassablePrefab).Material;

        Entity groundEntity = SystemAPI.GetSingletonEntity<Ground>();

        BufferFromEntity<GroundTile> groundDataLookup = state.GetBufferFromEntity<GroundTile>(true);
        if (groundDataLookup.TryGetBuffer(groundEntity, out DynamicBuffer<GroundTile> bufferData) )
        {
            foreach (var instance in SystemAPI.Query<GroundTileAspect>())
            {
                GroundTileState tileState = bufferData[instance.tileView.Index].tileState;

                if (tileState != instance.tileView.ViewState)
                {
                    MaterialMeshInfo meshInfo = instance.meshInfo;
                    if (GroundUtilities.IsTileTilled(tileState))
                    {
                        meshInfo.Material = tilledMaterialId;
                    }
                    else if (!GroundUtilities.IsTilePassable(tileState))
                    {
                        meshInfo.Material = unpassableMaterialId;
                    }
                    else
                    {
                        meshInfo.Material = normalMaterialId;
                    }
                    instance.meshInfo = meshInfo;

                    GroundTileView tileView = instance.tileView;
                    tileView.ViewState = tileState;
                    instance.tileView = tileView;
                }
            }
        }
    }
}

// on the ground entity singleton - maybe don't put in here?
//readonly RefRO<GroundTileState> tileStateBufferRef;
