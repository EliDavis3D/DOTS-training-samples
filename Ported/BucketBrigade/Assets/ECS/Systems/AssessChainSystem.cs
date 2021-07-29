using System.Net.Http.Headers;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

public class AssessChainSystem : SystemBase
{
    float nextAssessTime = 0.0f;

    protected override void OnCreate()
    {
        RequireSingletonForUpdate<GameConfigComponent>();
        RequireSingletonForUpdate<HeatMapElement>();
    }

    protected override void OnUpdate()
    {
        var curTime = UnityEngine.Time.time;
        if (nextAssessTime > curTime)
            return;
        var config = GetSingleton<GameConfigComponent>();
        var grid = config.SimulationSize;
        var flashPoint = config.FlashPoint;

        var query = GetEntityQuery(typeof(WaterTagComponent), typeof(Translation), typeof(WaterVolumeComponent));
        var waterTranslations = query.ToComponentDataArray<Translation>(Allocator.TempJob);
        var waterVolumes = query.ToComponentDataArray<WaterVolumeComponent>(Allocator.TempJob);
        var waterEntities = query.ToEntityArray(Allocator.TempJob);

        var heatMapEntity = GetSingletonEntity<HeatMapElement>();
        var heatMap = GetBuffer<HeatMapElement>(heatMapEntity);

        Entities
            .WithDisposeOnCompletion(waterTranslations)
            .WithDisposeOnCompletion(waterVolumes)
            .WithDisposeOnCompletion(waterEntities)
            .ForEach((in BotsChainComponent chain, in DynamicBuffer<BotChainElementData> chainBuffer) =>
            {
                var scooper = chain.scooper;
                var scooperPos = GetComponent<Translation>(scooper).Value;
                var thrower = chain.thrower;

                
                Entity water = GetComponent<TargetWater>(scooper).water;
                
                // Find closest water if current water source is not valid
                if (water == Entity.Null || GetComponent<WaterVolumeComponent>(water).Volume <= 0.0f)
                {
                    
                    var minDistance = float.MaxValue;
                    for (int i = 0; i < waterTranslations.Length; ++i)
                    {
                        var waterVolume = waterVolumes[i];
                        if (waterVolume.Volume <= 0.0f)
                            continue;

                        var waterTrans = waterTranslations[i];
                        var distance = math.lengthsq(scooperPos - waterTrans.Value);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            water = waterEntities[i];
                        }
                    }
                }

                if (water == Entity.Null)
                    return;

                var waterPos = GetComponent<Translation>(water).Value;

                // Find closest fire cell
                var bestFirePos = float3.zero;
                var minFireDistance = float.MaxValue;
                for (int i = 0; i < heatMap.Length; ++i)
                {
                    if (heatMap[i].temperature < flashPoint)
                        continue;

                    int col = i % grid;
                    int row = i / grid;
                    var firePos = new float3(col, 0.0f, row);
                    var distance = math.lengthsq(waterPos - firePos);
                    if (distance < minFireDistance)
                    {
                        minFireDistance = distance;
                        bestFirePos = firePos;
                    }
                }
                
                SetComponent(scooper, new TargetWater() {water = water});
                SetComponent(scooper, new BotDropOffLocation() {Value = waterPos.xz});

                SetComponent(thrower, new TargetLocationComponent() {location = bestFirePos.xz});

                for (int i = 1; i < chainBuffer.Length; ++i)
                {
                    var passerFull = chainBuffer[i].passerFull;
                    var pickUpPosFull = GetChainPosition(i - 1, chainBuffer.Length, waterPos.xz, bestFirePos.xz);
                    var dropOffPosFull = GetChainPosition(i, chainBuffer.Length, waterPos.xz, bestFirePos.xz);
                    SetComponent(passerFull, new BotPickUpLocation() {Value = pickUpPosFull});
                    SetComponent(passerFull, new BotDropOffLocation() {Value = dropOffPosFull});

                    var passerEmpty = chainBuffer[i].passerEmpty;
                    var pickUpPosEmpty = GetChainPosition(i - 1, chainBuffer.Length, bestFirePos.xz, waterPos.xz);
                    var dropOffPosEmpty = GetChainPosition(i, chainBuffer.Length, bestFirePos.xz, waterPos.xz);
                    SetComponent(passerEmpty, new BotPickUpLocation() {Value = pickUpPosEmpty});
                    SetComponent(passerEmpty, new BotDropOffLocation() {Value = dropOffPosEmpty});
                }
            }).Schedule();

        nextAssessTime = curTime + config.ChainAssessPeriod;
    }

    static float2 GetChainPosition(int _index, int _chainLength, float2 _startPos, float2 _endPos)
    {
        // adds two to pad between the SCOOPER AND THROWER
        float progress = (float) _index / _chainLength;
        float curveOffset = math.sin(progress * math.PI);

        // get Vec2 data
        float2 heading = _startPos - _endPos;
        float distance = math.length(heading);
        float2 direction = heading / distance;
        float2 perpendicular = new float2(direction.y, -direction.x);

        //Debug.Log("chain progress: " + progress + ",  curveOffset: " + curveOffset);
        return math.lerp(_startPos, _endPos, (float) _index / (float) _chainLength) + perpendicular * curveOffset;
    }
}