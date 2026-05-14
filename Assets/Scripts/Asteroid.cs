using System;
using Unity.VisualScripting;
using UnityEngine;
using Zenject;
public class Asteroid : SpaceObject, ISelectable
{
    [Inject]
    SignalBus _signalBus;
    Asteroid.Pool _pool;
    public WorldChunkManager worldChunkManager;
    private bool _isDespawned;
    public Chunk chunk;
    [Inject] DiContainer container;
    public float rotationSpeed= 1;
    private Vector3 currentRotationSpeed;
    private Vector3 targetRotationSpeed;
    private float smoothTime = 2f;

    public void ResetRotation()
    {
        // Начальная случайная скорость
        currentRotationSpeed = new Vector3(
            UnityEngine.Random.Range(-rotationSpeed, rotationSpeed),
            UnityEngine.Random.Range(-rotationSpeed, rotationSpeed),
            UnityEngine.Random.Range(-rotationSpeed, rotationSpeed)
        );

        // Целевая скорость (будет меняться со временем)
        SetNewRandomTarget();
    }

    void SetNewRandomTarget()
    {
        targetRotationSpeed = new Vector3(
            UnityEngine.Random.Range(-rotationSpeed, rotationSpeed),
            UnityEngine.Random.Range(-rotationSpeed, rotationSpeed),
            UnityEngine.Random.Range(-rotationSpeed, rotationSpeed)
        );
    }

    void Update()
    {
        if (!gameObject.activeSelf)
        {
            return;
        }
        currentRotationSpeed = Vector3.Lerp(currentRotationSpeed, targetRotationSpeed, Time.deltaTime / smoothTime);

        // Вращаем астероид
        transform.Rotate(currentRotationSpeed * Time.deltaTime);
    }

    public void SetPool(Asteroid.Pool pool)
    {
        _pool = pool;
    }
    Asteroid.Pool GetPool(string id)
    {
        return container.ResolveId<Asteroid.Pool>(id);
    }
    public override void OnTakeDamage(SpaceObjectOnTakeDamage signal)
    {
        if (signal.target == this)
        {
            shield -= signal.value;
            if (shield < 0)
            {
                hull -= -shield;
                shield = 0;
            }
            if (hull <= 0)
            {
                hull = 0;
                InvokeDestroyHide(signal.attacker);
            }
        }
    }
    public override void OnSpDestroyHide(SpaceObjectOnDestroyHide signal)
    {
        if (signal.target == this || signal.target == null)
        {
            Despawn();
        }
    }
    public override void OnDestroyAll(Type type)
    {
        Despawn();
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
            if (item.is_destroyed)
                return;
            item.transform.SetParent(null);
            item.Hide();
        }

        protected override void OnSpawned(Asteroid item)
        {
            if (item.is_destroyed)
                return;
            item.Show();
            item.ResetRotation();
        }
    }
    public void OnDestroyChunkAsteroids(SignalDestroyChunkAsteroids signal)
    {
        if (chunk != null && chunk.isHidden)
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
        if (TargetSelect.currentSelectedItem != null && TargetSelect.currentSelectedItem == this)
        {
            TargetSelect.currentSelectedItem.SetSpObject(null);
        }
        OnDespawned();
        _pool.Despawn(this);
    }
    public void OnSelect()
    {
        if (is_destroyed)
            return;
        canvasController.targetSelect.SetSpObject(this);
        TargetSelect.currentSelectedItem = canvasController.targetSelect;
        TargetSelect.InvokeSelect();
    }

    public void OnDeselect()
    {
        if (is_destroyed)
            return;
        canvasController.targetSelect.SetSpObject(null);
        TargetSelect.currentSelectedItem = null;
        TargetSelect.InvokeSelect();
    }

    public string GetLabel()
    {
        throw new System.NotImplementedException();
    }
}
