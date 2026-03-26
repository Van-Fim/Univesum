using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class Chunk : MonoBehaviour
{
    public bool isHidden;
    public bool isDestroyed;
    public Coroutine coroutine;
    [Inject] SignalBus signalBus;
    [Inject] WorldChunkManager worldChunkManager;
    public List<Asteroid> asteroids = new List<Asteroid>();
    public List<int> asteroidFieldsIds = new List<int>();
    void Start()
    {
        if (isDestroyed)
        {
            return;
        }
        signalBus.Subscribe<SignalChunkFloatingOriginFix>(OnChunkFloatingOriginFix);
        signalBus.Subscribe<SignalChunkDestroy>(OnChunkDestroy);
    }
    public void OnChunkDestroy(SignalChunkDestroy signal)
    {
        isDestroyed = true;
        signalBus.Unsubscribe<SignalChunkFloatingOriginFix>(OnChunkFloatingOriginFix);
        signalBus.Unsubscribe<SignalChunkDestroy>(OnChunkDestroy);
        Destroy(this.gameObject);
    }
    public void OnChunkFloatingOriginFix(SignalChunkFloatingOriginFix signal)
    {
        if (isDestroyed)
        {
            return;
        }
        transform.localPosition -= signal.offset;
    }
    public void Hide()
    {
        if (isDestroyed)
        {
            return;
        }
        isHidden = true;
        name = "Destroyed";
        asteroids = new List<Asteroid>();
    }
}
