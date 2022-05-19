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
            foreach (FarmerRockBreakingAspect instance in SystemAPI.Query<FarmerRockBreakingAspect>())
            {
                FarmerIntentState intentState = instance.intent.value;
                if (intentState != FarmerIntentState.SmashRocks) { continue; }

                if (instance.pathfindingIntent.destinationType == PathfindingDestination.None &&
                    instance.combat.combatTarget == Entity.Null)
                {
                    if (rockEntities.Length > 0 &&
                        instance.pathfindingIntent.destinationType == PathfindingDestination.None/* &&
                        TryAquireTarget(instance.translation, rockEntities, ref randomGenerator, config.PathfindingAcquisitionRange, ref state, out Targeting targeting, out int2 targetTile)*/)
                    {
                        instance.pathfindingIntent = new PathfindingIntent
                        {
                            navigatorType = NavigatorType.Farmer,
                            destinationType = PathfindingDestination.Rock,
                            RequiredZone = GroundUtilities.GetFullMapBounds(config)
                        };
                    }
                    else
                    {
                        instance.intent = CreateEmptyIntent(instance.intent.random);
                    }
                }
                else if (instance.combat.combatTarget == Entity.Null &&
                         IsInRangeOfPathfindingDestination(instance.translation, instance.PathfindingWaypoints, config.RockSmashActionRange, config.MapSize.x))
                {
                    if(instance.PathfindingWaypoints.Length > 0)
                    {
                        Waypoint destination = instance.PathfindingWaypoints.ElementAt(0);
                        Entity rockEntity = bufferData[destination.TileIndex].rockEntityByTile;
                        if(rockEntity != Entity.Null)
                        {
                            instance.combat = new FarmerCombat
                            {
                                combatTarget = rockEntity,
                                cooldownTicker = 0.0f
                            };
                        }
                        else
                        {
                            instance.intent = CreateEmptyIntent(instance.intent.random);
                        }
                    }
                    else
                    {
                        instance.intent = CreateEmptyIntent(instance.intent.random);
                    }
                }
                if(instance.combat.combatTarget != Entity.Null)
                {
                    if(!state.EntityManager.Exists(instance.combat.combatTarget))
                    {
                        instance.intent = CreateEmptyIntent(instance.intent.random);
                    }
                    else
                    {
                        float newCooldownTicker = instance.combat.cooldownTicker - state.Time.DeltaTime;
                        if (newCooldownTicker <= 0)
                        {
                            if (TryBreakRock(instance.combat.combatTarget, config.RockDamagePerHit, ref state, ref ecb))
                            {
                                GroundUtilities.DestroyRock(instance.combat.combatTarget, state.EntityManager, ecb, config, ref bufferData);

                                instance.combat = new FarmerCombat
                                {
                                    combatTarget = Entity.Null,
                                    cooldownTicker = 0.0f
                                };
                                instance.intent = CreateEmptyIntent(instance.intent.random);
                            }
                            else
                            {
                                instance.combat = new FarmerCombat
                                {
                                    combatTarget = instance.combat.combatTarget,
                                    cooldownTicker = config.FarmerAttackCooldown
                                };
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


                /*else if (!state.EntityManager.Exists(instance.targeting.entityTarget))
                //{
                    //instance.targeting = CreateEmptyTarget();
                    //instance.intent = CreateEmptyIntent(instance.intent.random);
                //}
                else if (IsAtTarget(instance.translation, instance.targeting, config.RockSmashActionRange, ref state))
                {
                    // @TODO: If close enough, convert final waypoint to rockEntity
                    float newCooldownTicker = instance.combat.cooldownTicker - state.Time.DeltaTime;
                    if (newCooldownTicker <= 0)
                    {
                        instance.combat = new FarmerCombat
                        {
                            cooldownTicker = config.FarmerAttackCooldown
                        };

                        if (TryBreakRock(instance.targeting, config.RockDamagePerHit, ref state, ref ecb))
                        {
                            GroundUtilities.DestroyRock(instance.targeting.entityTarget, state.EntityManager, ecb, config, ref bufferData);

                            instance.targeting = CreateEmptyTarget();
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
                }*/
            }
        }

        rockEntities.Dispose();
    }
    bool IsInRangeOfPathfindingDestination(in Translation translation, in DynamicBuffer<Waypoint> waypoints, in float rockBreakDist, in int mapWidth)
    {
        if (waypoints.Length == 0) return true;

        Waypoint destination = waypoints.ElementAt(0);

        float2 finalWaypointTranslation = GroundUtilities.GetTileTranslation(destination.TileIndex, mapWidth);

        float rockDistanceSquared = math.distancesq(finalWaypointTranslation, translation.Value.xz);

        return rockDistanceSquared < rockBreakDist * rockBreakDist;
    }
    /*bool TryAquireTarget(in Translation farmerTranslation, in NativeArray<Entity> rockEntities, ref Random randomGenerator, in float rockAgroDist, ref SystemState state, out Targeting targeting, out int2 targetTile)
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
            };
            targetTile = int2.zero;
            return false;
        }

        // @TODO: build path for travel, and actually verify the rock is reachable

        targetTile = (int2)math.floor(contactPoint);
        targeting = new Targeting
        {
            entityTarget = rockEntity,
            //tileTarget = targetTile
        };
        return true;
    }*/

    /*bool IsAtTarget(in Translation translation, in Targeting targeting, in float rockBreakDist, ref SystemState state)
    {
        Translation targetTranslation = state.EntityManager.GetComponentData<Translation>(targeting.entityTarget);
        Rock rock = state.EntityManager.GetComponentData<Rock>(targeting.entityTarget);

        float2 contactPoint = CalcRockClosestPoint(translation, targetTranslation, rock);
        float rockDistanceSquared = math.distancesq(contactPoint, translation.Value.xz);

        return rockDistanceSquared < rockBreakDist * rockBreakDist;
    }*/

    bool TryBreakRock(in Entity rockEntity, in float damagePerHit, ref SystemState state, ref EntityCommandBuffer ecb)
    {
        RockHealth rockHealth = state.EntityManager.GetComponentData<RockHealth>(rockEntity);
        float newHealth = rockHealth.Value - damagePerHit;
        bool didBreak = newHealth <= 0;

        ecb.SetComponent(rockEntity, new RockHealth
        {
            Value = newHealth
        });

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
    /*static Targeting CreateEmptyTarget()
    {
        return new Targeting
        {
            entityTarget = Entity.Null,
            tileTarget = int2.zero
        };
    }*/
}
