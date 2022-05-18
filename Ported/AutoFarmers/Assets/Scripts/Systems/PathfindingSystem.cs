
using System.Text;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public enum SearchTask
{
    Silo,
    UntilledGround,
    Rock,
    Plant,
}

public enum NavigableType
{
    Farmer,
    Drone,
}

[BurstCompile]
partial struct PathfindingSystem : ISystem
{
    private int frame;
    
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
        if (frame++ % 180 != 0)
        {
            return;
        }
        
        // Get game config to get map size
        var gameConfig = SystemAPI.GetSingleton<GameConfig>();
        
        // Get ecb
        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
        
        // Get ground tiles buffer from ground singleton
        BufferFromEntity<GroundTile> groundData = state.GetBufferFromEntity<GroundTile>(true);
        Entity groundEntity = SystemAPI.GetSingletonEntity<Ground>();
        
        // temp
        //BufferFromEntity<Waypoint> waypointBufferFromEntity = state.GetBufferFromEntity<Waypoint>(true);
                
        var allocator = state.WorldUnmanaged.UpdateAllocator.ToAllocator;
        
        if (groundData.TryGetBuffer(groundEntity, out DynamicBuffer<GroundTile> groundBuffer))
        {
            // Initialize the job
            var findPathJob = new FindPath
            {
                // temp
                //waypointBufferFromEntity = waypointBufferFromEntity,
                VisitedTiles = CollectionHelper.CreateNativeArray<int>(gameConfig.MapSize.x * gameConfig.MapSize.y, allocator),
                ActiveTiles = new NativeList<int>(allocator),
                NextTiles = new NativeList<int>(allocator),
                OutputTiles = new NativeList<int>(allocator),
                
                DirsX = new int4(-1, 1, 0, 0),
                DirsY = new int4(0, 0, -1, 1),
                Ground = groundBuffer,
                MapSize = gameConfig.MapSize,
                Range = 2,
                RequiredZone = new RectInt(0, 0, gameConfig.MapSize.x, gameConfig.MapSize.y),
            
                ECB = ecb,
            };
            
            // Schedule execution in a single thread, and do not block main thread.
            findPathJob.Schedule();

            var testJob = new printJob();
            testJob.Schedule();
        }
    }
}

[BurstCompile]
partial struct FindPath : IJobEntity
{
    // temp
    //public BufferFromEntity<Waypoint> waypointBufferFromEntity;
    
    public int4 DirsX;
    public int4 DirsY;

    public NavigableType Navigator;
    public SearchTask Task;
    public DynamicBuffer<GroundTile> Ground;
    public int2 MapSize;
    public int Range;
    public RectInt RequiredZone;

    public NativeArray<int> VisitedTiles;
    public NativeList<int> ActiveTiles;
    public NativeList<int> NextTiles;
    public NativeList<int> OutputTiles;
    
    public EntityCommandBuffer ECB;

    void Execute(ref PathfindingAspect pathfinder)
    {
        int startX = pathfinder.CurrentCoordinates.x;
        int startY = pathfinder.CurrentCoordinates.y;
        
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

                for (int j = 0; j < 4; j++)
                {
                    int x2 = x + DirsX[j];
                    int y2 = y + DirsY[j];

                    if (x2 < 0 || y2 < 0 || x2 >= mapWidth || y2 >= mapHeight)
                    {
                        continue;
                    }

                    int hashedX2Y2 = Hash(x2, y2);
                    if (VisitedTiles[hashedX2Y2]==-1 || VisitedTiles[hashedX2Y2]>steps) {
                        
                        int hash = Hash(x2,y2);
                        if (GetNavigable(Navigator, x2, y2)) {
                            VisitedTiles[hashedX2Y2] = steps;
                            NextTiles.Add(hash);
                        }
                        if (x2 >= RequiredZone.xMin && x2 <= RequiredZone.xMax) {
                            if (y2 >= RequiredZone.yMin && y2 <= RequiredZone.yMax) {
                                if (CheckMatchingTile(Task, x2, y2)) {
                                    OutputTiles.Add(hash);
                                    
                                    pathfinder.ClearWaypoints();
                                    
                                    // Add the waypoints in the calculated path
                                    foreach (int indexInOutputTile in OutputTiles)
                                    {
                                        //temp
                                        //waypointBufferFromEntity[pathfinder.Self].Add(new Waypoint());
                                        
                                        pathfinder.AddWaypoint(new Waypoint()
                                        {
                                            TileIndex = indexInOutputTile
                                        });
                                    }

                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private bool CheckMatchingTile(SearchTask searchTask, int x, int y)
    {
        return true;
    }

    private bool GetNavigable(NavigableType navigator, int x, int y)
    {
        return true;
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

// Prints the Waypoints of every farmer
partial struct printJob : IJobEntity
{
    void Execute(in PathfindingAspect pathfinder)
    {
        var waypoints = pathfinder.Waypoints;
        
        StringBuilder sb = new StringBuilder();
        foreach (var waypoint in waypoints)
        {
            sb.Append($"[{waypoint.TileIndex}] ");
        }
        Debug.Log($"{pathfinder.Self}: {sb}");
    }
}
