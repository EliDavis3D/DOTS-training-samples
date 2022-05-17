using Unity.Entities;
using Unity.Mathematics;

public static class GroundUtilities
{
    public static void GenerateGroundAndRocks(EntityCommandBuffer ecb, in int2 mapSize)
    {
        // Create Ground
        Entity groundEntity = ecb.CreateEntity();
        DynamicBuffer<GroundTile> groundTiles = ecb.AddBuffer<GroundTile>(groundEntity);
        groundTiles.Length = mapSize.x * mapSize.y;
        for (int i = 0; i < mapSize.x * mapSize.y; ++i)
        {
            groundTiles[i] = new GroundTile
            {
                tileState = GroundTileState.Open
            };
        }
    }
}
