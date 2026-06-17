using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using UnityEngine;
using Zenject;

public class StarSystem : PSpace
{
    public int galaxyId;
    public List<Ship> ships = new List<Ship>();
    public List<Station> stations = new List<Station>();
    public List<AsteroidFieldConfig> asteroidFields = new List<AsteroidFieldConfig>();
    public List<StarSystem> neighbors = new List<StarSystem>();
    public class Factory : PlaceholderFactory<StarSystem> { }
    public override void Save()
    {
        base.Save();
        config.galaxyId = galaxyId;
        config.systemId = id;
        config.asteroidFieldsConfig = asteroidFields;
        SaveManager.singleton.spaceConfigs.Add(config);
    }
    public override void OnMinimapRender(System.Type type)
    {
        base.OnMinimapRender(type);
        StarSystem psys = _playerService.GetStarSystem();
        if (canShow)
        {
            if (mapSpaceUi)
            {
                if (psys != null && psys.galaxyId == galaxyId)
                {
                    mapSpaceUi.gameObject.SetActive(true);
                    gameObject.SetActive(true);
                }
                else
                {
                    mapSpaceUi.gameObject.SetActive(false);
                    gameObject.SetActive(false);
                }
            }
        }
        else
        {
            if (mapSpaceUi)
            {
                mapSpaceUi.gameObject.SetActive(false);
                gameObject.SetActive(false);
            }
        }
    }
    public List<StarSystem> GetNeighbors()
    {
        // Очищаем старых соседей перед новым расчетом
        neighbors.Clear();

        List<StarSystem> allValidSystems = new List<StarSystem>();

        // 1. Фильтруем системы: только из нашей галактики и не мы сами
        for (int i = 0; i < Universe.singleton.systemsList.Count; i++)
        {
            StarSystem sys = Universe.singleton.systemsList[i];

            if (sys.galaxyId != galaxyId || sys == this)
                continue;

            allValidSystems.Add(sys);
        }

        // 2. Сортируем отфильтрованные системы по расстоянию до этой системы (this)
        // и берем, например, 4 самые близкие.

        neighbors = allValidSystems
            .OrderBy(sys => Vector3.Distance(this.transform.position, sys.transform.position))
            .Take(config.maxNeighborsCount)
            .ToList();
        return neighbors;
    }
    public void LoadAsteroidFields()
    {
        if (config.asteroidFields.Count == 0)
        {
            return;
        }
        for (int i = 0; i < config.asteroidFields.Count; i++)
        {
            SpaceAsteroidFieldListItem a = config.asteroidFields[i];
            int count = Random.Range(a.countMin, a.countMax + 1);
            for (int j = 0; j < count; j++)
            {
                AsteroidFieldConfig cfg = null;
                int range = Random.Range(a.rangeMin, a.rangeMax + 1);
                int y = Random.Range(a.YMin, a.YMax + 1);
                Vector2 pos2d = Random.insideUnitSphere * range;
                Vector3 position = new Vector3(pos2d.x, y, pos2d.y);
                cfg = (AsteroidFieldConfig)_asteroidConfigs.Find(x => x.name == a.name).Clone();
                cfg.countMin = a.countMin;
                cfg.countMax = a.countMax;
                cfg.shapeSize = a.shapeSize;
                cfg.position = position;
                asteroidFields.Add(cfg);
            }
        }
    }
}
