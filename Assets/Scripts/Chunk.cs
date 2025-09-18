using UnityEngine;
using Zenject;

public class Chunk : MonoBehaviour
{
    public bool isDestroyed;
    public Coroutine coroutine;
    [Inject] SignalBus signalBus;
    [Inject] WorldChunkManager worldChunkManager;
    void Start()
    {
        signalBus.Subscribe<SignalChunkFloatingOriginFix>(OnChunkFloatingOriginFix);
    }
    public void OnChunkFloatingOriginFix(SignalChunkFloatingOriginFix signal)
    {
        transform.localPosition -= signal.offset;
    }
    public void Destroy()
    {
        isDestroyed = true;
        name = "Destroyed";
    }
}
