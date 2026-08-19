using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

[Serializable]
public class Job
{
    public int id;
    public string space_object = "ship01";
    public string name = "police_patrol01";
    public string loadoutName;
    public string task = "follow";
    public string taskParams = "player";
    public string type;
    public int maxUniverseCount = 60;
    public int maxGalaxyCount = 30;
    public int maxStarSystemCount = 10;
    public string faction;
    public bool onOwnerSystem = false;
    public bool onOwnerStation = false;
    public int spawnRangeMin = 100;
    public int spawnRangeMax = 100;
    public int heigthMin = 100;
    public int heigthMax = 100;

    public static UnityAction<SpaceObject> OnJobObjectDestroyedAction;

    // Текущие счетчики
    [NonSerialized] public int currentUniverseCount = 0;
    [NonSerialized] public int currentGalaxyCount = 0;
    [NonSerialized] public int currentStarSystemCount = 0;

    // Словари для отслеживания распределения
    [NonSerialized] public Dictionary<int, int> galaxyCounts = new Dictionary<int, int>();
    [NonSerialized] public Dictionary<int, int> starSystemCounts = new Dictionary<int, int>();

    public void Init()
    {
        OnJobObjectDestroyedAction += OnJobObjectDestroyed;
    }
    public void OnJobObjectDestroyed(SpaceObject spaceObject)
    {

    }
    public static void InvokeJobObjectDestroyed(SpaceObject spaceObject = null)
    {
        OnJobObjectDestroyedAction?.Invoke(spaceObject);
    }
}

public class JobInstance
{
    public int id;
    public Job job;
    public SpaceObject spaceObject;
    public int galaxyId;
    public int systemId;
    public DateTime spawnTime;
}
public class NpcJobManager : IInitializable
{
    [Inject] private Universe _universe;
    [Inject] private DiContainer _container;
    [Inject] private SpaceObjectFactory _spFactory;
    [Inject] private SignalBus _signalBus;

    public bool isEnabled = true;

    private List<Job> _jobs = new List<Job>();
    private Dictionary<int, List<JobInstance>> _activeJobs = new Dictionary<int, List<JobInstance>>();
    private Dictionary<string, Queue<Job>> _pendingJobs = new Dictionary<string, Queue<Job>>();

    private float _spawnCheckInterval = 5f;
    private float _lastSpawnCheck;

    // Настройки
    private int _maxSpawnPerFrame = 3;
    private float _minSpawnDelay = 0.5f;
    private float _maxSpawnDelay = 2f;

    public static NpcJobManager singleton;
    void Start()
    {
        singleton = this;
    }
    public void Initialize()
    {
        singleton = this;
        _signalBus.Subscribe<SignalOnUpdateTick>(OnUpdateTick);
    }
    public void Load()
    {
        LoadJobs();
        _lastSpawnCheck = Time.time;
    }
    public void OnUpdateTick()
    {
        if (!isEnabled)
        {
            return;
        }
        // if (Time.time - _lastSpawnCheck >= _spawnCheckInterval)
        // {
        //     _lastSpawnCheck = Time.time;
        // }
        UpdateJobCounters();
        ProcessJobSpawning();

        CleanupDestroyedShips();
    }
    public void ClearJobs()
    {
        // Удаляем все активные джобы (уничтожаем спавненные объекты)
        foreach (var jobId in _activeJobs.Keys.ToList())
        {
            foreach (var jobInstance in _activeJobs[jobId])
            {
                if (jobInstance.spaceObject != null)
                {
                    // Уничтожаем объект в игре
                    GameObject.Destroy(jobInstance.spaceObject.gameObject);
                }
            }
            _activeJobs[jobId].Clear();
        }

        // Очищаем все очереди ожидающих джобов
        foreach (var jobId in _pendingJobs.Keys.ToList())
        {
            _pendingJobs[jobId].Clear();
        }

        // Сбрасываем все счетчики у джобов
        foreach (var job in _jobs)
        {
            job.currentUniverseCount = 0;
            job.currentGalaxyCount = 0;
            job.currentStarSystemCount = 0;
            job.galaxyCounts.Clear();
            job.starSystemCounts.Clear();
        }

        Debug.Log($"All jobs cleared. Active jobs: {_activeJobs.Values.Sum(list => list.Count)}, Pending jobs: {_pendingJobs.Values.Sum(queue => queue.Count)}");
    }
    public void ClearAllJobsAndData()
    {
        // Сначала удаляем все активные объекты
        ClearJobs();

        // Очищаем все структуры данных
        _jobs.Clear();
        _activeJobs.Clear();
        _pendingJobs.Clear();

        Debug.Log("All job data completely cleared");
    }
    public void LoadJobs()
    {
        _activeJobs = new Dictionary<int, List<JobInstance>>();
        _pendingJobs = new Dictionary<string, Queue<Job>>();
        // Загрузка джобов из ресурсов или конфига
        var jobsArr = JsonConfigLoader.LoadAllFromFolder<Job>("Jobs");

        // Инициализация структур данных
        for (int i = 0; i < jobsArr.Length; i++)
        {
            Job job = jobsArr[i];
            job.id = _jobs.Count;
            job.Init();
            _jobs.Add(job);
            _activeJobs[job.id] = new List<JobInstance>();
            _pendingJobs[job.name] = new Queue<Job>();
        }
    }

