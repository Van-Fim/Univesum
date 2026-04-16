using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class StarSystem : PSpace
{
    public int galaxyId;
    public List<AsteroidFieldConfig> asteroidFields = new List<AsteroidFieldConfig>();
    public class Factory : PlaceholderFactory<StarSystem> { }

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
