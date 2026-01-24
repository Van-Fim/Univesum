using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class Chunk : MonoBehaviour
{
    public bool isDestroyed;
    public Coroutine coroutine;
    [Inject] SignalBus signalBus;
    [Inject] WorldChunkManager worldChunkManager;
    public List<Asteroid> asteroids = new List<Asteroid>();
    public List<int> asteroidFieldsIds = new List<int>();
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
        asteroids = new List<Asteroid>();
    }
}
