﻿using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct BotSpawner : IComponentData
{
    public Entity botPrefab;
    public int numberBots;
    public float spawnRadius;
    public float2 spawnCenter;
}