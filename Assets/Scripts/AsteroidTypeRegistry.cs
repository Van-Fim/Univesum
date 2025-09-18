using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AsteroidTypeRegistry : MonoBehaviour
{
    public List<AsteroidFieldItemConfig> configs;

    private Dictionary<string, AsteroidFieldItemConfig> configMap;

    public void Init()
    {
        configMap = configs.ToDictionary(c => c.name, c => c);
    }

    public AsteroidFieldItemConfig Get(string id)
    {
        return configMap[id];
    }

    public IEnumerable<AsteroidFieldItemConfig> All => configs;
}

