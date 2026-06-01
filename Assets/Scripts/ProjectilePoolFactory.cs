using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Zenject;

public class ProjectilePoolFactory
{
    private Dictionary<string, ProjectilePool> pools = new Dictionary<string, ProjectilePool>();
    private DiContainer container;
    
    public ProjectilePoolFactory(DiContainer container)
    {
        this.container = container;
    }
    
    public ProjectilePool CreatePool(ProjectileConfig config, GameObject prefab)
    {
        if (!pools.ContainsKey(config.name))
        {
            var pool = container.Resolve<ProjectilePool>();
            pools.Add(config.name, pool);
        }
        return pools[config.name];
    }
    
    public ProjectilePool GetPool(string id)
    {
        return pools.ContainsKey(id) ? pools[id] : null;
    }
}
