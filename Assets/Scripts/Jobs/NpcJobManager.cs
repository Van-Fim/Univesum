using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

[Serializable]
public class Job
{
    public int id;
    public string ship = "ship01";
    public string name = "police_patrol01";
    public string task = "patrol_sector";
    public int maxUniverseCount = 60;
    public int maxGalaxyCount = 30;
    public int maxStarSystemCount = 10;
    public string owner = "paranid";
    public bool onOwnerSystem = false;
    public bool onOwnerStation = false;

    // Текущие счетчики
    [NonSerialized] public int currentUniverseCount = 0;
    [NonSerialized] public int currentGalaxyCount = 0;
    [NonSerialized] public int currentStarSystemCount = 0;

    // Словари для отслеживания распределения
    [NonSerialized] public Dictionary<int, int> galaxyCounts = new Dictionary<int, int>();
    [NonSerialized] public Dictionary<int, int> starSystemCounts = new Dictionary<int, int>();
}

public class JobInstance
{
    public Job job;
    public Ship ship;
    public int galaxyId;
    public int systemId;
    public DateTime spawnTime;
}
public class NpcJobManager : ITickable, IInitializable
{
    [Inject] private Universe _universe;
    [Inject] private DiContainer _container;
    [Inject] private SpaceObjectFactory _shipFactory;

    private List<Job> _jobs = new List<Job>();
    private Dictionary<int, List<JobInstance>> _activeJobs = new Dictionary<int, List<JobInstance>>();
    private Dictionary<string, Queue<Job>> _pendingJobs = new Dictionary<string, Queue<Job>>();

    private float _spawnCheckInterval = 5f;
    private float _lastSpawnCheck;

    // Настройки
    private int _maxSpawnPerFrame = 3;
    private float _minSpawnDelay = 0.5f;
    private float _maxSpawnDelay = 2f;

    public void Initialize()
    {
        LoadJobs();
        _lastSpawnCheck = Time.time;
    }

    public void Tick()
    {
        if (Time.time - _lastSpawnCheck >= _spawnCheckInterval)
        {
            UpdateJobCounters();
            ProcessJobSpawning();
            _lastSpawnCheck = Time.time;
        }

        CleanupDestroyedShips();
    }

    private void LoadJobs()
    {
        // Загрузка джобов из ресурсов или конфига
        var jobsArr = JsonConfigLoader.LoadAllFromFolder<Job>("Jobs");

        // Инициализация структур данных
        for (int i = 0; i < jobsArr.Length; i++)
        {
            Job job = jobsArr[i];
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
            if (jobInstance.ship != null && jobInstance.ship.gameObject.activeInHierarchy)
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
                    SpawnShipForJob(job, spawnLocation.Value);
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
            if (job.onOwnerSystem && galaxy.owner != job.owner)
                return false;

            return true;
        }).ToList();
    }

    private List<StarSystem> GetAvailableSystems(Job job, int galaxyId)
    {
        return _universe.systemsList.Where(system =>
            system.galaxyId == galaxyId &&
            (!job.onOwnerSystem || system.owner == job.owner) &&
            job.starSystemCounts.GetValueOrDefault(system.id, 0) < job.maxStarSystemCount
        ).ToList();
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

    private void SpawnShipForJob(Job job, (Galaxy galaxy, StarSystem system) location)
    {
        // Создание корабля через фабрику
        Ship ship = _shipFactory.Create<Ship>(
            "Prefabs/ShipPrefab",
            "SpaceObjects/Ships/" + job.ship
        );
        var loadout = JsonConfigLoader.LoadFromFile<Loadout>(
                        "Loadouts/Ship01_Loadout01"
                    );
        ship.InstallLoadout(loadout);
        ship.transform.localPosition = Vector3.zero;
        ship.transform.localEulerAngles = Vector3.zero;
        ship.SetStarSystem(location.galaxy.id, location.system.id);
        ship.Init();
        Debug.Log($"{ship} {location.galaxy.id} {location.system.id}");
        // Создание экземпляра джоба
        var jobInstance = new JobInstance
        {
            job = job,
            ship = ship,
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

        Debug.Log($"Spawned {job.name} for {job.owner} in galaxy {location.galaxy.id}, system {location.system.id}");
    }

    private void CleanupDestroyedShips()
    {
        foreach (var jobId in _activeJobs.Keys.ToList())
        {
            _activeJobs[jobId].RemoveAll(instance =>
                instance.ship == null || !instance.ship.gameObject.activeInHierarchy);
        }
    }

    public void OnShipDestroyed(Ship ship)
    {
        // Поиск и удаление джоба
        foreach (var jobInstance in _activeJobs.Values.SelectMany(v => v))
        {
            if (jobInstance.ship == ship)
            {
                _activeJobs[jobInstance.job.id].Remove(jobInstance);
                UpdateJobCounters(); // Обновление счетчиков после удаления
                break;
            }
        }
    }

    public List<Ship> GetActiveShipsForJob(string jobName)
    {
        var job = _jobs.FirstOrDefault(j => j.name == jobName);
        if (job == null) return new List<Ship>();

        return _activeJobs[job.id].Select(ji => ji.ship).Where(s => s != null).ToList();
    }

    public int GetJobCountInSystem(int jobId, int systemId)
    {
        var job = _jobs.FirstOrDefault(j => j.id == jobId);
        if (job == null) return 0;

        return job.starSystemCounts.GetValueOrDefault(systemId, 0);
    }
}