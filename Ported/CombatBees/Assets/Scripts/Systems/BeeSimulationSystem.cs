using TreeEditor;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Mathematics;

class BeeSimulationSystem: SystemBase
{
    uint m_seed;
    protected override void OnCreate()
    {
        m_seed = 1234;
    }

    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;
        const float aggressivity = 0.5f;
        const float speed = 1.0f;
        const float beeSize = 0.01f;
        m_seed = m_seed + 1;
        var seed = m_seed;

        EntityCommandBuffer ecb = new EntityCommandBuffer( Allocator.TempJob );
        var parallelECB = ecb.AsParallelWriter();
        var teamAQuery = GetEntityQuery(ComponentType.ReadOnly<TeamA>());
        NativeArray<Entity> teamABees = teamAQuery.ToEntityArray( Allocator.TempJob );
        var teamBQuery = GetEntityQuery(ComponentType.ReadOnly<TeamB>());
        NativeArray<Entity> teamBBees = teamBQuery.ToEntityArray( Allocator.TempJob );
        var resourceQuery = GetEntityQuery(ComponentType.ReadOnly<Resource>());
        NativeArray<Entity> resources = resourceQuery.ToEntityArray( Allocator.TempJob );

        var capacity = teamABees.Length + teamBBees.Length;
        NativeList<Entity> list = new NativeList<Entity>(capacity, Allocator.TempJob);
        var parallelList = list.AsParallelWriter();
        var rng1 = new Random((uint) UnityEngine.Random.Range(1, 100000));
        
        Entities
            .WithReadOnly(teamABees)
            .WithReadOnly(teamBBees)
            .WithReadOnly(resources)
            .ForEach((Entity entity, ref Bee bee, ref NewTranslation pos) =>
            {

                var rng = Random.CreateFromIndex(seed*175834927 + (uint) entity.Index);
                rng.NextFloat();
                
                // set target if not set
                if(bee.Target == Entity.Null && bee.State != BeeState.ReturningToBase)
                {
                    bool fetchResource = resources.Length > 0 && rng.NextFloat() > aggressivity;
                    if(!fetchResource)
                    {
                        if(HasComponent<TeamA>(entity))
                        {
                            if(teamBBees.Length>0)
                            {
                                int beeIdx = rng.NextInt(teamBBees.Length);
                                bee.Target = teamBBees[beeIdx];
                                bee.State = BeeState.ChasingEnemy;
                            }
                            else
                                fetchResource = true;
                        }
                        else
                        {
                            if(teamABees.Length>0)
                            {
                                int beeIdx = rng.NextInt(teamABees.Length);
                                bee.Target = teamABees[beeIdx];
                                bee.State = BeeState.ChasingEnemy;
                            }
                            else
                                fetchResource = true;
                        }
                    }
                    if(fetchResource && resources.Length > 0)
                    {
                        int resourceIdx = rng.NextInt(resources.Length);
                        bee.Target = resources[resourceIdx];
                        bee.State = BeeState.GettingResource;
                    }
                }

                // move bee towards the target
                if(bee.Target != Entity.Null && !HasComponent<Translation>(bee.Target))
                {
                    bee.Target = Entity.Null;
                    bee.State = BeeState.Idle;
                    return;
                }
                float3 basePos = HasComponent<TeamA>(entity) ? new float3(-2.0f, 0.5f, 0.0f) : new float3(2.0f, 0.5f, 0.0f);
                var targetPos = bee.Target == Entity.Null ? basePos : GetComponent<Translation>(bee.Target).Value;
                float3 targetVec = targetPos - pos.translation.Value;
                float3 dir = math.normalize(targetVec);
                pos.translation.Value += dir * speed * deltaTime;

                // check collision with target
                const float targetSize = 0.01f;/*todo: set target size */
                float collR2 = beeSize + targetSize;
                collR2 *= collR2;
                if(math.lengthsq(targetVec) < collR2)
                {
                    // this?
                    parallelList.AddNoResize(entity);
                    // or this?
                    //parallelECB.AddComponent(0, entity, new NeedsStateChange());
                }
            }).ScheduleParallel();
        Dependency.Complete();
        ecb.Playback(EntityManager);
        ecb.Dispose();
        teamABees.Dispose();
        teamBBees.Dispose();
        resources.Dispose();

        foreach (var beeEntity in list)
        {
            if (!HasComponent<Translation>(beeEntity))
                continue;
            var bee = GetComponent<Bee>(beeEntity);
            switch(bee.State)
            {
/*                        case BeeState.Idle:
                        {
                        } break;
*/
                case BeeState.GettingResource:
                {
                    var resource = GetComponent<Resource>(bee.Target);
                    bee.resource = bee.Target;
                    bee.Target = Entity.Null;
                    if (resource.CarryingBee == Entity.Null)
                    {
                        resource.CarryingBee = beeEntity;
                        SetComponent(bee.resource, resource);
                        bee.State = BeeState.ReturningToBase;
                    }
                    else
                    {
                        // chase the damn bee?
                        // for now:
                        bee.resource = Entity.Null;
                        bee.Target = Entity.Null;
                        bee.State = BeeState.Idle;
                    }
                } break;

                case BeeState.ReturningToBase:
                {
                    var resource = GetComponent<Resource>(bee.resource);
                    resource.CarryingBee = Entity.Null;
                    SetComponent(bee.resource, resource); // write it back
                    bee.resource = Entity.Null;
                    bee.Target = Entity.Null;
                    bee.State = BeeState.Idle;
                } break;

                case BeeState.ChasingEnemy:
                {
                    if (HasComponent<Translation>(bee.Target))
                    {
                        var enemyBee = GetComponent<Bee>(bee.Target);
                        if (enemyBee.resource != Entity.Null)
                        {
                            // drop the resource
                            var resource = GetComponent<Resource>(enemyBee.resource);
                            resource.CarryingBee = Entity.Null;
                            SetComponent(enemyBee.resource, resource);
                        }

                        EntityManager.DestroyEntity(bee.Target);
                        bee.Target = Entity.Null;
                        bee.State = BeeState.Idle;
                    }
                } break;
            }
            
            SetComponent(beeEntity, bee);
        }

        list.Dispose();


    }
}