    private void UpdateJobCounters()
    {
        // Сброс счетчиков
        foreach (var job in _jobs)
        {
            job.currentUniverseCount = 0;
            job.currentGalaxyCount = 0;
            job.currentStarSystemCount = 0;
            job.galaxyCounts.Clear();
            job.starSystemCounts.Clear();
        }

        // Подсчет активных кораблей
        foreach (var jobInstance in _activeJobs.Values.SelectMany(v => v))
        {
            if (jobInstance.spaceObject != null)
            {
                var job = jobInstance.job;
                job.currentUniverseCount++;
                job.currentGalaxyCount++;

                if (!job.galaxyCounts.ContainsKey(jobInstance.galaxyId))
                    job.galaxyCounts[jobInstance.galaxyId] = 0;
                job.galaxyCounts[jobInstance.galaxyId]++;

                if (!job.starSystemCounts.ContainsKey(jobInstance.systemId))
                    job.starSystemCounts[jobInstance.systemId] = 0;
                job.starSystemCounts[jobInstance.systemId]++;
            }
        }
    }

    private void ProcessJobSpawning()
    {
        int spawnedThisFrame = 0;

        foreach (var job in _jobs)
        {
            if (spawnedThisFrame >= _maxSpawnPerFrame)
                break;

            if (CanSpawnJob(job))
            {
                var spawnLocation = GetSpawnLocation(job);
                if (spawnLocation.HasValue)
                {
                    if (job.type == "station" || job.type == null)
                    {
                        SpawnStationForJob(job, spawnLocation.Value);
                    }
                    else if (job.type == "ship")
                    {
                        SpawnShipForJob(job, spawnLocation.Value);
                    }
                    spawnedThisFrame++;
                }
            }
        }
    }

    private bool CanSpawnJob(Job job)
    {
        // Проверка глобальных лимитов
        if (job.currentUniverseCount >= job.maxUniverseCount)
            return false;

        // Поиск подходящей галактики
        var availableGalaxies = GetAvailableGalaxies(job);
        if (availableGalaxies.Count == 0)
            return false;

        return true;
    }

    private List<Galaxy> GetAvailableGalaxies(Job job)
    {
        return _universe.galaxiesList.Where(galaxy =>
        {
            // Проверка лимита галактики
            int galaxyCount = job.galaxyCounts.GetValueOrDefault(galaxy.id, 0);
            if (galaxyCount >= job.maxGalaxyCount)
                return false;

            // Проверка принадлежности рассы
            // if (job.onOwnerSystem && galaxy.owner != job.faction)
            //     return false;

            return true;
        }).ToList();
    }

    private List<StarSystem> GetAvailableSystems(Job job, int galaxyId)
    {
        List<StarSystem> ret = new List<StarSystem>();

        ret = _universe.systemsList.Where(system =>
            system.galaxyId == galaxyId &&
            (!job.onOwnerSystem || (system.faction != null && system.faction.name == job.faction)) &&
            job.starSystemCounts.GetValueOrDefault(system.id, 0) < job.maxStarSystemCount
        ).ToList();
        return ret;
    }

