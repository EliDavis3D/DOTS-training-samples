using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[BurstCompile]
public partial class FarmerFindTileToTillSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<GameConfig>();
        RequireForUpdate<Ground>();
    }

    [BurstCompile]
    protected override void OnUpdate()
    {
        var ground = SystemAPI.GetSingletonEntity<Ground>();
        BufferFromEntity<GroundTile> tileBufferEntity = GetBufferFromEntity<GroundTile>();
        DynamicBuffer<GroundTile> tiles;
        if (!tileBufferEntity.TryGetBuffer(ground, out tiles))
            return; // Should always exist, but be cautious.

        var config = SystemAPI.GetSingleton<GameConfig>();
        var ecbSystem = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSystem.CreateCommandBuffer(World.Unmanaged);

        int groundWidth = config.MapSize.x;
        int groundHeight = config.MapSize.y;
        int groundArea = groundWidth * groundHeight;
        float dt = Time.DeltaTime;

        Random random = new Random((uint)math.abs(Time.ElapsedTime) + 1);
        Entities.WithAll<Farmer>().WithNone<TillGroundTarget>().ForEach((Entity entity, FarmerIntent intent, ref Mover mover) =>
        {
            if (intent.value == FarmerIntentState.TillGround)
            {
                int tileIndex;
                if (TryGetRandomOpenTile(ref random, tiles, groundArea, out tileIndex))
                {
                    //float2 tileTranslation = GroundUtilities.GetTileTranslation(tileIndex, groundWidth);
                    int2 tileCoords = GroundUtilities.GetTileCoords(tileIndex, groundWidth);
                    mover.DesiredLocation = tileCoords;
                    mover.HasDestination = true;
                    tiles[tileIndex] = new GroundTile() { tileState = GroundTileState.Claimed };
                    ecb.AddComponent<TillGroundTarget>(entity, new TillGroundTarget { tileIndex = tileIndex, tileTranslation = tileCoords });
                }
            }
        }).Schedule();
    }

    private static bool TryGetRandomOpenTile(ref Random random, in DynamicBuffer<GroundTile> tiles, int groundArea, out int tileIndex)
    {
        tileIndex = 0;

        int attempts = 8;
        while (attempts > 0)
        {
            tileIndex = random.NextInt(groundArea);
            GroundTileState tileState = tiles[tileIndex].tileState;
            if (tileState == GroundTileState.Open || tileState == GroundTileState.Tilled) 
                return true;

            --attempts;
        }

        return false;
    }
}