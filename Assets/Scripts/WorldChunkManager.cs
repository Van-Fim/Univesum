using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class WorldChunkManager : MonoBehaviour
{
    List<Chunk> chunks = new List<Chunk>();
    [Inject] Universe _universe;
    [Inject] SignalBus signalBus;
    [Inject] PlayerService playerService;
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
    bool IsInsideOval(Vector3 point, Vector3 center, float radiusX, float radiusZ, float height)
    {
        // переводим точку в локальные координаты относительно центра
        float dx = point.x - center.x;
        float dz = point.z - center.z;
        float dy = point.y - center.y;

        // проверка эллипса
        float ellipse = (dx * dx) / (radiusX * radiusX) + (dz * dz) / (radiusZ * radiusZ);

        bool insideEllipse = ellipse <= 1f;
        bool insideHeight = Mathf.Abs(dy) <= height * 0.5f;

        return insideEllipse && insideHeight;
    }

    public void Start()
    {
        is_initialized = true;
        playerTransform = playerService._player.GetCurrentController().transform;
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
    Vector3 GetRandomPointInEllipsoid(float radiusX, float radiusZ, float height, Vector3 center)
    {
        // Генерируем случайную точку в единичной сфере
        Vector3 randomSpherePoint = Random.insideUnitSphere;

        // Масштабируем координаты по осям в соответствии с радиусами эллипсоида
        // Умножаем каждую компоненту на соответствующий радиус
        randomSpherePoint.x *= radiusX;
        randomSpherePoint.z *= radiusZ;
        randomSpherePoint.y *= height;

        // Смещаем точку в нужную позицию
        randomSpherePoint += center;

        return randomSpherePoint;
    }
    bool CheckChunkOnAsteroidFieldOnPosition(Vector3 chunkPos,
                                             Vector3 fieldCenter, float radiusX, float radiusZ, float height)
    {
        // переводим в локальные координаты относительно центра поля
        Vector3 local = chunkPos - fieldCenter;

        // проверка эллипса (по XZ)
        float ellipse = (local.x * local.x) / (radiusX * radiusX)
                      + (local.z * local.z) / (radiusZ * radiusZ);

        bool insideEllipse = ellipse <= 1f;

        // проверка высоты (по Y)
        bool insideHeight = Mathf.Abs(local.y) <= height * 0.5f;

        // если центр чанка внутри овала и по высоте — значит чанк пересекает поле
        if (insideEllipse && insideHeight)
            return true;

        // 🔧 Дополнительно: можно проверить углы чанка
        Vector3 half = new Vector3(chunkSize * 0.5f, 0, chunkSize * 0.5f);
        Vector3[] corners =
        {
        chunkPos + new Vector3( half.x, 0,  half.z),
        chunkPos + new Vector3(-half.x, 0,  half.z),
        chunkPos + new Vector3( half.x, 0, -half.z),
        chunkPos + new Vector3(-half.x, 0, -half.z),
    };

        foreach (var c in corners)
        {
            Vector3 lc = c - fieldCenter;
            float e = (lc.x * lc.x) / (radiusX * radiusX) + (lc.z * lc.z) / (radiusZ * radiusZ);
            if (e <= 1f && Mathf.Abs(lc.y) <= height * 0.5f)
                return true; // хотя бы угол попал внутрь
        }

        return false;
    }

    Asteroid SpawnAsteroid(Chunk chunk, Vector3Int chunkCoord, AsteroidFieldConfig config)
    {
        if (config == null)
        {
            return null;
        }

        int rand = Random.Range(0, config.asteroids.Count);
        AsteroidFieldItemConfig astItem = config.asteroids[rand];
        if (astItem == null)
        {
            return null;
        }
        float scale = Random.Range(astItem.scaleMin, astItem.scaleMax);

        if (config.speedThresholds != null && config.speedThresholds.Count > 0)
        {
            float currentSpeed = playerService._player.GetCurrentController()._rigidbody.linearVelocity.magnitude;
            config.speedThresholds.Sort((a, b) => a.speed.CompareTo(b.speed));
            int ascIndex = 0;
            AsteroidSpeedThresholdsConfig asc = null;
            for (int i = 0; i < config.speedThresholds.Count; i++)
            {
                asc = config.speedThresholds[i];
                if (asc.speed <= currentSpeed)
                {
                    ascIndex = i;
                }
            }
            asc = config.speedThresholds[ascIndex];
            if (scale < asc.scale && currentSpeed >= asc.speed)
            {
                return null;
            }
        }

        Vector3 localOffset = new Vector3(
    Random.Range(-chunkSize / 2f, chunkSize / 2f),
    Random.Range(-chunkSize / 2f, chunkSize / 2f),
    Random.Range(-chunkSize / 2f, chunkSize / 2f)
);

        int rotX = Random.Range(0, 180 + 1);
        int rotY = Random.Range(0, 180 + 1);
        int rotZ = Random.Range(0, 180 + 1);
        Asteroid.Pool pool = GetPool($"{config.name}_{astItem.name}");
        Asteroid asteroid = pool.Spawn();
        asteroid.maxShield = 0;
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
        if (chunk.isDestroyed || chunk.asteroidFieldsIds.Count == 0)
        {
            return;
        }
        StarSystem starSystem = playerService.GetStarSystem();
        Random.InitState($"{_universe.seed}{starSystem.galaxyId}{starSystem.id}{chunkCoord}".GetHashCode());
        
        for (int i1 = 0; i1 < chunk.asteroidFieldsIds.Count; i1++)
        {
            int id = chunk.asteroidFieldsIds[i1];
            AsteroidFieldConfig a = starSystem.asteroidFields[id];
            int count = Random.Range(a.countMin, a.countMax + 1);
            for (int i = 0; i < count; i++)
            {
                if (chunk == null)
                {
                    break;
                }
                if (chunk.asteroids.Count > i && chunk.asteroids[i] != null)
                {
                    break;
                }
                Random.InitState($"{_universe.seed}{starSystem.galaxyId}{starSystem.id}{chunkCoord}{i}".GetHashCode());

                Asteroid asteroid = SpawnAsteroid(chunk, chunkCoord, a);
                chunk.asteroids.Add(asteroid);
            }
        }
    }
    IEnumerator SpawnAsteroidsAsync(Chunk chunk, Vector3Int chunkCoord)
    {
        if (chunk.isDestroyed || chunk.asteroidFieldsIds.Count == 0)
        {
            yield return null;
        }
        StarSystem starSystem = playerService.GetStarSystem();
        Random.InitState($"{_universe.seed}{starSystem.galaxyId}{starSystem.id}{chunkCoord}".GetHashCode());
        for (int i1 = 0; i1 < chunk.asteroidFieldsIds.Count; i1++)
        {
            int id = chunk.asteroidFieldsIds[i1];
            AsteroidFieldConfig a = starSystem.asteroidFields[id];
            int count = Random.Range(a.countMin, a.countMax + 1);

            for (int i = 0; i < count; i++)
            {
                if (chunk == null)
                    yield break;
                if (chunk.asteroids.Count > i && chunk.asteroids[i] != null)
                {
                    yield break;
                }
                Random.InitState($"{_universe.seed}{starSystem.galaxyId}{starSystem.id}{chunkCoord}{i}".GetHashCode());
                Asteroid asteroid = SpawnAsteroid(chunk, chunkCoord, a);
                chunk.asteroids.Add(asteroid);
                yield return null; // вместо WaitForSeconds — быстрее и легче
            }
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
        int loadRadius = 5; // сколько чанков вокруг загружать

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
        StarSystem sys = playerService.GetStarSystem();
        if (sys == null)
        {
            return null;
        }
        chunk.asteroidFieldsIds = new List<int>();
        for (int i = 0; i < sys.asteroidFields.Count; i++)
        {
            AsteroidFieldConfig asteroidField = sys.asteroidFields[i];
            bool check = CheckChunkOnAsteroidFieldOnPosition(chunkCoord * chunkSize, asteroidField.position, asteroidField.shapeSize.x, asteroidField.shapeSize.z, asteroidField.shapeSize.y);
            if (check)
            {
                if (!chunk.asteroidFieldsIds.Contains(i))
                {
                    chunk.asteroidFieldsIds.Add(i);
                }
            }
        }
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
