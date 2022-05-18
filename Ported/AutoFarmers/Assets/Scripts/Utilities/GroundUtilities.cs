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
        ecb.Instantiate(config.GroundTileNormalPrefab, groundTileEntities);

        // Create Ground
        Entity groundEntity = ecb.CreateEntity();
        ecb.AddComponent<Ground>(groundEntity);
        DynamicBuffer<GroundTile> groundData = ecb.AddBuffer<GroundTile>(groundEntity);
        groundData.Length = mapSize.x * mapSize.y;

        for (int y = 0; y < mapSize.y; ++y)
        {
            for (int x = 0; x < mapSize.x; ++x)
            {
                int index = mapSize.x * y + x;

                GroundTileState newState = GroundTileState.Open;

                groundData[index] = new GroundTile
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

        Random randomGenerator = new Random((uint)config.WorldGenerationSeed);
        for (int i=0; i<config.InitialRockAttempts; ++i)
        {
            TryGenerateRock(ecb, config, ref groundData, ref randomGenerator);
        }
    }

    #region Rock Lifecycle Helpers
    public static bool TryGenerateRock(
        EntityCommandBuffer ecb,
        in GameConfig config,
        ref DynamicBuffer<GroundTile> groundData,
        ref Random randomGenerator)
    {

        float2 size = new float2(
            randomGenerator.NextFloat(config.MinRockSize, config.MaxRockSize),
            randomGenerator.NextFloat(config.MinRockSize, config.MaxRockSize));

        float2 topLeft = new float2(
            randomGenerator.NextFloat(0, config.MapSize.x - size.x),
            randomGenerator.NextFloat(0, config.MapSize.y - size.y));

        int2 minTile = (int2)math.floor(topLeft);
        int2 maxTile = (int2)math.floor(topLeft + size);

        if(!AreAllTilesInRangeOpen(groundData, minTile, maxTile, config.MapSize.x))
        {
            return false;
        }

        SetAllTilesInRangeTo(GroundTileState.Unpassable, ref groundData, minTile, maxTile, config.MapSize.x);

        float depth = randomGenerator.NextFloat(config.MinRockDepth, config.MaxRockDepth);

        float3 rockSize = new float3(size.x, depth, size.y);
        float3 rockCenter = new float3(topLeft.x + size.x/2 - 0.5f, depth / 2, topLeft.y + size.y/ 2 - 0.5f);

        float health = (size.x) * (size.y) * config.RockHealthPerUnitArea;

        Entity rockEntity = ecb.Instantiate(config.RockPrefab);
        ecb.SetComponent(rockEntity, new Translation
        {
            Value = rockCenter
        });
        ecb.AddComponent(rockEntity, new NonUniformScale
        {
            Value = rockSize
        });
        ecb.SetComponent(rockEntity, new Rock
        {
            size = rockSize
        });
        ecb.SetComponent(rockEntity, new RockHealth
        {
            Value = health,
        });

        return true;
    }

    public static void DestroyRock(
        in Entity rockEntity,
        in EntityManager entityManager,
        EntityCommandBuffer ecb,
        in GameConfig config,
        ref DynamicBuffer<GroundTile> groundData)
    {
        Translation rockTranslation = entityManager.GetComponentData<Translation>(rockEntity);
        NonUniformScale rockScale = entityManager.GetComponentData<NonUniformScale>(rockEntity);

        int2 minTile = math.clamp((int2)math.floor(rockTranslation.Value.xz - rockScale.Value.xz/2), int2.zero, config.MapSize);
        int2 maxTile = math.clamp((int2)math.floor(rockTranslation.Value.xz + rockScale.Value.xz/2), int2.zero, config.MapSize);

        SetAllTilesInRangeTo(GroundTileState.Open, ref groundData, minTile, maxTile, config.MapSize.x);

        ecb.DestroyEntity(rockEntity);
    }
    #endregion

    #region Tile Access Helpers
    public static bool IsTileTilled(GroundTileState state)
    {
        return state == GroundTileState.Tilled || state == GroundTileState.Planted;
    }
    public static bool IsTilePassable(GroundTileState state)
    {
        return state != GroundTileState.Unpassable;
    }


    public static bool AreAllTilesInRangeOpen(
        in DynamicBuffer<GroundTile> groundData,
        in int2 minTile, in int2 maxTile, in int mapWidth)
    {
        for (int y = minTile.y; y <= maxTile.y; ++y)
        {
            for (int x = minTile.x; x <= maxTile.x; ++x)
            {
                int index = y * mapWidth + x;
                if (groundData[index].tileState != GroundTileState.Open)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public static void SetAllTilesInRangeTo( GroundTileState state,
        ref DynamicBuffer<GroundTile> groundData,
        in int2 minTile, in int2 maxTile, in int mapWidth)
    {
        for (int y = minTile.y; y <= maxTile.y; ++y)
        {
            for (int x = minTile.x; x <= maxTile.x; ++x)
            {
                int index = y * mapWidth + x;
                groundData[index] = new GroundTile { tileState = state };
            }
        }
    }
    #endregion
}
