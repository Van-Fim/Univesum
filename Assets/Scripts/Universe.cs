using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class Universe
{
    public int seed = 100;
    public SpaceConfig config;
    [Inject] private Galaxy.Factory _galaxyFactory;
    [Inject] private StarSystem.Factory _starSystemFactory;


    public List<AsteroidFieldConfig> _asteroidConfigs;

    public Transform universeMap;
    public Transform galaxies;
    public Transform systems;
    public Transform currentSystem;
    public List<Galaxy> galaxiesList = new List<Galaxy>();
    public List<StarSystem> systemsList = new List<StarSystem>();
    public List<SpaceObject> allSpaceObjects = new List<SpaceObject>();

    public static Universe singleton;
    private int _nextId = 0;
    public List<int> freeIds = new List<int>();

    public int GenerateId()
    {
        if(freeIds.Count > 0)
        {
            _nextId = freeIds[0];
            freeIds.Remove(freeIds[0]);
            return _nextId;
        }
        return _nextId++;
    }

    public void Init()
    {
        GameObject gm = new GameObject();
        universeMap = gm.transform;
        gm = new GameObject();
        galaxies = gm.transform;
        gm = new GameObject();
        systems = gm.transform;
        gm = new GameObject();
        currentSystem = gm.transform;
        galaxies.SetParent(universeMap);
        systems.SetParent(universeMap);
        currentSystem.SetParent(universeMap);
        universeMap.transform.rotation = Quaternion.identity;
        universeMap.transform.localPosition = Vector3.zero;
        universeMap.name = "UniverseMap";
        galaxies.transform.rotation = Quaternion.identity;
        galaxies.transform.localPosition = Vector3.zero;
        galaxies.name = "Galaxies";
        systems.transform.rotation = Quaternion.identity;
        systems.transform.localPosition = Vector3.zero;
        systems.name = "Systems";
        currentSystem.transform.rotation = Quaternion.identity;
        currentSystem.transform.localPosition = Vector3.zero;
        currentSystem.name = "CurrentSystem";
        singleton = this;
    }
    public void Clear()
    {
        NpcJobManager npcJobManager = NpcJobManager.singleton;
        npcJobManager.isEnabled = false;
        npcJobManager.ClearAllJobsAndData();
        PSpace.InvokeDestroyAll();
        SpaceObject.InvokeDestroyAll();
        npcJobManager.isEnabled = true;
        allSpaceObjects = new List<SpaceObject>();
        galaxiesList = new List<Galaxy>();
        systemsList = new List<StarSystem>();
        WorldChunkManager.singleton.Reset();
    }
    public void DistributeGalacticTerritories(Galaxy galaxy)
    {
        // 1. Собираем все системы нужной галактики
        List<StarSystem> allSystems = Universe.singleton.systemsList
            .Where(s => s.galaxyId == galaxy.id)
            .ToList();
        List<FactionConfig> startFactions = galaxy.config.start_factions;
        List<Faction> factions = FactionsManager.singleton.Factions;
        int numFractions = startFactions.Count;

        // Сразу сбрасываем фракции у всех систем в этой галактике (-1 = нейтральная)
        foreach (StarSystem sys in allSystems) sys.faction = null;

        // 2. Выбираем максимально удаленные стартовые точки (столицы)
        List<StarSystem> capitals = new List<StarSystem>();
        for (int f = 0; f < numFractions; f++)
        {
            Faction ff = factions.Find(x=>x.name == startFactions[f].name);
            if (ff == null)
            {
                continue;
            }
            int rnd = Random.Range(startFactions[f].systems_count_min, startFactions[f].systems_count_max + 1);
            startFactions[f].systems_count = rnd;
            startFactions[f].is_started = true;
        }
        for (int f = 0; f < numFractions; f++)
        {
            Faction ff = factions.Find(x=>x.name == startFactions[f].name);
            if (ff == null)
            {
                continue;
            }
            if (f == 0)
            {
                // Первая столица — в случайную систему
                capitals.Add(allSystems[Random.Range(0, allSystems.Count)]);
            }
            else
            {
                // Каждая следующая — максимально далеко от всех уже выбранных столиц
                var bestCapital = allSystems
                    .Where(s => s.faction == null)
                    .OrderByDescending(s => capitals.Min(c => Vector3.Distance(s.transform.position, c.transform.position)))
                    .First();
                capitals.Add(bestCapital);
            }

            capitals[f].faction = factions[f]; // Задаем ID фракции (0, 1, 2, 3, 4)
            startFactions[f].systems_count--; // Уменьшаем лимит систем фракции, так как столица уже занята
        }

        // Подготавливаем списки «фронта экспансии» (граничащие свободные системы)
        List<List<StarSystem>> expansionFronts = new List<List<StarSystem>>();
        for (int f = 0; f < numFractions; f++)
        {
            // На старте фронт фракции — это соседи её столицы
            expansionFronts.Add(new List<StarSystem>(capitals[f].neighbors));
        }

        // 3. Пошаговое круговое расширение
        bool systemsAssignedInThisTurn = true;

        while (systemsAssignedInThisTurn)
        {
            systemsAssignedInThisTurn = false;

            // По очереди даем походить каждой фракции
            for (int f = 0; f < numFractions; f++)
            {
                // Если фракция уже исчерпала свой лимит X систем, пропускаем её
                if (startFactions[f].is_started && startFactions[f].systems_count <= 0) continue;

                // Очищаем фронт от систем, которые уже успел захватить кто-то другой
                expansionFronts[f] = expansionFronts[f].Where(s => s.faction == null).ToList();

                if (expansionFronts[f].Count > 0)
                {
                    // Ищем во фронте систему, которая ближе всего к столице этой фракции.
                    // Это заставляет территорию расти кучно («пятном»), а не сосиской.
                    var targetSystem = expansionFronts[f]
                        .OrderBy(s => Vector3.Distance(s.transform.position, capitals[f].transform.position))
                        .First();

                    // Захватываем систему
                    targetSystem.faction = FactionsManager.singleton.GetFaction(startFactions[f].name);
                    if (targetSystem.mapSpaceUi && targetSystem.faction != null)
                    {
                        targetSystem.mapSpaceUi.SetColor(0, targetSystem.faction.factionConfig.color);
                    }
                    startFactions[f].systems_count--;
                    systemsAssignedInThisTurn = true;

                    // Добавляем соседей захваченной системы в наш фронт расширения
                    foreach (var neighbor in targetSystem.neighbors)
                    {
                        if (neighbor.faction == null && !expansionFronts[f].Contains(neighbor))
                        {
                            expansionFronts[f].Add(neighbor);
                        }
                    }
                }
            }
        }

        // 4. Страховка (на случай, если из-за генерации остались изолированные «ничейные» системы)
        // foreach (var sys in allSystems.Where(s => s.faction == null))
        // {
        //     var closestOccupied = allSystems
        //         .Where(s => s.faction != null)
        //         .OrderBy(s => Vector3.Distance(sys.transform.position, s.transform.position))
        //         .FirstOrDefault();

        //     if (closestOccupied != null) sys.faction = closestOccupied.faction;
        // }
    }
    public void BuildByList(List<SpaceConfig> spaceConfigs)
    {
        for (int c = 0; c < spaceConfigs.Count; c++)
        {
            SpaceConfig sc = spaceConfigs[c];
            if (sc.spaceType != null)
            {
                if (sc.spaceType == "Galaxy")
                {
                    Random.InitState($"{seed}{sc.id}".GetHashCode());
                    Galaxy space = _galaxyFactory.Create();
                    space.transform.SetParent(galaxies);
                    space.transform.localPosition = sc.position;
                    space.transform.localEulerAngles = sc.rotation;
                    space.id = sc.id;
                    galaxiesList.Add(space);
                }
                else if (sc.spaceType == "StarSystem")
                {
                    Random.InitState($"{seed}{sc.id}".GetHashCode());
                    StarSystem space = _starSystemFactory.Create();
                    space.transform.SetParent(systems);
                    space.transform.localPosition = sc.position;
                    space.transform.localEulerAngles = sc.rotation;
                    space.id = sc.id;
                    space.galaxyId = sc.galaxyId;
                    space.asteroidFields = sc.asteroidFieldsConfig;
                    space.config = sc;
                    space.faction = FactionsManager.singleton.GetFaction(sc.faction);
                    systemsList.Add(space);
                }
            }
        }
        for (int i = 0; i < systemsList.Count; i++)
        {
            StarSystem starSystem = systemsList[i];
            starSystem.GetNeighbors();
        }
    }
    public void Build()
    {
        int id = 0;
        for (int i = 0; i < config.list.Count; i++)
        {
            SpaceConfigListItem it = config.list[i];
            int tryes = 10;
            bool br = true;
            int count = Random.Range(it.countMin, it.countMax + 1);
            for (int c = 0; c < count; c++)
            {
                Random.InitState($"{seed}{id}".GetHashCode());
                Galaxy space = _galaxyFactory.Create();
                space.transform.SetParent(galaxies);
                space.config = JsonConfigLoader.LoadFromFile<SpaceConfig>($"Universe/Galaxies/{it.name}");

                for (int t = 0; t < tryes; t++)
                {
                    int range = Random.Range(it.rangeMin, it.rangeMax + 1);
                    int y = Random.Range(it.YMin, it.YMax + 1);
                    Vector2 pos2D = Random.insideUnitCircle * range;
                    Vector3 pos = new Vector3(pos2D.x, y, pos2D.y);

                    float dst = 0;
                    br = true;
                    for (int j = 0; j < galaxiesList.Count; j++)
                    {
                        Galaxy sp = galaxiesList[j];
                        dst = Vector3.Distance(pos, sp.transform.localPosition);
                        if (dst < sp.safeRange)
                        {
                            br = false;
                            break;
                        }
                    }
                    space.transform.localPosition = pos;
                    if (br)
                    {
                        break;
                    }
                }
                if (!br)
                {
                    space.Destroy();
                    continue;
                }
                else
                {
                    space.id = id;
                    galaxiesList.Add(space);
                    space.Build();
                    id++;
                }
            }
        }
        for (int i = 0; i < systemsList.Count; i++)
        {
            StarSystem starSystem = systemsList[i];
            starSystem.GetNeighbors();
        }
        for (int i = 0; i < galaxiesList.Count; i++)
        {
            DistributeGalacticTerritories(galaxiesList[i]);
        }
    }
    public Galaxy FindGalaxy(int galaxyId)
    {
        return galaxiesList.Find(x => x.id == galaxyId);
    }
    public StarSystem FindSystem(int galaxyId, int systemId)
    {
        return systemsList.Find(x => x.galaxyId == galaxyId && x.id == systemId);
    }
}
