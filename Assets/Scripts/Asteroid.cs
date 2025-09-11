using Unity.VisualScripting;
using UnityEngine;
using Zenject;
public class Asteroid : MonoBehaviour
{
    public Vector3Int chunkCoord;
    [Inject]
    SignalBus _signalBus;
    Asteroid.Pool _pool;
    public WorldChunkManager worldChunkManager;
    private bool _isDespawned;
    public Transform chunkTransform;
    [Inject] DiContainer container;
    public void SetPool(Asteroid.Pool pool)
    {
        _pool = pool;
    }
    Asteroid.Pool GetPool(string id)
    {
        return container.ResolveId<Asteroid.Pool>(id);
    }

    public void OnSpawned()
    {
        _isDespawned = false;
        _signalBus.Subscribe<SignalDestroyChunkAsteroids>(OnDestroyChunkAsteroids);
    }
    public void OnDespawned()
    {
        _signalBus.Unsubscribe<SignalDestroyChunkAsteroids>(OnDestroyChunkAsteroids);
    }

    [Inject]
    void Construct(SignalBus signalBus)
    {
        signalBus.Subscribe<SignalDestroyChunkAsteroids>(OnDestroyChunkAsteroids);
    }
    public class Pool : MonoMemoryPool<Asteroid>
    {
        AsteroidConfig config;
        public void Configure(AsteroidConfig cfg)
        {
            config = cfg;
            this.Resize(cfg.poolSize);
        }
        protected override void OnDespawned(Asteroid item)
        {
            item.transform.SetParent(item.worldChunkManager.worldTransform);
            item.gameObject.SetActive(false);
        }

        protected override void OnSpawned(Asteroid item)
        {
            item.gameObject.SetActive(true);
        }
    }
    public void OnDestroyChunkAsteroids(SignalDestroyChunkAsteroids signal)
    {
        if (chunkCoord == signal.ChunkCoord)
        {
            Despawn();
        }
    }
    public void Despawn()
    {
        if (_isDespawned) return;
        _isDespawned = true;
        OnDespawned();
        _pool.Despawn(this);
    }
}
