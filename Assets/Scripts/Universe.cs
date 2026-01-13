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
    }
    public void Build()
    {
        for (int i = 0; i < config.list.Count; i++)
        {
            SpaceConfigListItem it = config.list[i];
            Galaxy galaxy = _galaxyFactory.Create();
            galaxy.transform.SetParent(galaxies);
            galaxy.config = JsonConfigLoader.LoadFromResources<SpaceConfig>($"Configs/Universe/Galaxies/{it.name}");
            int tryes = 10;
            bool br = true;
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
                galaxy.transform.localPosition = pos;
                if (br)
                {
                    break;
                }
            }
            if (!br)
            {
                galaxy.Destroy();
                return;
            }
            galaxiesList.Add(galaxy);
        }
    }
}