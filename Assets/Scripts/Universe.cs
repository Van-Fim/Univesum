using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class Universe
{
    public int seed = 100;
    public SpaceConfig config;
    [Inject] private Galaxy.Factory _galaxyFactory;
    [Inject] private StarSystem.Factory _starSystemFactory;

    public Transform universeMap;
    public Transform galaxies;
    public Transform systems;
    public List<Galaxy> galaxiesList = new List<Galaxy>();
    public List<StarSystem> systemsList = new List<StarSystem>();
    public List<SpaceObject> allSpaceObjects = new List<SpaceObject>();
    public static Universe singleton;
    public void Init()
    {
        GameObject gm = new GameObject();
        universeMap = gm.transform;
        gm = new GameObject();
        galaxies = gm.transform;
        gm = new GameObject();
        systems = gm.transform;
        galaxies.SetParent(universeMap);
        systems.SetParent(universeMap);
        universeMap.transform.rotation = Quaternion.identity;
        universeMap.transform.localPosition = Vector3.zero;
        universeMap.name = "UniverseMap";
        galaxies.transform.rotation = Quaternion.identity;
        galaxies.transform.localPosition = Vector3.zero;
        galaxies.name = "Galaxies";
        systems.transform.rotation = Quaternion.identity;
        systems.transform.localPosition = Vector3.zero;
        systems.name = "Systems";
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
                Random.InitState(seed + id);
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