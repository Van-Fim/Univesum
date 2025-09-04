using UnityEngine;

public class SignalDestroyChunkAsteroids
{
    public Vector3Int ChunkCoord { get; }

    public SignalDestroyChunkAsteroids(Vector3Int chunkCoord)
    {
        ChunkCoord = chunkCoord;
    }
}
