using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class AsteroidPoolInstaller : MonoInstaller
{
    public List<AsteroidConfig> asteroidConfigs;

    public override void InstallBindings()
    {
        asteroidConfigs = Resources.LoadAll<AsteroidConfig>("Configs/Asteroids").ToList();
        foreach (AsteroidConfig config in asteroidConfigs)
        {
            Debug.Log($"Binding Asteroid.Pool with ID: {config.id}");
            Container.BindMemoryPool<Asteroid, Asteroid.Pool>()
                .WithId(config.id)
                .WithInitialSize(config.poolSize)
                .FromComponentInNewPrefab(config.prefab)
                .UnderTransformGroup("Asteroids");
        }
    }
}
