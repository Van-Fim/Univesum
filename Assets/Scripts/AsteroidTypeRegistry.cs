using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AsteroidTypeRegistry : MonoBehaviour
{
    public List<AsteroidConfig> configs;

    private Dictionary<string, AsteroidConfig> configMap;

    public void Init()
    {
        configMap = configs.ToDictionary(c => c.id, c => c);
    }

    public AsteroidConfig Get(string id)
    {
        return configMap[id];
    }

    public IEnumerable<AsteroidConfig> All => configs;
}

