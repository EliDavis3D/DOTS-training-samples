using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct TargetWater : IComponentData
{
    public Entity water;
}
