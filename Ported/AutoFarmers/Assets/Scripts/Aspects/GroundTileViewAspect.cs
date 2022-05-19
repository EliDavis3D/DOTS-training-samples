using Unity.Entities;
using Unity.Rendering;

public readonly partial struct GroundTileAspect : IAspect<GroundTileAspect>
{
    readonly RefRW<GroundTileView> tileViewRef;
    readonly RefRW<MaterialMeshInfo> tileMaterialMeshInfo;

    public GroundTileView tileView { get => tileViewRef.ValueRO; set => tileViewRef.ValueRW = value; }
    public MaterialMeshInfo meshInfo { get => tileMaterialMeshInfo.ValueRO; set => tileMaterialMeshInfo.ValueRW = value; }
}
