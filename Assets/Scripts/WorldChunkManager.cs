using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class WorldChunkManager : MonoBehaviour
{
    [Inject] public PlayerController playerController;
    [Inject] Asteroid.Pool asteroidPool;
    [Inject] SignalBus signalBus;

    public Transform playerTransform;
    public Transform worldTransform; // THEWORLD
    public float originThreshold = 10000f;
    public int chunkSize = 1000;

    bool offTest = true;

    public Vector3Int currentChunk;
    public Dictionary<Vector3Int, GameObject> loadedChunks = new();
    Queue<(Transform, Vector3Int)> spawnQueue = new();
    bool isSpawningChunks = false;
    public bool isFirstChunksReady = false;

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
            yield return null; // пауза между чанками
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
    void SpawnAsteroids(Transform chunkTransform, Vector3Int chunkCoord)
    {
        Random.InitState($"{chunkCoord}".GetHashCode());
        int count = Random.Range(2, 3);

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

            Asteroid asteroid = asteroidPool.Spawn();
            asteroid.worldChunkManager = this;
            asteroid.chunkCoord = chunkCoord;
            asteroid.transform.SetParent(chunkTransform);
            asteroid.transform.localPosition = localOffset;
        }
    }
    IEnumerator SpawnAsteroidsAsync(Transform chunkTransform, Vector3Int chunkCoord)
    {
        Random.InitState($"{chunkCoord}".GetHashCode());
        int count = Random.Range(5, 6);

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

            Asteroid asteroid = asteroidPool.Spawn();
            asteroid.worldChunkManager = this;
            asteroid.chunkCoord = chunkCoord;
            asteroid.transform.SetParent(chunkTransform);
            asteroid.transform.localPosition = localOffset;

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
        int loadRadius = 2; // сколько чанков вокруг загружать

        HashSet<Vector3Int> requiredChunks = new();

        for (int x = -loadRadius; x <= loadRadius; x++)
            for (int y = -loadRadius; y <= loadRadius; y++)
                for (int z = -loadRadius; z <= loadRadius; z++)
                {
                    Vector3Int chunkCoord = centerChunk + new Vector3Int(x, y, z);
                    requiredChunks.Add(chunkCoord);

                    if (!loadedChunks.ContainsKey(chunkCoord))
                    {
                        GameObject chunk = GenerateChunk(chunkCoord);
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
                Destroy(kvp.Value);
                toRemove.Add(kvp.Key);
            }
        }

        foreach (var key in toRemove)
        {
            loadedChunks.Remove(key);
        }
    }

    GameObject GenerateChunk(Vector3Int chunkCoord)
    {
        GameObject chunk = new GameObject("Chunk_" + chunkCoord);
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
