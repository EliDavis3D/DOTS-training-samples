using Unity.Collections;
using Unity.Entities;

public enum GroundTileState : byte
{
    Open=0,
    Unpassable,
    Tilled,
    Planted,
}

public struct GroundTile : IBufferElementData
{
    public GroundTileState tileState;
}
