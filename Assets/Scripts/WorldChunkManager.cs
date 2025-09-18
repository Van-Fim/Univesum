using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class WorldChunkManager : MonoBehaviour
{
    List<Chunk> chunks = new List<Chunk>();
    [Inject] public PlayerController playerController;
    [Inject] SignalBus signalBus;
    [Inject] private List<AsteroidFieldConfig> asteroidConfigs;

    public Vector3 worldPos = Vector3.zero;

    public Transform playerTransform;
    public float originThreshold = 25000f;
    public int chunkSize = 20000;

    public Vector3Int currentChunk;
    public Dictionary<Vector3Int, Chunk> loadedChunks = new();
    Queue<(Chunk, Vector3Int)> spawnQueue = new();
    bool isSpawningChunks = false;
    public bool isFirstChunksReady = false;
    bool is_initialized;
    [Inject] DiContainer container;

    Asteroid.Pool GetPool(string id)
    {
        return container.ResolveId<Asteroid.Pool>(id);
    }

    void EnqueueChunkSpawn(Chunk chunk, Vector3Int chunkCoord)
    {
        spawnQueue.Enqueue((chunk, chunkCoord));
        if (!isSpawningChunks)
        {
            if (chunk.coroutine != null)
            {
                StopCoroutine(chunk.coroutine);
            }
            chunk.coroutine = StartCoroutine(ChunkSpawnWorker());
        }
    }

    IEnumerator ChunkSpawnWorker()
    {
        isSpawningChunks = true;

        while (spawnQueue.Count > 0)
        {
            var (chunk, chunkCoord) = spawnQueue.Dequeue();
            yield return SpawnAsteroidsAsync(chunk, chunkCoord);
            yield return null; // пауза между чанками
        }

        isSpawningChunks = false;
    }

    public void Start()
    {
        is_initialized = true;
        playerTransform = playerController.transform;
        UpdateCurrentChunk();
        UpdateChunksAround(currentChunk);
        Tick();
        signalBus.Fire(new SignalChunkManagerReady());
    }
    void Update()
    {
        if (!is_initialized)
        {
            return;
        }
        Tick();
    }
    void Tick()
    {
        HandleFloatingOrigin();
        HandleChunks();
    }
    Asteroid SpawnAsteroid(Chunk chunk, Vector3Int chunkCoord)
    {
        string configName = "AsteroidField01";
        AsteroidFieldConfig cfg = asteroidConfigs.Find(x => x.name == configName);
        if (cfg == null)
        {
            return null;
        }
        AsteroidFieldItemConfig astItem = cfg.asteroids[0];
        if (astItem == null)
        {
            return null;
        }
        float scale = Random.Range(astItem.scaleMin, astItem.scaleMax);
        Vector3 localOffset = new Vector3(
    Random.Range(-chunkSize / 2f, chunkSize / 2f),
    Random.Range(-chunkSize / 2f, chunkSize / 2f),
    Random.Range(-chunkSize / 2f, chunkSize / 2f)
);

        int rotX = Random.Range(0, 180 + 1);
        int rotY = Random.Range(0, 180 + 1);
        int rotZ = Random.Range(0, 180 + 1);
        Asteroid.Pool pool = GetPool($"{configName}_{astItem.name}");
        Asteroid asteroid = pool.Spawn();
        asteroid.Init();
        SpaceObjectConfig asteroidSp = JsonConfigLoader.LoadFromResources<SpaceObjectConfig>(astItem.spaceObjectPath);
        asteroid.InstallConfig(asteroidSp);
        asteroid.OnSpawned();
        asteroid.SetPool(pool);
        asteroid.worldChunkManager = this;
        asteroid.chunk = chunk;
        asteroid.transform.SetParent(chunk.transform);
        asteroid.transform.localScale = new Vector3(scale, scale, scale);
        asteroid.transform.localPosition = localOffset;
        asteroid.transform.localEulerAngles = new Vector3(rotX, rotY, rotZ);
        return asteroid;
    }
    void SpawnAsteroids(Chunk chunk, Vector3Int chunkCoord)
    {
        if (chunk.isDestroyed)
        {
            return;
        }
        Random.InitState($"{chunkCoord}".GetHashCode());
        int count = Random.Range(4, 5);

        for (int i = 0; i < count; i++)
        {
            if (chunk == null)
            {
                break;
            }
            Random.InitState($"{chunkCoord}{i}".GetHashCode());

            Asteroid asteroid = SpawnAsteroid(chunk, chunkCoord);

        }
    }
    IEnumerator SpawnAsteroidsAsync(Chunk chunk, Vector3Int chunkCoord)
    {
        if (chunk.isDestroyed)
        {
            yield return null;
        }
        int count = Random.Range(4, 5);

        for (int i = 0; i < count; i++)
        {
            if (chunk == null)
                yield break;

            Random.InitState($"{chunkCoord}{i}".GetHashCode());
            Asteroid asteroid = SpawnAsteroid(chunk, chunkCoord);
            yield return null; // вместо WaitForSeconds — быстрее и легче
        }
    }

    void HandleFloatingOrigin()
    {
        if (playerTransform.position.magnitude > originThreshold)
        {
            Vector3 offset = playerTransform.position;
            worldPos -= offset;
            signalBus.Fire(new SignalChunkFloatingOriginFix(offset));
            playerTransform.position = Vector3.zero;

            Rigidbody rb = playerTransform.GetComponent<Rigidbody>();
            if (rb != null)
                rb.position = Vector3.zero;

            // Debug.Log("Floating Origin Shifted by " + offset);
            UpdateCurrentChunk();
        }
    }

    void UpdateCurrentChunk()
    {
        Vector3 globalPosition = -(worldPos - playerTransform.localPosition);
        currentChunk = new Vector3Int(
            Mathf.FloorToInt(globalPosition.x / chunkSize),
            Mathf.FloorToInt(globalPosition.y / chunkSize),
            Mathf.FloorToInt(globalPosition.z / chunkSize)
        );
    }

    void HandleChunks()
    {
        Vector3 globalPosition = -(worldPos - playerTransform.localPosition);

        Vector3Int newChunk = new Vector3Int(
            Mathf.FloorToInt(globalPosition.x / chunkSize),
            Mathf.FloorToInt(globalPosition.y / chunkSize),
            Mathf.FloorToInt(globalPosition.z / chunkSize)
        );

        if (newChunk != currentChunk)
        {
            currentChunk = newChunk;
            UpdateChunksAround(currentChunk);
        }
    }

    void UpdateChunksAround(Vector3Int centerChunk)
    {
        int loadRadius = 3; // сколько чанков вокруг загружать

        HashSet<Vector3Int> requiredChunks = new();

        for (int x = -loadRadius; x <= loadRadius; x++)
            for (int y = -loadRadius; y <= loadRadius; y++)
                for (int z = -loadRadius; z <= loadRadius; z++)
                {
                    Vector3Int chunkCoord = centerChunk + new Vector3Int(x, y, z);
                    requiredChunks.Add(chunkCoord);

                    if (!loadedChunks.ContainsKey(chunkCoord))
                    {
                        Chunk chunk = GenerateChunk(chunkCoord);
                        loadedChunks[chunkCoord] = chunk;
                    }
                }

        // Выгружаем лишние чанки
        List<Vector3Int> toRemove = new();
        foreach (var kvp in loadedChunks)
        {
            if (!requiredChunks.Contains(kvp.Key))
            {
                kvp.Value.Destroy();
                toRemove.Add(kvp.Key);
            }
        }
        signalBus.Fire(new SignalDestroyChunkAsteroids());
        foreach (var key in toRemove)
        {
            loadedChunks.Remove(key);
        }
    }

    Chunk GenerateChunk(Vector3Int chunkCoord)
    {
        Chunk chunk = chunks.Find(x => x.isDestroyed == true);

        if (chunk == null)
        {
            chunk = new GameObject().AddComponent<Chunk>();
            container.Inject(chunk);
            chunks.Add(chunk);
        }
        chunk.name = "Chunk_" + chunkCoord;
        chunk.isDestroyed = false;
        chunk.transform.localPosition = worldPos + (chunkCoord * chunkSize);
        if (isFirstChunksReady)
        {
            EnqueueChunkSpawn(chunk, chunkCoord);
        }
        else
        {
            SpawnAsteroids(chunk, chunkCoord);
        }
        return chunk;
    }

}
