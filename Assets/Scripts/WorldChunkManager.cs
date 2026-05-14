using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Zenject;

public class WorldChunkManager : MonoBehaviour
{
    List<Chunk> chunks = new List<Chunk>();
    [Inject] NpcJobManager _npcJobManager;
    [Inject] Universe _universe;
    [Inject] SignalBus signalBus;
    [Inject] PlayerService playerService;
    public Vector3 worldPos = Vector3.zero;

    [Inject] SpaceContainer spaceContainer;

    public Transform playerTransform;
    public float originThreshold = 25000f;
    public int chunkSize = 20000;

    public Vector3Int currentChunk;
    public Dictionary<Vector3Int, Chunk> loadedChunks = new();
    Queue<(Chunk, Vector3Int)> spawnQueue = new();
    bool isSpawningChunks = false;
    public bool isFirstChunksReady = false;
    public bool is_initialized;
    public bool stopWorker;
    public bool noAsteroids;
    [Inject] DiContainer container;

    public static WorldChunkManager singleton;

    Asteroid.Pool GetPool(string id)
    {
        return container.ResolveId<Asteroid.Pool>(id);
    }
    public void Reset()
    {
        ClearChunks();
        is_initialized = false;
        worldPos = Vector3.zero;
        currentChunk = Vector3Int.zero;
        stopWorker = true;
        StopAllCoroutines();
        chunks = new List<Chunk>();
        loadedChunks = new();
        spawnQueue = new();
        isSpawningChunks = false;
        isFirstChunksReady = false;
        currentChunk = new Vector3Int(0, 0, 0);
        spaceContainer.transform.localPosition = Vector3.zero;
    }
    public void ClearChunks()
    {
        // Останавливаем все процессы спавна
        stopWorker = true;
        if (isSpawningChunks)
        {
            StopAllCoroutines();
            isSpawningChunks = false;
        }

        // Очищаем очередь спавна
        spawnQueue?.Clear();

        // Уничтожаем все загруженные чанки
        if (loadedChunks != null && loadedChunks.Count > 0)
        {
            foreach (var kvp in loadedChunks)
            {
                Chunk chunk = kvp.Value;
                if (chunk != null)
                {
                    // Уничтожаем все астероиды в чанке
                    if (chunk.asteroids != null)
                    {
                        for (int i = chunk.asteroids.Count - 1; i >= 0; i--)
                        {
                            Asteroid asteroid = chunk.asteroids[i];
                            if (asteroid != null)
                            {
                                if (asteroid.gameObject != null)
                                {
                                    asteroid.Despawn();
                                }
                            }
                        }
                        chunk.asteroids.Clear();
                    }

                    // Уничтожаем GameObject чанка
                    if (chunk.gameObject != null)
                    {
                        Destroy(chunk.gameObject);
                    }
                }
            }

            loadedChunks.Clear();
        }

        // Очищаем список всех чанков
        if (chunks != null)
        {
            chunks.Clear();
        }

        // Сбрасываем флаги
        isFirstChunksReady = false;
        is_initialized = false;
        currentChunk = Vector3Int.zero;

        Debug.Log("WorldChunkManager: All chunks cleared successfully");
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
            if (stopWorker)
            {
                stopWorker = false;
                break;
            }
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
        signalBus.Subscribe<SignalOnUpdateTick>(OnUpdateTick);
        Init();
        isFirstChunksReady = false;
        UpdateCurrentChunk();
        UpdateChunksAround(currentChunk);
        isFirstChunksReady = true;
    }
    public void Init()
    {
        is_initialized = false;
        worldPos = Vector3.zero;
        currentChunk = Vector3Int.zero;
        stopWorker = true;
        StopAllCoroutines();
        chunks = new List<Chunk>();
        loadedChunks = new();
        spawnQueue = new();
        isSpawningChunks = false;
        isFirstChunksReady = false;
        SpaceObjectController spc = playerService._player.GetCurrentController();
        if (spc)
        {
            playerTransform = spc.transform;
        }
        else
        {
            playerTransform = transform;
        }
        is_initialized = true;
        signalBus.Fire(new SignalChunkManagerReady());
        singleton = this;
    }
    void OnUpdateTick()
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
        asteroid.spawnId = $"{config.name}_{astItem.name}";
        asteroid.maxShield = 0;
        asteroid.Init();
        asteroid.Hide();
        asteroid.spaceObjectConfig = JsonConfigLoader.LoadFromFile<SpaceObjectConfig>(astItem.spaceObjectPath);
        asteroid.InstallConfig();
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
        if (chunk.isHidden || chunk.asteroidFieldsIds.Count == 0)
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
        if (chunk.isHidden || chunk.asteroidFieldsIds.Count == 0)
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
        if (!playerTransform)
        {
            return;
        }

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

    public void UpdateCurrentChunk()
    {
        Vector3 globalPosition = -(worldPos - playerTransform.localPosition);
        signalBus.Fire(new SignalChunkFloatingOriginFixStart(globalPosition));
        currentChunk = new Vector3Int(
            Mathf.FloorToInt(globalPosition.x / chunkSize),
            Mathf.FloorToInt(globalPosition.y / chunkSize),
            Mathf.FloorToInt(globalPosition.z / chunkSize)
        );
        spaceContainer.transform.localPosition = -globalPosition;
        signalBus.Fire(new SignalChunkFloatingOriginFixEnd(globalPosition));
    }

    void HandleChunks()
    {
        if (!playerTransform)
        {
            return;
        }
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

    public void UpdateChunksAround(Vector3Int centerChunk)
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
                kvp.Value.Hide();
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
        Chunk chunk = chunks.Find(x => x.isHidden == true);

        if (chunk == null)
        {
            chunk = new GameObject().AddComponent<Chunk>();
            container.Inject(chunk);
            chunks.Add(chunk);
        }
        chunk.name = "Chunk_" + chunkCoord;
        chunk.isHidden = false;
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
            bool check = true;
            check = CheckChunkOnAsteroidFieldOnPosition(chunkCoord * chunkSize, asteroidField.position, asteroidField.shapeSize.x, asteroidField.shapeSize.z, asteroidField.shapeSize.y);
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
            if (noAsteroids)
            {
                return chunk;
            }
            EnqueueChunkSpawn(chunk, chunkCoord);
        }
        else
        {
            if (noAsteroids)
            {
                return chunk;
            }
            SpawnAsteroids(chunk, chunkCoord);
        }
        return chunk;
    }

}
