using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Zenject;

public class AsteroidPoolInstaller : MonoInstaller
{
    public Asteroid asteroidPrefab;
    public override void InstallBindings()
    {
        // Загружаем конфиги
        List<string> list = FolderLister.GetDirFiles(JsonConfigLoader.ConfigPath + "/" + "AsteroidFields");
        List<AsteroidFieldConfig> configs = new List<AsteroidFieldConfig>();
        for (int i = 0; i < list.Count; i++)
        {
            AsteroidFieldConfig config = JsonConfigLoader.LoadFromFile<AsteroidFieldConfig>("AsteroidFields/" + list[i]);
            
            if (config.asteroids == null)
            {
                return;
            }
            config.name = list[i];
            configs.Add(config);
        }
        Container.Bind<List<AsteroidFieldConfig>>().FromInstance(configs).AsSingle();
        for (int i = 0; i < configs.Count; i++)
        {
            foreach (AsteroidFieldItemConfig cfg in configs[i].asteroids)
            {
                Debug.Log($"Binding Asteroid.Pool with ID: {configs[i].name}_{cfg.name}");
                Container.BindMemoryPool<Asteroid, Asteroid.Pool>()
                    .WithId($"{configs[i].name}_{cfg.name}")
                    .WithInitialSize(cfg.poolSize)
                    .FromComponentInNewPrefab(asteroidPrefab);
            }
        }
    }
}
