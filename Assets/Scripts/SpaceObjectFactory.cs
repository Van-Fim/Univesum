using UnityEngine;
using Zenject;

public class SpaceObjectFactory {
    private readonly DiContainer _container;

    public SpaceObjectFactory(DiContainer container) {
        _container = container;
    }

    public T Create<T>(string prefabPath, string configPath) where T : SpaceObject {
        // Загружаем префаб
        GameObject prefab = Resources.Load<GameObject>(prefabPath);
        GameObject go = _container.InstantiatePrefab(prefab);

        // Получаем компонент
        T obj = go.GetComponent<T>();

        // Загружаем конфиг
        SpaceObjectConfig cfg = JsonConfigLoader.LoadFromFile<SpaceObjectConfig>(configPath);
        obj.InstallConfig(cfg);

        return obj;
    }
}
