using Unity.VisualScripting;
using UnityEngine;
using Zenject;
public class Asteroid : SpaceObject
{
    [Inject]
    SignalBus _signalBus;
    Asteroid.Pool _pool;
    public WorldChunkManager worldChunkManager;
    private bool _isDespawned;
    public Chunk chunk;
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
        Show();
        _signalBus.Subscribe<SignalDestroyChunkAsteroids>(OnDestroyChunkAsteroids);
    }
    public void OnDespawned()
    {
        Hide();
        _signalBus.Unsubscribe<SignalDestroyChunkAsteroids>(OnDestroyChunkAsteroids);
    }
    public class Pool : MonoMemoryPool<Asteroid>
    {
        AsteroidFieldItemConfig config;
        public void Configure(AsteroidFieldItemConfig cfg)
        {
            config = cfg;
            this.Resize(cfg.poolSize);
        }
        protected override void OnDespawned(Asteroid item)
        {
            item.transform.SetParent(null);
            item.Hide();
        }

        protected override void OnSpawned(Asteroid item)
        {
            item.Show();
        }
    }
    public void OnDestroyChunkAsteroids(SignalDestroyChunkAsteroids signal)
    {
        if (chunk != null && chunk.isDestroyed)
        {
            Despawn();
        }
        else if (chunk == null)
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
