using UnityEngine;
using Zenject;

public class GameStartedHandler
{
    [Inject] private WorldChunkManager worldChunkManager;
    public void HandleGameStarted(SignalGameStarted signal)
    {
        worldChunkManager.isFirstChunksReady = true;
        Debug.Log("Game Started Signal Received");
    }
}

