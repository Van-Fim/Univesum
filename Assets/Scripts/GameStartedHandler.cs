using UnityEngine;
using Zenject;

public class GameStartedHandler
{
    [Inject] private WorldChunkManager worldChunkManager;
    public void ChunkManagerReady(SignalChunkManagerReady signal)
    {
        worldChunkManager.isFirstChunksReady = true;
        Debug.Log("Chunk manager is ready");
    }
    public void HandleGameStarted(SignalGameStarted signal)
    {
        
        Debug.Log("Game Started Signal Received");
    }
}

