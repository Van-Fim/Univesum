using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class WorldChunkManager : MonoBehaviour
{
    List<Chunk> chunks = new List<Chunk>();
    [Inject] public PlayerController playerController;
    [Inject] SignalBus signalBus;

    public Transform playerTransform;
    public Transform worldTransform; // THEWORLD
    public float originThreshold = 25000f;
    public int chunkSize = 20000;

    public Vector3Int currentChunk;
    public Dictionary<Vector3Int, Chunk> loadedChunks = new();
    Queue<(Transform, Vector3Int)> spawnQueue = new();
    bool isSpawningChunks = false;
    public bool isFirstChunksReady = false;
    [Inject] DiContainer container;

    Asteroid.Pool GetPool(string id)
    {
        return container.ResolveId<Asteroid.Pool>(id);
    }

    void EnqueueChunkSpawn(Transform chunkTransform, Vector3Int chunkCoord)
    {
        spawnQueue.Enqueue((chunkTransform, chunkCoord));
        if (!isSpawningChunks)
            StartCoroutine(ChunkSpawnWorker());
    }

    IEnumerator ChunkSpawnWorker()
    {
        isSpawningChunks = true;

        while (spawnQueue.Count > 0)
        {
            var (chunkTransform, chunkCoord) = spawnQueue.Dequeue();
            yield return SpawnAsteroidsAsync(chunkTransform, chunkCoord);
            yield return new WaitForSeconds(0.1f); // пауза между чанками
        }

        isSpawningChunks = false;
    }

    void Start()
    {
        playerTransform = playerController.transform;
        worldTransform = GameObject.Find("SpaceContainer").transform;
        UpdateCurrentChunk();
        UpdateChunksAround(currentChunk);
        Tick();
        signalBus.Fire(new SignalGameStarted());
    }
    void Update()
    {
        Tick();
    }
    void Tick()
    {
        HandleFloatingOrigin();
        HandleChunks();
    }
    Asteroid SpawnAsteroid(Transform chunkTransform, Vector3Int chunkCoord, Vector3 localOffset)
    {
        Asteroid.Pool pool = GetPool("Ast01");
        Asteroid asteroid = pool.Spawn();
        asteroid.SetPool(pool);
        asteroid.worldChunkManager = this;
        asteroid.chunkCoord = chunkCoord;
        asteroid.chunkTransform = chunkTransform;
        asteroid.transform.SetParent(chunkTransform);
        asteroid.transform.localPosition = localOffset;
        return asteroid;
    }
    void SpawnAsteroids(Transform chunkTransform, Vector3Int chunkCoord)
    {
        Random.InitState($"{chunkCoord}".GetHashCode());
        int count = Random.Range(4, 5);

        for (int i = 0; i < count; i++)
        {
            if (chunkTransform == null)
            {
                break;
            }
            Random.InitState($"{chunkCoord}{i}".GetHashCode());
            Vector3 localOffset = new Vector3(
                Random.Range(-chunkSize / 2f, chunkSize / 2f),
                Random.Range(-chunkSize / 2f, chunkSize / 2f),
                Random.Range(-chunkSize / 2f, chunkSize / 2f)
            );
            int scale = Random.Range(1700, 4000 + 1);
            int rotX = Random.Range(0, 180 + 1);
            int rotY = Random.Range(0, 180 + 1);
            int rotZ = Random.Range(0, 180 + 1);
            Asteroid asteroid = SpawnAsteroid(chunkTransform, chunkCoord, localOffset);
            asteroid.transform.localEulerAngles = new Vector3(rotX, rotY, rotZ);
            asteroid.transform.localScale = new Vector3(scale, scale, scale);
        }
    }
    IEnumerator SpawnAsteroidsAsync(Transform chunkTransform, Vector3Int chunkCoord)
    {
        int count = Random.Range(4, 5);

        for (int i = 0; i < count; i++)
        {
            if (chunkTransform == null)
                yield break;

            Random.InitState($"{chunkCoord}{i}".GetHashCode());
            Vector3 localOffset = new Vector3(
                Random.Range(-chunkSize / 2f, chunkSize / 2f),
                Random.Range(-chunkSize / 2f, chunkSize / 2f),
                Random.Range(-chunkSize / 2f, chunkSize / 2f)
            );

            int scale = Random.Range(1700, 4000 + 1);
            int rotX = Random.Range(0, 180 + 1);
            int rotY = Random.Range(0, 180 + 1);
            int rotZ = Random.Range(0, 180 + 1);
            Asteroid.Pool pool = GetPool("Ast01");
            Asteroid asteroid = SpawnAsteroid(chunkTransform, chunkCoord, localOffset);
            asteroid.transform.localEulerAngles = new Vector3(rotX, rotY, rotZ);
            asteroid.transform.localScale = new Vector3(scale, scale, scale);
            asteroid.transform.localEulerAngles = new Vector3(rotX, rotY, rotZ);

            yield return null; // вместо WaitForSeconds — быстрее и легче
        }
    }

    void HandleFloatingOrigin()
    {
        if (playerTransform.position.magnitude > originThreshold)
        {
            Vector3 offset = playerTransform.position;
            worldTransform.position -= offset;
            playerTransform.position = Vector3.zero;

            Rigidbody rb = playerTransform.GetComponent<Rigidbody>();
            if (rb != null)
                rb.position = Vector3.zero;

            Debug.Log("Floating Origin Shifted by " + offset);
            UpdateCurrentChunk();
        }
    }

    void UpdateCurrentChunk()
    {
        Vector3 globalPosition = -(worldTransform.position - playerTransform.localPosition);
        currentChunk = new Vector3Int(
            Mathf.FloorToInt(globalPosition.x / chunkSize),
            Mathf.FloorToInt(globalPosition.y / chunkSize),
            Mathf.FloorToInt(globalPosition.z / chunkSize)
        );
    }

    void HandleChunks()
    {
        Vector3 globalPosition = -(worldTransform.position - playerTransform.localPosition);

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
                signalBus.Fire(new SignalDestroyChunkAsteroids(kvp.Key));
                kvp.Value.Destroy();
                toRemove.Add(kvp.Key);
            }
        }

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
            chunks.Add(chunk);
        }
        chunk.name = "Chunk_" + chunkCoord;
        chunk.isDestroyed = false;
        chunk.transform.parent = worldTransform;
        chunk.transform.localPosition = (chunkCoord * chunkSize);
        if (isFirstChunksReady)
        {
            EnqueueChunkSpawn(chunk.transform, chunkCoord);
        }
        else
        {
            SpawnAsteroids(chunk.transform, chunkCoord);
        }
        return chunk;
    }

}
