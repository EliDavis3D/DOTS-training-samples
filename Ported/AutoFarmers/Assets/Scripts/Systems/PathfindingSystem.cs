
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
partial struct PathfindingSystem : ISystem
{
    public delegate bool IsNavigableDelegate(int x,int y);
    public delegate bool CheckMatchDelegate(int x,int y);
    
    public void OnCreate(ref SystemState state)
    {
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Get game config to get map size
        // var gameConfig = SystemAPI.GetSingleton<GameConfig>();
        //
        // // Get ground tiles buffer from ground singleton
        // BufferFromEntity<GroundTile> groundData = state.GetBufferFromEntity<GroundTile>(true);
        // Entity groundEntity = SystemAPI.GetSingletonEntity<Ground>();
        // if (groundData.TryGetBuffer(groundEntity, out DynamicBuffer<GroundTile> groundBuffer))
        // {
        //     // Initialize the job
        //     var turretShootJob = new Pathfind
        //     {
        //         Ground = groundBuffer,
        //         MapSize = gameConfig.MapSize,
        //     };
        //
        //     // Schedule execution in a single thread, and do not block main thread.
        //     turretShootJob.Schedule();
        // }
    }
}

[BurstCompile]
partial struct Pathfind : IJobEntity
{
    public NativeArray<int> DirsX;
    public NativeArray<int> DirsY;
    
    public DynamicBuffer<GroundTile> Ground;
    public int2 MapSize;
    public int Range;
    public FunctionPointer<PathfindingSystem.IsNavigableDelegate> IsNavigable;
    public FunctionPointer<PathfindingSystem.CheckMatchDelegate> CheckMatch;
    public RectInt RequiredZone;

    public NativeArray<int> VisitedTiles;
    public NativeList<int> ActiveTiles;
    public NativeList<int> NextTiles;
    public NativeList<int> OutputTiles;

    public DynamicBuffer<Waypoint> PathfinderWaypoints;

    void Execute(ref PathfindingAspect pathfinding)
    {
        int startX = pathfinding.CurrentCoordinates.x;
        int startY = pathfinding.CurrentCoordinates.y;
        
        int mapWidth = MapSize.x;
        int mapHeight = MapSize.y;

        for (int x=0;x<mapWidth;x++) {
            for (int y = 0; y < mapHeight; y++) {
                VisitedTiles[Hash(x,y)] = -1;
            }
        }
        OutputTiles.Clear();
        VisitedTiles[Hash(startX,startY)] = 0;
        ActiveTiles.Clear();
        NextTiles.Clear();
        NextTiles.Add(Hash(startX,startY));

        int steps = 0;

        while (NextTiles.Length > 0 && (steps<Range || Range==0)) {
            NativeList<int> temp = ActiveTiles;
            ActiveTiles = NextTiles;
            NextTiles = temp;
            NextTiles.Clear();

            steps++;

            for (int i=0;i<ActiveTiles.Length;i++) {
                int x, y;
                Unhash(ActiveTiles[i],out x,out y);

                for (int j=0;j<DirsX.Length;j++) {
                    int x2 = x + DirsX[j];
                    int y2 = y + DirsY[j];

                    if (x2<0 || y2<0 || x2>=mapWidth || y2>=mapHeight) {
                        continue;
                    }

                    int hashedX2Y2 = Hash(x2, y2);
                    if (VisitedTiles[hashedX2Y2]==-1 || VisitedTiles[hashedX2Y2]>steps) {

                        int hash = Hash(x2,y2);
                        if (IsNavigable.Invoke(x2,y2)) {
                            VisitedTiles[hashedX2Y2] = steps;
                            NextTiles.Add(hash);
                        }
                        if (x2 >= RequiredZone.xMin && x2 <= RequiredZone.xMax) {
                            if (y2 >= RequiredZone.yMin && y2 <= RequiredZone.yMax) {
                                if (CheckMatch.Invoke(x2,y2)) {
                                    OutputTiles.Add(hash);
                                    
                                    // Clear pathfinder's buffer and add the waypoints in the calculated path
                                    PathfinderWaypoints.Clear();
                                    foreach (int indexInOutputTile in OutputTiles)
                                    {
                                        PathfinderWaypoints.Add(new Waypoint()
                                        {
                                            TileIndex = indexInOutputTile
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private int Hash(int x, int y)
    {
        return MapSize.x * y + x;
    }

    private void Unhash(int index, out int x, out int y)
    {
        y = index / MapSize.x;
        x = index % MapSize.y;
    }
}