    private (Galaxy galaxy, StarSystem system)? GetSpawnLocation(Job job)
    {
        var availableGalaxies = GetAvailableGalaxies(job);

        // Выбор случайной галактики с весами (меньше кораблей - выше вес)
        var weightedGalaxies = availableGalaxies.Select(g => new
        {
            Galaxy = g,
            Weight = Mathf.Max(1, job.maxGalaxyCount - job.galaxyCounts.GetValueOrDefault(g.id, 0))
        }).ToList();

        if (weightedGalaxies.Count == 0)
            return null;

        var selectedGalaxy = SelectWeightedRandom(weightedGalaxies.Select(w => w.Galaxy).ToList(),
                                                  weightedGalaxies.Select(w => w.Weight).ToList());

        var availableSystems = GetAvailableSystems(job, selectedGalaxy.id);
        if (availableSystems.Count == 0)
            return null;

        var selectedSystem = availableSystems[UnityEngine.Random.Range(0, availableSystems.Count)];
        return (selectedGalaxy, selectedSystem);
    }

    private T SelectWeightedRandom<T>(List<T> items, List<int> weights)
    {
        int totalWeight = weights.Sum();
        int randomWeight = UnityEngine.Random.Range(0, totalWeight);

        int currentWeight = 0;
        for (int i = 0; i < items.Count; i++)
        {
            currentWeight += weights[i];
            if (randomWeight < currentWeight)
                return items[i];
        }

        return items[0];
    }
    private void SpawnStationForJob(Job job, (Galaxy galaxy, StarSystem system) location)
    {
        Station station = _spFactory.Create<Station>(
            "Prefabs/StationPrefab",
            "SpaceObjects/Stations/" + job.space_object
        );
        int radius = UnityEngine.Random.Range(job.spawnRangeMin, job.spawnRangeMax + 1);
        Vector2 randomPoint2D = UnityEngine.Random.insideUnitCircle * radius;
        int hr = UnityEngine.Random.Range(job.heigthMin, job.heigthMax + 1);
        int y = UnityEngine.Random.Range(-hr, hr + 1);
        Vector3 newPos = new Vector3(randomPoint2D.x, y, randomPoint2D.y);
        Faction faction = FactionsManager.singleton.GetFaction(job.faction);
        station.jobId = job.id;
        station.loadoutName = "Station01_Loadout01";
        station.transform.localPosition = newPos;
        station.transform.localEulerAngles = Vector3.zero;
        station.SetStarSystem(location.galaxy.id, location.system.id);
        station.Init();
        bool inst = station.TryInstallConfig(station._StarSystem);
        station.InstallAi();
        station.StartCommand(job.task, job.taskParams);
        station.InstallLoadout();
        station.SetOwner(faction);
        // Создание экземпляра джоба
        JobInstance jobInstance = new JobInstance
        {
            id = _activeJobs[job.id].Count,
            job = job,
            spaceObject = station,
            galaxyId = location.galaxy.id,
            systemId = location.system.id,
            spawnTime = DateTime.Now
        };
        station.jobInstId = jobInstance.id;
        _activeJobs[job.id].Add(jobInstance);

        // Обновление счетчиков
        job.currentUniverseCount++;
        if (!job.galaxyCounts.ContainsKey(location.galaxy.id))
            job.galaxyCounts[location.galaxy.id] = 0;
        job.galaxyCounts[location.galaxy.id]++;

        if (!job.starSystemCounts.ContainsKey(location.system.id))
            job.starSystemCounts[location.system.id] = 0;
        job.starSystemCounts[location.system.id]++;

        // Debug.Log($"Spawned {job.name} for {job.faction} in galaxy {location.galaxy.id}, system {location.system.id}");
    }
    public void AddStationForJob(Station station)
    {
        Job job = _jobs.Find(x => x.id == station.jobId);
        // Создание экземпляра джоба
        var jobInstance = new JobInstance
        {
            job = job,
            spaceObject = station,
            galaxyId = station.galaxyId,
            systemId = station.systemId,
            spawnTime = DateTime.Now
        };

        _activeJobs[job.id].Add(jobInstance);

        // Обновление счетчиков
        job.currentUniverseCount++;
        if (!job.galaxyCounts.ContainsKey(station.galaxyId))
            job.galaxyCounts[station.galaxyId] = 0;
        job.galaxyCounts[station.galaxyId]++;

        if (!job.starSystemCounts.ContainsKey(station.systemId))
            job.starSystemCounts[station.systemId] = 0;
        job.starSystemCounts[station.systemId]++;

        Debug.Log($"------------ {job.id}");
    }
    public void AddShipForJob(Ship ship)
    {
        Job job = _jobs.Find(x => x.id == ship.jobId);
        // Создание экземпляра джоба
        var jobInstance = new JobInstance
        {
            job = job,
            spaceObject = ship,
            galaxyId = ship.galaxyId,
            systemId = ship.systemId,
            spawnTime = DateTime.Now
        };

        _activeJobs[job.id].Add(jobInstance);

        // Обновление счетчиков
        job.currentUniverseCount++;
        if (!job.galaxyCounts.ContainsKey(ship.galaxyId))
            job.galaxyCounts[ship.galaxyId] = 0;
        job.galaxyCounts[ship.galaxyId]++;

        if (!job.starSystemCounts.ContainsKey(ship.systemId))
            job.starSystemCounts[ship.systemId] = 0;
        job.starSystemCounts[ship.systemId]++;
    }
    private void SpawnShipForJob(Job job, (Galaxy galaxy, StarSystem system) location)
    {
        // Создание корабля через фабрику
        Ship ship = _spFactory.Create<Ship>(
            "Prefabs/ShipPrefab",
            "SpaceObjects/Ships/" + job.space_object
        );
        
        StarSystem sys = PlayerService.singleton.GetStarSystem();
        int radius = UnityEngine.Random.Range(job.spawnRangeMin, job.spawnRangeMax + 1);
        Vector2 randomPoint2D = UnityEngine.Random.insideUnitCircle * radius;
        int hr = UnityEngine.Random.Range(job.heigthMin, job.heigthMax + 1);
        int y = UnityEngine.Random.Range(-hr, hr + 1);
        Vector3 newPos = new Vector3(randomPoint2D.x, y, randomPoint2D.y);
        Faction faction = FactionsManager.singleton.GetFaction(job.faction);
        ship.jobId = job.id;
        ship.loadoutName = job.loadoutName;
        ship.transform.localEulerAngles = Vector3.zero;
        ship.SetStarSystem(location.galaxy.id, location.system.id);
        ship.Init();
        bool inst = ship.TryInstallConfig(ship._StarSystem);
        ship.transform.localPosition = newPos;
        ship.InstallLoadout();
        ship.InstallController();
        ship.InstallAi();
        ship.StartCommand(job.task, job.taskParams);
        ship.BuildLoadouts();
        ship.SetOwner(faction);
        string[] paramsList = job.taskParams.Split(';');
        ship.spaceObjectController.SetMainCommand(job.task, paramsList.ToList());

        // Создание экземпляра джоба
        var jobInstance = new JobInstance
        {
            job = job,
            spaceObject = ship,
            galaxyId = location.galaxy.id,
            systemId = location.system.id,
            spawnTime = DateTime.Now
        };

        _activeJobs[job.id].Add(jobInstance);

        // Обновление счетчиков
        job.currentUniverseCount++;
        if (!job.galaxyCounts.ContainsKey(location.galaxy.id))
            job.galaxyCounts[location.galaxy.id] = 0;
        job.galaxyCounts[location.galaxy.id]++;

        if (!job.starSystemCounts.ContainsKey(location.system.id))
            job.starSystemCounts[location.system.id] = 0;
        job.starSystemCounts[location.system.id]++;
        if (!ship.is_player && ship.systemId == sys.id && ship.galaxyId == sys.galaxyId)
        {
            ship.BuildLoadouts();
        }
        // Debug.Log($"Spawned {job.name} for {job.faction} in galaxy {location.galaxy.id}, system {location.system.id}");
    }

    private void CleanupDestroyedShips()
    {
        foreach (var jobId in _activeJobs.Keys.ToList())
        {
            _activeJobs[jobId].RemoveAll(instance =>
                instance.spaceObject == null);
        }
    }

    public void OnShipDestroyed(Ship ship)
    {
        // Поиск и удаление джоба
        foreach (var jobInstance in _activeJobs.Values.SelectMany(v => v))
        {
            if (jobInstance.spaceObject == ship)
            {
                _activeJobs[jobInstance.job.id].Remove(jobInstance);
                UpdateJobCounters(); // Обновление счетчиков после удаления
                break;
            }
        }
    }

    public int GetJobCountInSystem(int jobId, int systemId)
    {
        var job = _jobs.FirstOrDefault(j => j.id == jobId);
        if (job == null) return 0;

        return job.starSystemCounts.GetValueOrDefault(systemId, 0);
    }
}
