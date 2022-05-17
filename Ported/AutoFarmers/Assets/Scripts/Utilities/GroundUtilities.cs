using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

static class GroundUtilities
{
    public static void GenerateGroundAndRocks(EntityCommandBuffer ecb, in GameConfig config, in Allocator allocator)
    {
        int2 mapSize = config.MapSize;

        NativeArray<Entity> groundTileEntities = CollectionHelper.CreateNativeArray<Entity>(
            mapSize.x * mapSize.y, allocator);
        ecb.Instantiate(config.GroundTileUntilledPrefab, groundTileEntities);

        // Create Ground
        Entity groundEntity = ecb.CreateEntity();
        ecb.AddComponent<Ground>(groundEntity);
        DynamicBuffer<GroundTile> groundTiles = ecb.AddBuffer<GroundTile>(groundEntity);
        groundTiles.Length = mapSize.x * mapSize.y;

        Random randomGenerator = new Random(124536789);
        for (int y = 0; y < mapSize.y; ++y)
        {
            for (int x = 0; x < mapSize.x; ++x)
            {
                int index = mapSize.x * y + x;

                GroundTileState newState = GroundTileState.Open;
                if (randomGenerator.NextInt(4) == 0)
                {
                    newState = GroundTileState.Tilled;
                }

                groundTiles[index] = new GroundTile
                {
                    tileState = newState
                };

                ecb.SetComponent(groundTileEntities[index], new GroundTileView
                {
                    Index = index
                    // Don't set the tilled state, otherwise the visualizer won't update it
                });
                ecb.SetComponent(groundTileEntities[index], new Translation
                {
                    Value = new float3(x, 0, y)
                });
            }
        }
    }

    public static bool StateIsTilled(GroundTileState state)
    {
        return state == GroundTileState.Tilled || state == GroundTileState.Planted;
    }
}
