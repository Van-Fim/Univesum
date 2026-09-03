using UnityEngine;
using Zenject;

public class SpaceObjectFactory
{
    private readonly DiContainer _container;
    private readonly Universe _universe;

    public SpaceObjectFactory(DiContainer container, Universe universe)
    {
        _container = container;
        _universe = universe;
    }

    public T Create<T>(string prefabPath, string configPath = null) where T : SpaceObject
    {
        // Загружаем префаб
        GameObject prefab = Resources.Load<GameObject>(prefabPath);
        GameObject go = _container.InstantiatePrefab(prefab);

        // Получаем компонент
        T obj = go.GetComponent<T>();

        if (configPath != null)
        {
            // Загружаем конфиг
            obj.spaceObjectConfig = JsonConfigLoader.LoadFromFile<SpaceObjectConfig>(configPath);
        }
        obj.id = obj.GetId();
        if (!obj.mapObject && obj.id != 0)
        {
            if (obj is Station)
            {
                obj.mapObject = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/StationMapPrefab"));
                //obj.mapObject.transform.SetParent(obj.transform);
                obj.mapObject.transform.position = obj.transform.position / SpaceObject.scaleFactor;
                obj.mapObject.name = "Station " + obj.id;
                obj.mapObject.transform.localScale = Vector3.one * 10;
                obj.mapObject.layer = 6;
            }
            else if (obj is Ship)
            {
                obj.mapObject = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/ShipMapPrefab"));
                //obj.mapObject.transform.SetParent(obj.transform);
                obj.mapObject.transform.position = obj.transform.position / SpaceObject.scaleFactor;
                obj.mapObject.name = "Ship " + obj.id;
                obj.mapObject.transform.localScale = Vector3.one * 5;
                obj.mapObject.layer = 6;
            }
        }
        _universe.allSpaceObjects.Add(obj);
        return obj;
    }
}
