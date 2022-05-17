using Unity.Entities;
using Unity.Rendering;

public readonly partial struct GroundTileAspect : IAspect<GroundTileAspect>
{
    readonly RefRW<GroundTileView> tileViewRef;
    readonly RefRW<MaterialMeshInfo> tileMaterialMeshInfo;

    public GroundTileView tileView => tileViewRef.ValueRW;
    public MaterialMeshInfo meshInfo { get => tileMaterialMeshInfo.ValueRW; set => tileMaterialMeshInfo.ValueRW = value; }
}
