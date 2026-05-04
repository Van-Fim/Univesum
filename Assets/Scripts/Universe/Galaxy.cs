using UnityEngine;
using Zenject;

public class Galaxy : PSpace
{
    public class Factory : PlaceholderFactory<Galaxy> { }
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
                Random.InitState(_universe.seed + this.id + id);
                StarSystem space = _starSystemFactory.Create();
                space.transform.SetParent(_universe.systems);
                space.config = JsonConfigLoader.LoadFromFile<SpaceConfig>($"Universe/Systems/{it.name}");

                for (int t = 0; t < tryes; t++)
                {
                    int range = Random.Range(it.rangeMin, it.rangeMax + 1);
                    int y = Random.Range(it.YMin, it.YMax + 1);
                    Vector2 pos2D = Random.insideUnitCircle * range;
                    Vector3 pos = new Vector3(pos2D.x, y, pos2D.y);

                    float dst = 0;
                    br = true;
                    for (int j = 0; j < _universe.systemsList.Count; j++)
                    {
                        StarSystem sp = _universe.systemsList[j];
                        dst = Vector3.Distance(pos, sp.transform.localPosition);
                        if (sp.galaxyId == this.id && dst < sp.safeRange)
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
                    space.galaxyId = this.id;
                    space.faction = _factionsManager.GetFaction(space.config.faction);
                    space.LoadAsteroidFields();
                    _universe.systemsList.Add(space);
                    id++;
                }
            }
        }
    }
}
