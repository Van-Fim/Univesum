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
        _universe.allSpaceObjects.Add(obj);

        return obj;
    }
}
