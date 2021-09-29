using System.Collections.Generic;
using Unity.Entities;
using UnityGameObject = UnityEngine.GameObject;
using UnityRangeAttribute = UnityEngine.RangeAttribute;
using UnityMonoBehaviour = UnityEngine.MonoBehaviour;

public class RailsSpawnerAuthoring : UnityMonoBehaviour
    , IConvertGameObjectToEntity,IDeclareReferencedPrefabs
{

    public UnityGameObject RailPrefab;
    [UnityRange(100, 1000)] public int RailsPerMeter = 100; //Not actually used yet
    
    public void DeclareReferencedPrefabs(List<UnityGameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(RailPrefab);
    }
    
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new RailsSpawner
        {
            RailPrefab = conversionSystem.GetPrimaryEntity(RailPrefab),
            NbRails = RailsPerMeter,
        });
    }
}
