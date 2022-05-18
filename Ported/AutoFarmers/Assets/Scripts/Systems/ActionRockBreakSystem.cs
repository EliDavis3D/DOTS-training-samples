using System.Resources;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

[BurstCompile]
public partial struct ActionRockBreakSystem : ISystem
{
    EntityQuery rockQuery;

    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Ground>();
        rockQuery = state.EntityManager.CreateEntityQuery(typeof(Rock));
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var config = SystemAPI.GetSingleton<GameConfig>();

        var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
        var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

        Random randomGenerator = new Random((uint)(state.Time.ElapsedTime * 100));
        NativeArray<Entity> rockEntities = rockQuery.ToEntityArray(Allocator.Temp);

        BufferFromEntity<GroundTile> groundData = state.GetBufferFromEntity<GroundTile>(false);
        Entity groundEntity = SystemAPI.GetSingletonEntity<Ground>();

        if (groundData.TryGetBuffer(groundEntity, out DynamicBuffer<GroundTile> bufferData))
        {
            foreach (FarmerTargetingAspect instance in SystemAPI.Query<FarmerTargetingAspect>())
            {
                FarmerIntentState intentState = instance.intent.value;
                if (intentState != FarmerIntentState.SmashRocks) { continue; }

                if (instance.targeting.entityTarget == Entity.Null)
                {
                    if (TryAquireTarget(instance.translation, rockEntities, ref randomGenerator, config.RockSmashAgroRange, ref state, out Targeting targeting))
                    {
                        instance.targeting = targeting;
                    }
                    else
                    {
                        instance.intent = CreateEmptyIntent(instance.intent.random);
                    }
                }
                else if (!state.EntityManager.Exists(instance.targeting.entityTarget))
                {
                    instance.intent = CreateEmptyIntent(instance.intent.random);
                }
                else if (IsAtTarget(instance.translation, instance.targeting, config.RockSmashActionRange, ref state))
                {
                    float newCooldownTicker = instance.combat.cooldownTicker - state.Time.DeltaTime;
                    if (newCooldownTicker <= 0)
                    {
                        instance.combat = new FarmerCombat
                        {
                            cooldownTicker = config.FarmerAttackCooldown
                        };
                        if (TryBreakRock(instance.targeting, config.RockDamagePerHit, ref state))
                        {
                            GroundUtilities.DestroyRock(instance.targeting.entityTarget, state.EntityManager, ecb, config, ref bufferData);
                            instance.intent = CreateEmptyIntent(instance.intent.random);
                        }
                    }
                    else
                    {
                        instance.combat = new FarmerCombat
                        {
                            cooldownTicker = newCooldownTicker
                        };
                    }
                }
            }
        }

        rockEntities.Dispose();
    }

    bool TryAquireTarget(in Translation farmerTranslation, in NativeArray<Entity> rockEntities, ref Random randomGenerator, in float rockAgroDist, ref SystemState state, out Targeting targeting)
    {
        int index = randomGenerator.NextInt(0, rockEntities.Length);

        Entity rockEntity = rockEntities[index];
        Translation rockTranslation = state.EntityManager.GetComponentData<Translation>(rockEntity);
        Rock rock = state.EntityManager.GetComponentData<Rock>(rockEntities[index]);
        
        float2 contactPoint = CalcRockClosestPoint(farmerTranslation, rockTranslation, rock);
        float rockDistanceSquared = math.distancesq(contactPoint, farmerTranslation.Value.xz);

        if (rockDistanceSquared >= rockAgroDist * rockAgroDist)
        {
            targeting = new Targeting
            {
                entityTarget = Entity.Null,
                tileTarget = int2.zero
            };
            return false;
        }

        // @TODO: build path for travel, and actually verify the rock is reachable

        int2 targetTile = (int2)math.floor(contactPoint);
        targeting = new Targeting
        {
            entityTarget = rockEntity,
            tileTarget = targetTile
        };
        return true;
    }

    bool IsAtTarget(in Translation translation, in Targeting targeting, in float rockBreakDist, ref SystemState state)
    {
        return true;
        // @TODO: actually get movement sorted so farmers approach
        Translation targetTranslation = state.EntityManager.GetComponentData<Translation>(targeting.entityTarget);
        Rock rock = state.EntityManager.GetComponentData<Rock>(targeting.entityTarget);

        float2 contactPoint = CalcRockClosestPoint(translation, targetTranslation, rock);
        float rockDistanceSquared = math.distancesq(contactPoint, translation.Value.xz);

        return rockDistanceSquared < rockBreakDist * rockBreakDist;
    }

    bool TryBreakRock(in Targeting targeting, in float damagePerHit, ref SystemState state)
    {
        RockHealth rockHealth = state.EntityManager.GetComponentData<RockHealth>(targeting.entityTarget);
        float newHealth = rockHealth.Value - damagePerHit;
        bool didBreak = newHealth <= 0;

        return didBreak;
    }

    float2 CalcRockClosestPoint(in Translation translation, in Translation rockTranslation, in Rock rock)
    {
        float3 size = rock.size;

        return math.clamp(
            translation.Value.xz,
            rockTranslation.Value.xz - size.xz / 2,
            rockTranslation.Value.xz + size.xz / 2);
    }

    static FarmerIntent CreateEmptyIntent(in Random random)
    {
        return new FarmerIntent
        {
            value = FarmerIntentState.None,
            random = random,
            elapsed = 0
        };
    }
}
